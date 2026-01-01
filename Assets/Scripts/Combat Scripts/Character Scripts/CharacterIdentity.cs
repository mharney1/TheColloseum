using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class CharacterIdentity : NetworkBehaviour
{
	public int team => _team.Value;
	public int pair => _pair.Value;
	private bool _ai =false;
	private NetworkVariable<int> _pair = new( -1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server );
	private NetworkVariable<int> _team = new( -1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server );
	private NetworkVariable<int> _colorIndex = new( -1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server );
	private static readonly Color [] S_PALETTE = {
		Color.blue, // index 0 -> player 1
		Color.red, // index 1 -> player 2
		Color.green, // index 2 -> player 3
		new Color(1f, 0.5f, 0f) // index 3 -> player 4
	};

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		_colorIndex.OnValueChanged += OnColorIndexChanged;
		ApplyColorIndex( _colorIndex.Value );
		if (IsOwner)
		{
			RequestPlayerNameServerRPC();
		}
	}
	public override void OnNetworkDespawn()
	{
		_colorIndex.OnValueChanged -= OnColorIndexChanged;
	}

	public void SetTeam(int newTeam)
	{
		if (IsServer)
		{
		_team.Value = newTeam;
		}
	}
	public void SetPair(int newPair)
	{
		if (IsServer)
		{
		_pair.Value = newPair;
		}
	}
	[ClientRpc]
	private void SendPlayerNameClientRpc(string newName)
	{
		gameObject.name = newName;
	}
	[ServerRpc( RequireOwnership = false )]
	private void RequestPlayerNameServerRPC()
	{
		SendPlayerNameClientRpc( name );
	}
	public void SetColorIndexOnServer(int index)
	{
		if (IsServer)
		{
			_colorIndex.Value = Mathf.Clamp( index, 0, S_PALETTE.Length - 1 );
		}
	}
	private void OnColorIndexChanged(int oldIndex, int newIndex)
	{
		ApplyColorIndex( newIndex );
	}
	private void ApplyColorIndex(int index)
	{
		if (index < 0 || index >= S_PALETTE.Length)
			return;
		Color c = S_PALETTE [ index ];

		foreach (var renderer in GetComponentsInChildren<Renderer>())
		{
			if (renderer.material != null)
			{
				renderer.material = new Material( renderer.material );
				renderer.material.color = c;
			}
		}
	}
	public bool isAI()
	{
		return _ai;
	}
}
