using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatUI : MonoBehaviour
{
	private Character _character;

	[SerializeField] private GameObject _preppingScreen;
	[SerializeField] private Button _attackButton;
	[SerializeField] private Button _blockButton;
	[SerializeField] private Button _counterButton;
	[SerializeField] private Button _restButton;

	[SerializeField] private GameObject _preppedScreen;
	[SerializeField] private Button _changeButton;
	[SerializeField] private TextMeshProUGUI _preppedText;

	private CombatUIState _state;

	public void Awake()
	{
		_character = GetComponent<Character>();
	}
	private void OnEnable()
	{
		_attackButton.onClick.AddListener( () => Choose( Choices.Attack, "You are attacking" ) );
		_blockButton.onClick.AddListener( () => Choose( Choices.Block, "You are blocking" ) );
		_counterButton.onClick.AddListener( () => Choose( Choices.Counter, "You are countering" ) );
		_restButton.onClick.AddListener( () => Choose( Choices.Rest, "You are resting" ) );
		_changeButton.onClick.AddListener( () => SetState( CombatUIState.Choosing ) );
	}

	private void OnDisable()
	{
		_attackButton.onClick.RemoveAllListeners();
		_blockButton.onClick.RemoveAllListeners();
		_counterButton.onClick.RemoveAllListeners();
		_restButton.onClick.RemoveAllListeners();
		_changeButton.onClick.RemoveAllListeners();
	}
	public void ShowUI()
	{
		if (_character == null || _character.stats == null)
		{
			return;
		}

		if (_character.stats.IsDefeated())
		{
			SetState( CombatUIState.Disabled, "You have been defeated" );
		}
		else if (_character.stats.GetDizzy())
		{
			SetState( CombatUIState.Disabled, "Dizzy" );
		}
		else if (_character.identity.pair == -1)
		{
			SetState( CombatUIState.Disabled, "Waiting for an Opponent" );
		}
		else
		{
			SetState( CombatUIState.Choosing );
		}
	}
	private void SetState( CombatUIState newState, string prepped = "")
	{
		if (_state == newState)
			return;

#if UNITY_EDITOR
		if (newState == CombatUIState.Chosen && _state != CombatUIState.Choosing)
		{
			Debug.LogWarning( "Chosen state entered without Choosing", this );
		}
#endif

		_state = newState;

		switch (_state)
		{
			case CombatUIState.Hidden:
				_preppingScreen.SetActive( false );
				_preppedScreen.SetActive( false );
				_changeButton.interactable = false;
				_preppedText.text = prepped;
				break;

			case CombatUIState.Choosing:
				_preppingScreen.SetActive( true );
				_preppedScreen.SetActive( false );
				_changeButton.interactable = false;
				_preppedText.text = prepped;
				break;

			case CombatUIState.Chosen:
				_preppingScreen.SetActive( false );
				_preppedScreen.SetActive( true );
				_changeButton.interactable = true;
				_preppedText.text = prepped;
				break;

			case CombatUIState.Disabled:
				_preppingScreen.SetActive( false );
				_preppedScreen.SetActive( true );
				_changeButton.interactable = false;
				_preppedText.text = prepped;
				break;
		}
	}
	private void Choose(Choices choice, string text)
	{
		_character.combat.SetChoiceServerRpc( choice );
		SetState( CombatUIState.Chosen, text );
	}
	public void HideUI()
	{
		SetState( CombatUIState.Hidden );
	}
}
