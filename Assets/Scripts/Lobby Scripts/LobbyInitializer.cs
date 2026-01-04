using System.Collections.Generic;
using UnityEngine;


public class LobbyInitializer
{
	private readonly LobbyManager _lobby;

	public LobbyInitializer(LobbyManager lobby)
	{
		_lobby = lobby;
	}

	public void Initialize()
	{
		CreateSlots();
		AssignTeams();
		AssignHost();
		_lobby.aiManager.AIInitial();
	}

	private void CreateSlots()
	{
		var slots = _lobby.GetSlots();
		slots.Clear();
		int total = GameSession.S_INSTANCE.totalPlayerCount;

		for (int i = 0; i < total; i++)
		{
			slots.Add(
				new LobbySlot
				{
					index = i,
					status = SlotStatus.Open,
					team = Team.None,
					isHost = false,
					isReady = false
				}
			);
		}
	}

	private void AssignTeams()
	{
		var slots = _lobby.GetSlots();

		switch (GameSession.S_INSTANCE.gameMode)
		{
			case GameMode.Duos:
			case GameMode.Quads:
				for (int i = 0; i < slots.Count; i++)
				{
					slots [ i ].team = (i % 2 == 0)
						? Team.TeamA
						: Team.TeamB;
				}
				break;
			default:
				// FFA & Solos
				break;
		}
	}

	private void AssignHost()
	{
		var slots = _lobby.GetSlots();
		Debug.Assert(
			slots.Count > 0,
			"Cannot assign host with zero slots"
		);

		slots [ 0 ].status = SlotStatus.Human;
		slots [ 0 ].isHost = true;
		slots [ 0 ].isReady = false;
	}
}
