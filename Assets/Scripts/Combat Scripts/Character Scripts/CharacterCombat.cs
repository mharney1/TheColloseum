using System.Diagnostics;
using Unity.Netcode;

public class CharacterCombat : NetworkBehaviour
{
	private Character _character;

	private Character _opponent = null;
	private NetworkVariable<Choices> _currentChoice = new( Choices.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server );
	private NetworkList<int> _choices = new NetworkList<int>();

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		_character = GetComponent<Character>();
		if (PhaseManager.Instance != null)
		{
			PhaseManager.Instance.CurrentPhase.OnValueChanged += HandlePhaseChanged;
		}
	}
	public override void OnNetworkDespawn()
	{
		if (PhaseManager.Instance != null)
		{
			PhaseManager.Instance.CurrentPhase.OnValueChanged -= HandlePhaseChanged;
		}
	}
	private void HandlePhaseChanged(Phases previous, Phases current)
	{
		switch (current)
		{
			case Phases.Prepare:
				Debug.Assert( _character.decision != null,
					$"Decision source missing for {_character.gameObject.name}" );
				if ( IsOwner )
				{
					_character.decision.Decide( _character );
				}
				if (_character.stats.GetDizzy() && IsOwner)
				{
					SetChoiceServerRpc( Choices.None );
				}
				break;
			case Phases.Action:
				if ( IsOwner && !_character.identity.isAI() )
				{
					_character.ui.combat.HideUI();
				}
				break;
			case Phases.Resolve:
				if ( IsServer )
				{
					StoreChoice();
				}
				break;
			case Phases.End:
				// Nothing here yet
				break;
		}
	}
	public NetworkList<int> GetChoices()
	{
		return _choices;
	}
	public Choices GetChoice()
	{
		return _currentChoice.Value;
	}
	[ServerRpc( RequireOwnership = false )]
	public void SetChoiceServerRpc(Choices choice)
	{
		if (PhaseManager.Instance.GetCurrentPhase() != Phases.Prepare)
		{
			return;
		}
		_currentChoice.Value = choice;
		if (PhaseManager.Instance != null)
		{
			PhaseManager.Instance.RemoveUndecided( _character );
		}
	}
	public void RequestPlayerInput()
	{
		_character.ui.combat.ShowUI();
	}
	public void StoreChoice()
	{
		if (!IsServer || !IsSpawned)
		{
			return;
		}
			if (_currentChoice.Value != Choices.None)
			{
				if (_choices.Count >= 3)
				{
					_choices.RemoveAt( 0 );
				}
				_choices.Add( (int) _currentChoice.Value );
			}
		_currentChoice.Value = Choices.None;
	}
	public Character GetOpponent()
	{
		return _opponent;
	}
	[ClientRpc]
	public void SetOpponentClientRPC( ulong ownerId )
	{
		_opponent = null;
		foreach (var netObj in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
		{
			Character character = netObj.GetComponent<Character>();
			if (character != null && character.OwnerClientId == ownerId)
			{
				_opponent = character;
				break;
			}
		}
	}
	[ClientRpc]
	public void ClearOpponentClientRpc()
	{
		_opponent = null;
	}
}
