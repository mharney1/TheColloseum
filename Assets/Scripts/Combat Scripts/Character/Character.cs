using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class Character : NetworkBehaviour
{
	public CharacterCombat combat
	{
		get; private set;
	}
	public ICharacterDecision decision
	{
		get; private set;
	}
	public CharacterIdentity identity
	{
		get; private set;
	}
	public CharacterMovement movement
	{
		get; private set;
	}
	public CharacterStats stats
	{
		get; private set;
	}
	public CharacterUI ui
	{
		get; private set;
	}

	public void Awake()
	{
		Initialize();
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		Initialize();

		SetupGameplayLogic();
	}

	private void Initialize()
	{
		combat = GetComponent<CharacterCombat>();
		identity = GetComponent<CharacterIdentity>();
		movement = GetComponent<CharacterMovement>();
		stats = GetComponent<CharacterStats>();
		ui = GetComponent<CharacterUI>();

		Debug.Log($"Player {identity.GetCharacterID()} Owned by {OwnerClientId}");
	}

	private void SetupGameplayLogic()
	{
		bool isAI = identity.GetAI();

		if (!isAI && IsOwner)
		{
			decision = new PlayerDecisionSource();
			Debug.Log($"Player control assigned | Owner: {OwnerClientId}");
		}
		else if (isAI && IsServer)
		{
			decision = new AIDecisionSource();
			Debug.Log($"AI control assigned | Owner: {OwnerClientId}");
		}

		if (IsServer)
		{
			StartCoroutine(AddIdlePlayer());
		}
	}
	public IEnumerator AddIdlePlayer()
	{
		yield return new WaitUntil(() => CombatDirector.S_INSTANCE.PlayerManager != null);

		CombatDirector.S_INSTANCE.PlayerManager.AddIdlePlayer(this);
	}
}
