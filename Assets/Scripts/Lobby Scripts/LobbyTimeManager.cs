using UnityEngine;

public class LobbyTimeManager
{
	private readonly LobbyManager _lobby;

	private float _timeRemaining;
	private bool _running;

	private const float C_INITIAL_TIME = 60f;
	private const float C_FILLED_THRESHOLD = 15f;
	private const float C_READY_THRESHOLD = 3f;

	public float TimeRemaining => _timeRemaining;

	public LobbyTimeManager(LobbyManager lobby)
	{
		_lobby = lobby;
		_timeRemaining = C_INITIAL_TIME;
		_running = true; // 🔑 starts immediately
	}

	public void Tick()
	{
		if (!_running)
			return;

		ApplyThresholds();

		_timeRemaining -= Time.deltaTime;
		_timeRemaining = Mathf.Max( _timeRemaining, 0f );
	}

	public bool IsExpired()
	{
		return _timeRemaining <= 0f;
	}

	private void ApplyThresholds()
	{
		var slots = _lobby.GetSlots();

		bool allSlotsFilled = slots.TrueForAll(
			s => s.status != SlotStatus.Open
		);

		if (!allSlotsFilled)
			return;

		bool allHumansReady = slots.TrueForAll(
			s => s.status != SlotStatus.Human || s.isReady
		);

		if (allHumansReady)
		{
			_timeRemaining = Mathf.Min(
				_timeRemaining,
				C_READY_THRESHOLD
			);
		}
		else
		{
			_timeRemaining = Mathf.Min(
				_timeRemaining,
				C_FILLED_THRESHOLD
			);
		}
	}

}
