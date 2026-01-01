using UnityEngine;
using UnityEngine.UI;

public class SliderUI : MonoBehaviour
{
	private Character _character;
	[SerializeField] private Slider _healthSlider;
	[SerializeField] private Slider _exhaustionSlider;
	[SerializeField] private Canvas _playerCanvas;

	// visual-only scaling, not gameplay
	private int _halfCircleAdapter = 2;
	private float _sliderSpeed = 5f;
	private float _displayedHealth;
	private float _displayedExhaustion;

	private void Awake()
	{
		_character = GetComponent<Character>();
	}
	private void Start()
	{
		if (_healthSlider == null || _exhaustionSlider == null || _playerCanvas == null)
		{
			Debug.LogError( "SliderUI missing references", this );
			enabled = false;
			return;
		}
		_healthSlider.maxValue = _character.stats.GetMaxHealth() * _halfCircleAdapter;
		_healthSlider.value = _character.stats.GetHealth();
		_displayedHealth = _character.stats.GetHealth();
		_exhaustionSlider.maxValue = 1 * _halfCircleAdapter;
		_exhaustionSlider.value = _character.stats.GetExhaustion();
		_displayedExhaustion = _character.stats.GetExhaustion();
		_playerCanvas.worldCamera = Camera.main;
	}
	private void Update()
	{
		if (_character == null)
		{
			return;
		}

		_displayedHealth = Mathf.Lerp( _displayedHealth, _character.stats.GetHealth(), Time.deltaTime * _sliderSpeed );
		_displayedExhaustion = Mathf.Lerp( _displayedExhaustion, _character.stats.GetExhaustion(), Time.deltaTime * _sliderSpeed );

		_healthSlider.value = _displayedHealth;
		_exhaustionSlider.value = _displayedExhaustion;
	}
}
