using System;
using Unity.Netcode;
using UnityEngine;

public class CombatDirector : NetworkBehaviour
{
	public static CombatDirector S_INSTANCE;

	public static event Action<EndData> GameEnded;

	public NetworkVariable<CombatState> CurrentState = new();
	public NetworkVariable<float> TimeRemaining = new();

	[SerializeField] private GameObject _combatManagerPrefab;

	private CombatStateMachine _stateMachine;
	public CombatStateMachine StateMachine => _stateMachine;

	private WinService _winService;
	public WinService WinService => _winService;

	private PlayerManager _playerManager;
	public PlayerManager PlayerManager => _playerManager;

	private void Awake()
	{
		if (S_INSTANCE == null)
		{
			S_INSTANCE = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public override void OnNetworkSpawn()
	{
		if (!IsServer)
			return;

		_stateMachine = new CombatStateMachine(this);
		_winService = new WinService(this, GameSession.S_INSTANCE.gameMode);
		_playerManager = new PlayerManager();

		Initialize();
	}

	private void Update()
	{
		if (!IsServer)
			return;

		_stateMachine?.Tick();

		if (CurrentState.Value != _stateMachine.CurrentState)
		{
			CurrentState.Value = _stateMachine.CurrentState;
		}

		if (TimeRemaining.Value != _stateMachine.TimeRemaining)
		{
			TimeRemaining.Value = _stateMachine.TimeRemaining;
		}
	}

	public override void OnNetworkDespawn()
	{
		Shutdown();
	}

	private void Initialize()
	{
		Debug.Log("[PHASE MANAGER] Initializing");
		Instantiate( _combatManagerPrefab );
	}

	public void AnnounceGameEnd(EndData data)
	{
		AnnounceGameEndClientRpc(
			data.Tie,
			data.TeamBased,
			data.WinningTeams,
			data.WinningPlayerIds);
	}

	[ClientRpc]
	public void AnnounceGameEndClientRpc(
	bool tie,
	bool teamBased,
	Team [] winningTeams,
	int [] winningPlayers)
	{
		GameEnded?.Invoke(new EndData
		{
			Tie = tie,
			TeamBased = teamBased,
			WinningTeams = winningTeams,
			WinningPlayerIds = winningPlayers
		});
	}

	private void Shutdown()
	{
		_stateMachine = null;

		if (S_INSTANCE == this)
		{
			S_INSTANCE = null;
		}
	}
}
