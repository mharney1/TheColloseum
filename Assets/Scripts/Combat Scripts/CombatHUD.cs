using TMPro;
using UnityEngine;
using static UnityEngine.CullingGroup;

public class CombatHUD : MonoBehaviour
{
	[SerializeField] private GameObject _timerRoot;
	[SerializeField] private TMP_Text _timerText;

	private CombatDirector _director;

	private void Start()
	{
		_director = CombatDirector.S_INSTANCE;

		if (_director == null)
			return;

		_director.CurrentState.OnValueChanged += OnStateChanged;
		_director.TimeRemaining.OnValueChanged += OnTimerChanged;

		OnStateChanged(
			_director.CurrentState.Value,
			_director.CurrentState.Value
		);

		OnTimerChanged(
			_director.TimeRemaining.Value,
			_director.TimeRemaining.Value
		);
	}

	private void OnDestroy()
	{
		if (_director == null)
			return;

		_director.CurrentState.OnValueChanged -= OnStateChanged;
		_director.TimeRemaining.OnValueChanged -= OnTimerChanged;
	}

	private void OnStateChanged( CombatState oldState, CombatState newState )
	{
		_timerRoot.SetActive( newState == CombatState.Prepare );
	}

	private void OnTimerChanged( float oldValue, float newValue )
	{
		_timerText.text = Mathf.CeilToInt(newValue).ToString("00");
	}
}
