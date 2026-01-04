using UnityEngine;
using UnityEngine.UI;

public class MenuButtonBinder : MonoBehaviour
{
	private MainMenuUIManager _manager;

	[Header( "Main Menu Buttons" )]
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
		_manager = GetComponent<MainMenuUIManager>();
		_manager.RegisterMenuBinder( this );

		BindMainMenu();
		BindMatchTypeMenu();
		BindGameModeMenu();
	}

	public void ShutDown()
	{
		enabled = false;
		_manager = null;
		UnbindButtons();
		Destroy( gameObject );
	}

	// =====================
	// MAIN MENU
	// =====================

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

	// =====================
	// MATCH TYPE MENU
	// =====================

	private void BindMatchTypeMenu()
	{
		if (_singlePlayerBtn != null)
			_singlePlayerBtn.onClick.AddListener(
				() => _manager.SelectMatchType( MatchType.SinglePlayer )
			);

		if (_coopBtn != null)
			_coopBtn.onClick.AddListener(
				() => _manager.SelectMatchType( MatchType.CoOp )
			);

		if (_multiplayerBtn != null)
			_multiplayerBtn.onClick.AddListener(
				() => _manager.SelectMatchType( MatchType.Multiplayer )
			);
		if (_customsBtn != null)
			_customsBtn.onClick.AddListener(
				() => _manager.SelectMatchType( MatchType.Custom )
			);
		if (_matchTypeBackBtn != null)
			_matchTypeBackBtn.onClick.AddListener(
				() => _manager.BackToMainMenu()
			);
	}

	// =====================
	// GAME MODE MENU
	// =====================

	private void BindGameModeMenu()
	{
		if (_solosBtn != null)
			_solosBtn.onClick.AddListener(
				() => _manager.SelectGameMode( GameMode.Solos )
			);

		if (_duosBtn != null)
			_duosBtn.onClick.AddListener(
				() => _manager.SelectGameMode( GameMode.Duos )
			);

		if (_quadsBtn != null)
			_quadsBtn.onClick.AddListener(
				() => _manager.SelectGameMode( GameMode.Quads )
			);

		if (_ffa4pBtn != null)
			_ffa4pBtn.onClick.AddListener(
				() => _manager.SelectGameMode( GameMode.FFA4P )
			);

		if (_ffa8pBtn != null)
			_ffa8pBtn.onClick.AddListener(
				() => _manager.SelectGameMode( GameMode.FFA8P )
			);
		if (_gameModeBackBtn != null)
			_gameModeBackBtn.onClick.AddListener(
				() => _manager.BackToMatchType()
			);
	}

	private void UnbindButtons()
	{
		_storyBtn.onClick.RemoveAllListeners();
		_fightBtn.onClick.RemoveAllListeners();
		_customizationBtn.onClick.RemoveAllListeners();
		_singlePlayerBtn.onClick.RemoveAllListeners();
		_coopBtn.onClick.RemoveAllListeners();
		_multiplayerBtn.onClick.RemoveAllListeners();
		_customsBtn.onClick.RemoveAllListeners();
		_matchTypeBackBtn.onClick.RemoveAllListeners();
		_solosBtn.onClick.RemoveAllListeners();
		_ffa4pBtn.onClick.RemoveAllListeners();
		_ffa8pBtn.onClick.RemoveAllListeners();
		_duosBtn.onClick.RemoveAllListeners();
		_quadsBtn.onClick.RemoveAllListeners();
		_gameModeBackBtn.onClick.RemoveAllListeners();
	}

	public void FilterGameModes(MatchType type)
	{
		Debug.Assert(
			type != MatchType.None,
			"FilterGameModes called with MatchType.None"
		);

		SetGameModeButton( _solosBtn, IsSolosAllowed( type ) );
		SetGameModeButton( _duosBtn, IsDuosAllowed( type ) );
		SetGameModeButton( _quadsBtn, IsQuadsAllowed( type ) );
		SetGameModeButton( _ffa4pBtn, IsFfaAllowed( type ) );
		SetGameModeButton( _ffa8pBtn, IsFfaAllowed( type ) );
	}
	private void SetGameModeButton(Button button, bool enabled)
	{
		if (button == null)
			return;

		button.interactable = enabled;

		// Optional future hook:
		// button.GetComponent<ComingSoonTag>()?.SetVisible( !enabled );
	}
	private bool IsSolosAllowed(MatchType type)
	{
		return type != MatchType.CoOp;
	}

	private bool IsDuosAllowed(MatchType type)
	{
		return true;
	}

	private bool IsQuadsAllowed(MatchType type)
	{
		return true;
	}

	private bool IsFfaAllowed(MatchType type)
	{
		return type != MatchType.CoOp;
	}
}
