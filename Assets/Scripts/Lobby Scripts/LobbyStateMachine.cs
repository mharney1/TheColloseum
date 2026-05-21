using UnityEngine;

public enum LobbyState
{
	Filling,
	Preparing,
	CountingDown,
	Finalizing,
	Starting
}

public class LobbyStateMachine
{
	private readonly LobbyManager _lobby;

	private LobbyState _currentState;

	private float _timeRemaining;
	private bool _isActive;

	private const float C_INITIAL_TIME = 60f;
	private const float C_FILLED_THRESHOLD = 10f;
	private const float C_READY_THRESHOLD = 3f;

	public LobbyState CurrentState => _currentState;
	public float TimeRemaining => _timeRemaining;

	public LobbyStateMachine(LobbyManager lobby)
	{
		_lobby = lobby;

		_currentState = LobbyState.Filling;

		_timeRemaining = C_INITIAL_TIME;
		_isActive = true;
	}

	public void Tick()
	{
		if (!_isActive)
			return;

		UpdateTimer();
		EvaluateState();
	}

/// STATE EVALUATION
/// <summary>
/// The methods below evaluate the conditions neede to progress between states.
/// </summary>
	private void EvaluateState()
	{
		switch (_currentState)
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
				break;
		}
	}
	private void EvaluateFilling()
	{
		if (_lobby.SlotManager.AllSlotsFilled())
		{
			ForceCountdownThreshold(C_FILLED_THRESHOLD);
			ChangeState(LobbyState.Preparing);
		}
		if (_timeRemaining <= C_READY_THRESHOLD)
		{
			ChangeState(LobbyState.CountingDown);
		}
	}
	private void EvaluatePreparing()
	{
		if (_lobby.SlotManager.AllHumansReady())
		{
			ForceCountdownThreshold(C_READY_THRESHOLD);
			ChangeState(LobbyState.CountingDown);

			return;
		}
		if (_timeRemaining <= C_READY_THRESHOLD)
		{
			ChangeState(LobbyState.CountingDown);
		}
	}
	private void EvaluateCountingDown()
	{
		if (IsExpired())
		{
			ChangeState(LobbyState.Finalizing);
		}
	}
	private void EvaluateFinalizing()
	{
		_lobby.FinalizeLobby();

		Debug.Assert(
			GameSession.S_INSTANCE.players.Count ==
			GameSession.S_INSTANCE.participants,
			"Player list mismatch before combat load."
		);

		_isActive = false;

		ChangeState(LobbyState.Starting);
	}
/// TIMER UPDATES
/// <summary>
/// The methods below manage the timer.
/// </summary>
	private void UpdateTimer()
	{
		_timeRemaining -= Time.deltaTime;
		_timeRemaining = Mathf.Max(_timeRemaining, 0f);
	}
	private void ForceCountdownThreshold(float threshold)
	{
		_timeRemaining = Mathf.Min(
			_timeRemaining,
			threshold
		);
	}
/// HELPERS
/// <summary>
/// The functions below help manage the state of the lobby.
/// </summary>
/// <param name="newState"></param>

	private void ChangeState(LobbyState newState)
	{
		if (_currentState == newState)
			return;

		_currentState = newState;

		Debug.Log( $"[LOBBY STATE] Changed to {_currentState}" );
	}
	private bool IsExpired()
	{
		return _timeRemaining <= 0f;
	}
}
