using UnityEngine;
using UnityEngine.UI;

public class MenuButtonBinder : MonoBehaviour
{
	private MenuUIManager _manager;

	[ Header( "Main Menu Buttons" )]
	[SerializeField] private Button _storyBtn;
	[SerializeField] private Button _fightBtn;
	[SerializeField] private Button _customizationBtn;

	[Header( "Match Type Buttons" )]
	[SerializeField] private Button _singlePlayerBtn;
	[SerializeField] private Button _coopBtn;
	[SerializeField] private Button _multiplayerBtn;
	[SerializeField] private Button _customsBtn;
	[SerializeField] private Button _matchTypeBackBtn;

	[Header( "Game Mode Buttons" )]
	[SerializeField] private Button _solosBtn;
	[SerializeField] private Button _ffa4pBtn;
	[SerializeField] private Button _ffa8pBtn;
	[SerializeField] private Button _duosBtn;
	[SerializeField] private Button _quadsBtn;
	[SerializeField] private Button _gameModeBackBtn;

	private void Awake()
	{
		_manager = GetComponent<MenuUIManager>();

		BindMainMenu();
		BindMatchTypeMenu();
		BindGameModeMenu();
	}

	/// <summary>
	/// Main Menu
	/// </summary>

	private void BindMainMenu()
	{
		if (_fightBtn != null)
			_fightBtn.onClick.AddListener(
				() => _manager.OnFightSelected()
			);

		// Disabled for now
		if (_storyBtn != null)
			_storyBtn.interactable = false;

		if (_customizationBtn != null)
			_customizationBtn.interactable = false;
	}

	/// <summary>
	/// Match Type
	/// </summary>

	private void BindMatchTypeMenu()
	{
		if (_singlePlayerBtn != null)
			_singlePlayerBtn.onClick.AddListener(() => _manager.SelectMatchType( MatchType.SinglePlayer ));

		if (_coopBtn != null)
			_coopBtn.onClick.AddListener(() => _manager.SelectMatchType( MatchType.CoOp ));

		if (_multiplayerBtn != null)
			_multiplayerBtn.onClick.AddListener(() => _manager.SelectMatchType( MatchType.Multiplayer ));

		if (_customsBtn != null)
			_customsBtn.interactable = false;

		if (_matchTypeBackBtn != null)
			_matchTypeBackBtn.onClick.AddListener(() => _manager.BackToMainMenu());
	}

	/// <summary>
	/// Game Mode
	/// </summary>

	private void BindGameModeMenu()
	{
		if (_solosBtn != null)
			_solosBtn.onClick.AddListener(() => _manager.SelectGameMode( GameMode.Solos ));

		if (_duosBtn != null)
			_duosBtn.onClick.AddListener(() => _manager.SelectGameMode( GameMode.Duos ));

		if (_quadsBtn != null)
			_quadsBtn.onClick.AddListener(() => _manager.SelectGameMode( GameMode.Quads ));

		if (_ffa4pBtn != null)
			_ffa4pBtn.onClick.AddListener(() => _manager.SelectGameMode( GameMode.FFA4P ));

		if (_ffa8pBtn != null)
			_ffa8pBtn.onClick.AddListener(() => _manager.SelectGameMode( GameMode.FFA8P ));

		if (_gameModeBackBtn != null)
			_gameModeBackBtn.onClick.AddListener(() => _manager.BackToMatchType());
	}

	public void RefreshGameModeAvailability(MatchType type)
	{
		Debug.Assert(
			type != MatchType.None,
			"FilterGameModes called with MatchType.None"
		);

		SetGameModeButton(_solosBtn, MatchRules.IsCombinationValid(type, GameMode.Solos));
		SetGameModeButton(_duosBtn, MatchRules.IsCombinationValid(type, GameMode.Duos));
		SetGameModeButton(_quadsBtn, MatchRules.IsCombinationValid(type, GameMode.Quads));
		SetGameModeButton(_ffa4pBtn, MatchRules.IsCombinationValid(type, GameMode.FFA4P));
		SetGameModeButton(_ffa8pBtn, MatchRules.IsCombinationValid(type, GameMode.FFA8P));
	}

	public void SetGameModeButton(Button button, bool enabled)
	{
		if (button == null)
			return;

		button.interactable = enabled;
	}
}
