using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class CharacterIdentity : NetworkBehaviour
{
	private int _characterId;
	private bool _ai;
	private NetworkVariable<int> _pair = new( -1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server );
	private NetworkVariable<Team> _team = new(Team.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server );
	private NetworkVariable<int> _colorIndex = new( -1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server );
	private static readonly Color [] S_PALETTE = {
		Color.blue,
		Color.red,
		Color.green,
		new Color( 1f, 0.5f, 0f )
	};

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		_colorIndex.OnValueChanged += OnColorIndexChanged;
		ApplyColorIndex( _colorIndex.Value );
		if ( IsOwner )
		{
			RequestPlayerNameServerRPC();
		}
	}
	public override void OnNetworkDespawn()
	{
		_colorIndex.OnValueChanged -= OnColorIndexChanged;
	}
	public Team GetTeam()
	{
		return _team.Value;
	}
	public void SetTeam( Team newTeam )
	{
		if ( IsServer )
		{
		_team.Value = newTeam;
		}
	}
	public int GetPair()
	{
		return _pair.Value;
	}
	public void SetPair( int newPair )
	{
		if ( IsServer )
		{
		_pair.Value = newPair;
		}
	}
	[ClientRpc]
	private void SendPlayerNameClientRpc( string newName )
	{
		gameObject.name = newName;
	}
	[ServerRpc(RequireOwnership = false)]
	private void RequestPlayerNameServerRPC()
	{
		SendPlayerNameClientRpc( name );
	}
	public void SetColorIndexOnServer( int index )
	{
		if ( IsServer )
		{
			_colorIndex.Value = Mathf.Clamp( index, 0, S_PALETTE.Length - 1 );
		}
	}
	private void OnColorIndexChanged( int oldIndex, int newIndex )
	{
		ApplyColorIndex( newIndex );
	}
	private void ApplyColorIndex( int index )
	{
		if ( index < 0 || index >= S_PALETTE.Length )
			return;
		Color c = S_PALETTE [ index ];

		foreach ( var renderer in GetComponentsInChildren<Renderer>() )
		{
			if ( renderer.material != null )
			{
				renderer.material = new Material( renderer.material );
				renderer.material.color = c;
			}
		}
	}
	public int GetCharacterID()
	{
		return _characterId;
	}
	public void SetCharacterID(int id)
	{
		_characterId = id;
	}
	public bool GetAI()
	{
		return _ai;
	}
	public void SetAI(bool ai)
	{
		_ai = ai;
	}
	public string GetColorString()
	{
		if (_colorIndex.Value >= 0 && _colorIndex.Value < S_PALETTE.Length)
		{
			Color c = S_PALETTE [ _colorIndex.Value ];
			return $"RGBA({c.r:F2}, {c.g:F2}, {c.b:F2}, {c.a:F2})";
		}
		return "Invalid";
	}
}
