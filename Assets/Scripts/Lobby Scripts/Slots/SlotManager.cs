using Unity.Netcode;
using UnityEngine;

public class SlotManager
{
	private readonly NetworkList<SlotData> _slots;


	public SlotManager(NetworkList<SlotData> slots)
	{
		_slots = slots;
	}

	/// SLOT ACCESSORS
	/// <summary>
	/// These methods are used to access the slots.
	/// </summary>
	/// <returns></returns>
	public NetworkList<SlotData> GetSlots()
	{
		return _slots;
	}

	public int GetSlotIndexFromClient(ulong clientId)
	{
		for (int i = 0; i < _slots.Count; i++)
		{
			if (_slots [ i ].clientId == clientId)
				return i;
		}
		return -1;
	}

	/// SLOT ASSIGNMENT
	/// <summary>
	/// These methods manage the assignment and unassignment of slots.
	/// </summary>
	public void InitializeNetworkSlots()
	{
		Debug.Log("[SLOT INITIALIZATION] Creating slots.");

		int total = GameSession.S_INSTANCE.participants;

		for (int i = 0; i < total; i++)
		{
			_slots.Add(new SlotData
			{
				team = Team.None,
				status = SlotStatus.Open,
				isHost = false,
				isReady = false
			}
			);
		}
		Debug.Log($"[SLOT INITIALIZATION] Slots Created: {_slots.Count}.");
	}

	public void AssignClientToSlot(ulong clientId)
	{
		for (int i = 0; i < _slots.Count; i++)
		{
			var slot = _slots [ i ];

			if (slot.status != SlotStatus.Open)
				continue;

			slot.status = SlotStatus.Human;
			slot.clientId = clientId;
			slot.isReady = false;

			_slots [ i ] = slot;

			Debug.Log(
				$"[SLOT ASSIGNMENT] Client {clientId} assigned to slot {i}."
			);

			return;
		}
		Debug.LogWarning(
			$"[SLOT ASSIGNMENT] No open slot found for client {clientId}."
		);
	}

	public void ClearSlot(int index)
	{
		var slot = _slots [ index ];

		slot.status = SlotStatus.Open;
		slot.isReady = false;
		slot.isHost = false;

		_slots [ index ] = slot;
	}

	public void HandleClientDisconnected(ulong clientId)
	{
		int slotIndex = GetSlotIndexFromClient(clientId);

		if (slotIndex < 0)
			return;

		ClearSlot(slotIndex);
	}

	/// Modifiers
	/// <summary>
	/// These methods modify slots.
	/// </summary>
	/// <param name="clientId"></param>
	public void AssignTeams()
	{
		switch (GameSession.S_INSTANCE.gameMode)
		{
			case GameMode.Duos:
			case GameMode.Quads:
				for (int i = 0; i < _slots.Count; i++)
				{
					var slot = _slots [ i ];
					slot.team = ((i % 2 == 0)
						? Team.TeamA
						: Team.TeamB);
					_slots [ i ] = slot;
				}
				break;
			default:
				// FFA & Solos
				break;
		}
	}

	public void ToggleReady(ulong clientId)
	{
		for (int i = 0; i < _slots.Count; i++)
		{
			var slot = _slots [ i ];

			if (slot.clientId != clientId)
				continue;

			if (slot.status != SlotStatus.Human)
				return;

			slot.isReady = !slot.isReady;

			_slots [ i ] = slot;

			Debug.Log(
				$"[READY TOGGLE] Client {clientId} {(slot.isReady ? "is ready" : "is not ready")}."
			);
			return;
		}
		Debug.LogWarning(
			$"[READY TOGGLE] Client {clientId} has no slot."
		);
	}

	/// HELPERS
	/// <summary>
	/// These are additional helpers for the slot manager.
	/// </summary>
	/// <param name="clientId"></param>
	public bool AllSlotsFilled()
	{
		if (_slots.Count == 0)
			return false;

		for (int i = 0; i < _slots.Count; i++)
		{
			if (_slots [ i ].status == SlotStatus.Open)
				return false;
		}

		return true;
	}

	public bool AllHumansReady()
	{
		if (_slots.Count == 0)
			return false;

		for (int i = 0; i < _slots.Count; i++)
		{
			if (
				_slots [ i ].status == SlotStatus.Human &&
				!_slots [ i ].isReady
			)
			{
				return false;
			}
		}
		return true;
	}

	public void BuildGameSessionPlayers()
	{
		var session = GameSession.S_INSTANCE;

		session.players.Clear();

		foreach (var slot in _slots)
		{
			if (slot.status == SlotStatus.Open)
				continue;

			Player player = new Player
			{
				Identity = new PlayerIdentity()
			};

			player.Identity.SetClientID(slot.clientId);

			player.Identity.SetUsername(
				slot.status == SlotStatus.AI
					? "AI"
					: "Player"
			);

			player.Identity.SetHost(slot.isHost);

			player.Identity.SetAI(
				slot.status == SlotStatus.AI
			);

			player.Identity.SetTeam(slot.team);

			session.players.Add(player);
		}
	}
}
