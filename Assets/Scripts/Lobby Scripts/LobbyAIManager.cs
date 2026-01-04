using System.Collections.Generic;
using UnityEngine;

public class LobbyAIManager
{
	private readonly LobbyManager _lobby;

	public LobbyAIManager(LobbyManager lobby)
	{
		_lobby = lobby;
	}

	public void AIInitial()
	{
		var slots = _lobby.GetSlots();

		int aiRemaining = GameSession.S_INSTANCE.aiPlayerCount;
		bool isCoOp = GameSession.S_INSTANCE.matchType == MatchType.CoOp;

		if (isCoOp)
		{
			foreach (var slot in slots)
			{
				if (aiRemaining == 0)
					break;
				if (slot.team != Team.TeamB)
					continue;
				if (slot.status != SlotStatus.Open)
					continue;

				slot.status = SlotStatus.AI;
				slot.isReady = true;
				aiRemaining--;
			}
		}
		for (int i = slots.Count - 1; i >= 0 && aiRemaining > 0; i--)
		{
			if (slots [ i ].status != SlotStatus.Open)
				continue;
			slots [ i ].status = SlotStatus.AI;
			slots [ i ].isReady = true;
			aiRemaining--;
		}

		Debug.Assert(
			aiRemaining == 0,
			"Not enough open slots to place all AI players"
		);
	}
	public void AIFinal()
	{
		var slots = _lobby.GetSlots();

		for (int i = 0; i < slots.Count; i++)
		{
			if (slots [ i ].status != SlotStatus.Open)
				continue;
			slots [ i ].status = SlotStatus.AI;
			slots [ i ].isReady = true;
		}

		Debug.Assert(
			slots.TrueForAll( s => s.status != SlotStatus.Open ),
			"FinalizeLobby failed: open slots remain"
		);
	}

}
