using UnityEngine;
using Unity.Netcode;
using System.Collections;

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

		StartCoroutine(WaitForPhaseManager());
	}
	private IEnumerator WaitForPhaseManager()
	{
		while (CombatDirector.S_INSTANCE == null || !CombatDirector.S_INSTANCE.IsSpawned)
			yield return null;

		CombatDirector.S_INSTANCE.CurrentState.OnValueChanged += HandleStateChanged;

		Debug.Log($"{name} subscribed to phase changes");
	}
	public override void OnNetworkDespawn()
	{
		if (CombatDirector.S_INSTANCE != null)
		{
			CombatDirector.S_INSTANCE.CurrentState.OnValueChanged -= HandleStateChanged;
		}
	}
	private void HandleStateChanged(CombatState previous, CombatState current)
	{
		Debug.Log($"Phase changed to {current} on {name}");
		switch (current)
		{
			case CombatState.Prepare:
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
			case CombatState.Action:
				if ( IsOwner && !_character.identity.GetAI() )
				{
					_character.ui.combat.HideUI();
				}
				break;
			case CombatState.Resolve:
				if ( IsServer )
				{
					StoreChoice();
				}
				break;
			case CombatState.End:
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
	[ServerRpc(RequireOwnership = false)]
	public void SetChoiceServerRpc(Choices choice)
	{
		if (CombatDirector.S_INSTANCE.CurrentState.Value != CombatState.Prepare)
		{
			return;
		}
		_currentChoice.Value = choice;
		if (CombatDirector.S_INSTANCE != null)
		{
			CombatDirector.S_INSTANCE.StateMachine.PrepareState.RemoveUndecided( _character );
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
	public void SetOpponentClientRPC(int characterId)
	{
		_opponent = null;
		foreach (var netObj in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
		{
			Character character = netObj.GetComponent<Character>();
			if (character != null && character.identity.GetCharacterID() == characterId)
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
