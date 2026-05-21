using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class NetworkBootstrap : MonoBehaviour
{
	public static NetworkBootstrap S_INSTANCE;

	[SerializeField] private string _lobbySceneName = "Lobby_Scene";

	public Lobby CurrentLobby
	{
		get; private set;
	}

	private UnityTransport _transport;

	// ---------------- INIT ----------------

	private void Awake()
	{
		if (S_INSTANCE != null)
		{
			Destroy(gameObject);
			return;
		}

		S_INSTANCE = this;
		DontDestroyOnLoad(gameObject);

		StartCoroutine(Initialize());
	}

	private IEnumerator Initialize()
	{
		while (NetworkManager.Singleton == null)
			yield return null;

		_transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

		var initTask = UnityServices.InitializeAsync();
		yield return new WaitUntil(() => initTask.IsCompleted);

		var authTask = AuthenticationService.Instance.SignInAnonymouslyAsync();
		yield return new WaitUntil(() => authTask.IsCompleted);

		NetworkManager.Singleton.OnClientDisconnectCallback += HandleHostDisconnect;

		Debug.Log("UGS Ready");
	}

	// ---------------- CREATE ----------------

	public void CreateGame(bool isPrivate)
	{
		if (NetworkManager.Singleton.IsListening)
			return;

		StartCoroutine(CreateGameRoutine(isPrivate));
	}

	private IEnumerator CreateGameRoutine(bool isPrivate)
	{
		var session = GameSession.S_INSTANCE;

		var allocTask = RelayService.Instance.CreateAllocationAsync(session.participants - 1);
		yield return new WaitUntil(() => allocTask.IsCompleted);
		var allocation = allocTask.Result;

		var codeTask = RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
		yield return new WaitUntil(() => codeTask.IsCompleted);
		string joinCode = codeTask.Result;

		var options = new CreateLobbyOptions
		{
			IsPrivate = isPrivate,
			Data = new Dictionary<string, DataObject>
			{
				{ "joinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) },
				{ "matchType", new DataObject(DataObject.VisibilityOptions.Public, session.matchType.ToString(), DataObject.IndexOptions.S1) },
				{ "gameMode", new DataObject(DataObject.VisibilityOptions.Public, session.gameMode.ToString(), DataObject.IndexOptions.S2) }
			}
		};

		var lobbyTask = LobbyService.Instance.CreateLobbyAsync("Lobby", session.participants, options);
		yield return new WaitUntil(() => lobbyTask.IsCompleted);
		CurrentLobby = lobbyTask.Result;

		SetupHostRelay(allocation);

		NetworkManager.Singleton.StartHost();

		NetworkManager.Singleton.SceneManager.LoadScene(
			_lobbySceneName,
			UnityEngine.SceneManagement.LoadSceneMode.Single
		);
	}

	// ---------------- JOIN ----------------

	public void JoinGame(Lobby lobby)
	{
		if (NetworkManager.Singleton.IsListening)
			return;

		StartCoroutine(JoinRoutine(lobby));
	}

	private IEnumerator JoinRoutine(Lobby lobby)
	{
		var joinTask = LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
		yield return new WaitUntil(() => joinTask.IsCompleted);
		CurrentLobby = joinTask.Result;

		if (!CurrentLobby.Data.ContainsKey("joinCode"))
		{
			Debug.LogError("Missing joinCode");
			yield break;
		}

		string code = CurrentLobby.Data [ "joinCode" ].Value;

		var relayTask = RelayService.Instance.JoinAllocationAsync(code);
		yield return new WaitUntil(() => relayTask.IsCompleted);

		SetupClientRelay(relayTask.Result);

		NetworkManager.Singleton.StartClient();
	}

	// ---------------- QUICK MATCH ----------------

	public void QuickMatch()
	{
		StartCoroutine(QuickMatchRoutine());
	}

	private IEnumerator QuickMatchRoutine()
	{
		var task = GetFilteredLobbies();
		yield return new WaitUntil(() => task.IsCompleted);

		var lobbies = task.Result;

		if (lobbies == null || lobbies.Count == 0)
		{
			yield return new WaitForSeconds(1.5f);

			task = GetFilteredLobbies();
			yield return new WaitUntil(() => task.IsCompleted);

			lobbies = task.Result;
		}

		if (lobbies != null && lobbies.Count > 0)
		{
			lobbies.Sort((a, b) => a.Created.CompareTo(b.Created));
			JoinGame(lobbies [ 0 ]);
		}
		else
		{
			CreateGame(false);
		}
	}

	// ---------------- RELAY ----------------

	private void SetupHostRelay(Allocation a)
	{
		_transport.SetHostRelayData(
			a.RelayServer.IpV4,
			(ushort)a.RelayServer.Port,
			a.AllocationIdBytes,
			a.Key,
			a.ConnectionData
		);
	}

	private void SetupClientRelay(JoinAllocation a)
	{
		_transport.SetClientRelayData(
			a.RelayServer.IpV4,
			(ushort)a.RelayServer.Port,
			a.AllocationIdBytes,
			a.Key,
			a.ConnectionData,
			a.HostConnectionData
		);
	}

	// ---------------- QUERY ----------------

	private async Task<List<Lobby>> GetFilteredLobbies()
	{
		var s = GameSession.S_INSTANCE;

		QueryLobbiesOptions options = new QueryLobbiesOptions
		{
			Filters = new List<QueryFilter>
			{
				new QueryFilter(QueryFilter.FieldOptions.S1, s.matchType.ToString(), QueryFilter.OpOptions.EQ),
				new QueryFilter(QueryFilter.FieldOptions.S2, s.gameMode.ToString(), QueryFilter.OpOptions.EQ),
				new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
			}
		};

		QueryResponse res = await LobbyService.Instance.QueryLobbiesAsync(options);
		return res.Results;
	}

	// ---------------- LEAVE ----------------

	public async Task LeaveGame()
	{
		if (CurrentLobby != null)
		{
			try
			{
				await LobbyService.Instance.RemovePlayerAsync(
					CurrentLobby.Id,
					AuthenticationService.Instance.PlayerId
				);
			}
			catch (Exception e)
			{
				Debug.LogWarning($"Leave lobby failed: {e}");
			}
		}

		CurrentLobby = null;

		if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
		{
			NetworkManager.Singleton.Shutdown();
		}
	}

	// ---------------- DESTROY ----------------
	private void OnDestroy()
	{
		if (NetworkManager.Singleton != null)
		{

			NetworkManager.Singleton.OnClientDisconnectCallback -= HandleHostDisconnect;
			if (NetworkManager.Singleton.IsListening)
			{
				NetworkManager.Singleton.Shutdown();
			}
		}
	}

	private void HandleHostDisconnect(ulong clientId)
	{
		Debug.Log($"Disconnect Callback: {clientId}");

		// only clients should react here
		if (NetworkManager.Singleton.IsServer)
			return;

		Debug.Log("Host disconnected");

		StartCoroutine(ReturnToMenuAfterDisconnect());
	}

	private IEnumerator ReturnToMenuAfterDisconnect()
	{
		yield return LeaveGame().AsIEnumerator();

		SceneLoader.S_INSTANCE.LoadScene("Main_Menu");
	}

}

public static class TaskExtensions
{
	public static IEnumerator AsIEnumerator(this Task task)
	{
		while (!task.IsCompleted)
			yield return null;

		if (task.IsFaulted)
			throw task.Exception;
	}
}

