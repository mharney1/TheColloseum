using UnityEngine;

public enum LobbyState
{
	Filling,
	Preparing,
	CountingDown,
	Finalizing,
	Starting
}

public class LobbyStateManager
{
	private readonly LobbyManager _lobby;

	public LobbyState currentState
	{
		get; private set;
	}

	public LobbyStateManager(LobbyManager lobby)
	{
		_lobby = lobby;
		currentState = LobbyState.Filling;
	}

	public void Evaluate()
	{
		switch (currentState)
		{
			case LobbyState.Filling:
				EvaluateFilling();
				break;

			case LobbyState.Preparing:
				EvaluatePreparing();
				break;

			case LobbyState.CountingDown:
				EvaluateCountingDown();
				break;

			case LobbyState.Finalizing:
				EvaluateFinalizing();
				break;
			case LobbyState.Starting:
				LobbyManager.S_INSTANCE.Shutdown();
				FlowManager.S_INSTANCE.ToCombat();
				break;
		}
	}

	private void EvaluateFilling()
	{
		if (AllSlotsFilled())
		{
			currentState = LobbyState.Preparing;
			return;
		}

		if (LobbyManager.S_INSTANCE.timeManager.TimeRemaining <= 3f)
		{
			currentState = LobbyState.CountingDown;
		}
	}

	private void EvaluatePreparing()
	{
		if (AllHumansReady())
		{
			currentState = LobbyState.CountingDown;
			return;
		}

		if (LobbyManager.S_INSTANCE.timeManager.TimeRemaining <= 3f)
		{
			currentState = LobbyState.CountingDown;
		}
	}

	private void EvaluateCountingDown()
	{
		if (_lobby.timeManager.IsExpired())
		{
			currentState = LobbyState.Finalizing;
		}
	}

	private void EvaluateFinalizing()
	{
		_lobby.FinalizeLobby();
		currentState = LobbyState.Starting;
	}

	private bool AllSlotsFilled()
	{
		foreach (var slot in LobbyManager.S_INSTANCE.slots)
		{
			if (slot.status == SlotStatus.Open)
				return false;
		}
		return true;
	}

	private bool AllHumansReady()
	{
		foreach (var slot in LobbyManager.S_INSTANCE.slots)
		{
			if (slot.status == SlotStatus.Human && !slot.isReady)
				return false;
		}
		return true;
	}

}
