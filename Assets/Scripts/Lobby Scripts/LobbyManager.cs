using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
	public static LobbyManager S_INSTANCE;
	private List<LobbySlot> _slots = new List<LobbySlot>();
	public IReadOnlyList<LobbySlot> slots => _slots;

	public LobbyInitializer initializer;
	public LobbyAIManager aiManager;
	public LobbyStateManager stateManager;
	public LobbyTimeManager timeManager;
	//public LobbyValidator validator = new LobbyValidator();

	private void Awake()
	{
		if (S_INSTANCE == null)
			S_INSTANCE = this;
		else
			Destroy( gameObject );
	}
	private void Start()
	{
		initializer = new LobbyInitializer( this );
		aiManager = new LobbyAIManager( this );
		stateManager = new LobbyStateManager( this );
		timeManager = new LobbyTimeManager( this );

		initializer.Initialize();
		ValidateLobbyInvariants();
	}

	private void Update()
	{
		timeManager.Tick();
		stateManager.Evaluate();
	}
	public void FinalizeLobby()
	{
		aiManager.AIFinal();
	}
	public IReadOnlyList<LobbySlot> GetSlotsImmutable()
	{
		return slots;
	}

	internal List<LobbySlot> GetSlots()
	{
		return _slots;
	}

	public void ToggleLocalPlayerReady()
	{
		var localSlot = _slots.FirstOrDefault( s => s.isHost ); // assume host for now
		if (localSlot != null && localSlot.status == SlotStatus.Human)
			localSlot.isReady = !localSlot.isReady;
	}

	public void Shutdown()
	{
		enabled = false;

		initializer = null;
		aiManager = null;
		stateManager = null;
		timeManager = null;

		_slots.Clear();
		if (S_INSTANCE == this)
		{
			S_INSTANCE = null;
		}

		Destroy( gameObject );
	}

	private void ValidateLobbyInvariants()
	{
		int humans = 0;
		int ai = 0;

		foreach (var slot in _slots)
		{
			if (slot.status == SlotStatus.Human)
				humans++;

			if (slot.status == SlotStatus.AI)
				ai++;
		}


		if (GameSession.S_INSTANCE.matchType == MatchType.CoOp)
		{
			foreach (var slot in _slots)
			{
				if (slot.team == Team.TeamB)
					Debug.Assert( slot.status == SlotStatus.AI,
						"Team B mising AI"
					);
			}
		}
	}

}
