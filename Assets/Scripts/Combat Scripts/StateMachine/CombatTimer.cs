using UnityEngine;

public class CombatTimer
{
	public float TimeRemaining => _timeRemaining;

	private readonly float _startTime;
	private readonly float _shortenedTime;

	private float _timeRemaining;
	private float _unseenTime;

	public CombatTimer( float startTime, float shortenedTime )
	{
		_startTime = startTime;
		_shortenedTime = shortenedTime;
	}

	public void Reset()
	{
		_unseenTime = _startTime;
		_timeRemaining = _unseenTime;
	}

	public void Tick()
	{
		if (IsFinished())
			return;

		_unseenTime -= Time.deltaTime;

		if (Mathf.Ceil(_unseenTime) != _timeRemaining)
			_timeRemaining = _unseenTime;

		if( _unseenTime < 0f )
			_unseenTime = 0f;
	}

	public void Shorten()
	{
		if (_unseenTime > _shortenedTime)
			_unseenTime = _shortenedTime;
	}

	public bool IsFinished()
	{
		return _timeRemaining <= 0f;
	}
}
