
using Unity.Netcode;


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

	public override void OnNetworkSpawn()
	{
		combat = GetComponent<CharacterCombat>();
		identity = GetComponent<CharacterIdentity>();
		movement = GetComponent<CharacterMovement>();
		stats = GetComponent<CharacterStats>();
		ui = GetComponent<CharacterUI>();

		if (IsOwner)
		{
			decision = new PlayerDecisionSource();
		}
		else if (IsServer && identity.isAI())
		{
			decision = new AIDecisionSource();
		}

		if (IsServer && PlayerManager.Instance != null)
		{
			PlayerManager.Instance.AddIdlePlayer( this );
		}
	}
}
