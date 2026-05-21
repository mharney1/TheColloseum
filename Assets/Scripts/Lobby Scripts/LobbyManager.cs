using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
	public static LobbyManager S_INSTANCE;

	private LobbyAIManager _aiManager;
	private LobbyStateMachine _stateMachine;
	private SlotManager _slotManager;

	private NetworkList<SlotData> _slots;

	public NetworkVariable<float> TimeRemaining = new();
	public NetworkVariable<int> CurrentState = new();

	public LobbyAIManager AIManager => _aiManager;
	public LobbyStateMachine StateMachine => _stateMachine;
	public SlotManager SlotManager => _slotManager;

	/// LIFECYCLE METHODS
	/// <summary>
	///These are the generic unity lifecycle methods.
	/// </summary>
	private void Awake()
	{
		if (S_INSTANCE == null)
			S_INSTANCE = this;
		else
			Destroy(gameObject);

		_slots = new NetworkList<SlotData>();

		_slotManager = new SlotManager(_slots);
	}

	public override void OnNetworkSpawn()
	{
		if (!IsServer)
			return;

		NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
		NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

		_aiManager = new LobbyAIManager(this);

		InitializeLobby();

		_stateMachine = new LobbyStateMachine(this);

		_slotManager.AssignClientToSlot(
			NetworkManager.Singleton.LocalClientId
		);
	}

	public override void OnNetworkDespawn()
	{
		if (!IsServer || NetworkManager.Singleton == null)
			return;

		NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
		NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

		_aiManager = null;
		_stateMachine = null;
		_slotManager = null;

		if (S_INSTANCE == this)
		{
			S_INSTANCE = null;
		}

		Destroy(gameObject);
	}

	private void Update()
	{
		if (!IsServer)
			return;

		_stateMachine.Tick();

		SyncNetworkState();

		HandleSceneTransition();
	}

	/// SUBSCRIPTIONS
	/// <summary>
	/// These methods act as a connection for Slot Manager Methods and client events.
	/// </summary>
	private void OnClientConnected(ulong clientId)
	{
		_slotManager.AssignClientToSlot(clientId);
	}

	private void OnClientDisconnected(ulong clientId)
	{
		_slotManager.HandleClientDisconnected(clientId);
	}

	/// LOBBY FLOW
	/// <summary>
	/// These methods control the flow of the lobby.
	/// </summary>
	private void InitializeLobby()
	{
		Debug.Log("[LOBBY INITIALIZATION] Initializing lobby.");

		_slotManager.InitializeNetworkSlots();

		_slotManager.AssignTeams();

		_aiManager.AIInitial();
	}

	public void FinalizeLobby()
	{
		_aiManager.AIFinal();

		_slotManager.BuildGameSessionPlayers();
	}

	private void HandleSceneTransition()
	{
		if ((LobbyState)CurrentState.Value != LobbyState.Starting)
			return;

		NetworkManager.Singleton.SceneManager.LoadScene(
			"Combat_Scene",
			UnityEngine.SceneManagement.LoadSceneMode.Single
		);
	}

	private void SyncNetworkState()
	{
		if (TimeRemaining.Value != _stateMachine.TimeRemaining)
		{
			TimeRemaining.Value =
				_stateMachine.TimeRemaining;
		}

		if (CurrentState.Value != (int)_stateMachine.CurrentState)
		{
			CurrentState.Value =
				(int)_stateMachine.CurrentState;
		}
	}

	/// RPCS
	[ServerRpc(RequireOwnership = false)]
	public void ToggleReadyServerRpc(
		ServerRpcParams rpcParams = default
	)
	{
		ulong clientId =
			rpcParams.Receive.SenderClientId;

		_slotManager.ToggleReady(clientId);
	}
}
