using Unity.Netcode;
using UnityEngine;

public class LobbyAIManager
{
	private readonly LobbyManager _lobby;

	public LobbyAIManager(LobbyManager lobby)
	{
		_lobby = lobby;
	}
	/// AI FILLERS
	/// <summary>
	/// These Methods fill slots with AI at both lobby creation and end.
	/// </summary>
	public void AIInitial()
	{
		var slots = _lobby.SlotManager.GetSlots();

		int aiRemaining = GameSession.S_INSTANCE.aiPlayerCount;
		bool isCoOp = GameSession.S_INSTANCE.matchType == MatchType.CoOp;

		if (isCoOp)
			FillTeamB( slots, ref aiRemaining );

		FillRemainingAI( slots, ref aiRemaining );

		Debug.Assert(
			aiRemaining == 0, "[AI INITIALIZATION] Not enough open slots for AI." );
	}

	public void AIFinal()
	{
		Debug.Log("[AI FINALIZATION] Starting");
		var slots = _lobby.SlotManager.GetSlots();

		int openSlots = 0;

		for (int i = 0; i < slots.Count; i++)
		{
			if (slots [ i ].status == SlotStatus.Open)
			{
				openSlots++;
			}
		}
		Debug.Log($"[AI FINALIZATION] {openSlots} Open Slots");
		FillRemainingAI(slots, ref openSlots);

		Debug.Assert(
			_lobby.SlotManager.AllSlotsFilled(),
			"[AI FINALIZATION] Open slots remain after AI fill."
		);
	}

	/// FILLING LOGIC
	/// <summary>
	/// These help reduce the amount of duplicate logic within the two filler methods.
	/// </summary>

	private void FillRemainingAI( NetworkList<SlotData> slots, ref int aiRemaining
)
	{
		for (int i = slots.Count - 1;
			 i >= 0 && aiRemaining > 0;
			 i--)
		{
			if (slots [ i ].status != SlotStatus.Open)
				continue;

			AssignAIToSlot(slots, i);

			aiRemaining--;
		}
	}

	private void FillTeamB( NetworkList<SlotData> slots, ref int aiRemaining )
	{
		for (int i = 0; i < slots.Count && aiRemaining > 0; i++)
		{
			if (slots [ i ].team != Team.TeamB)
				continue;

			if (slots [ i ].status != SlotStatus.Open)
				continue;

			AssignAIToSlot(slots, i);

			aiRemaining--;
		}
	}

	private void AssignAIToSlot(NetworkList<SlotData> slots, int index)
	{
		var slot = slots [ index ];

		slot.status = SlotStatus.AI;
		slot.clientId = 0;
		slot.isHost = false;
		slot.isReady = false;

		slots [ index ] = slot;
	}
}
