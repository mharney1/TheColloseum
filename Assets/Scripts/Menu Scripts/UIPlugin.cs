using UnityEngine;
using UnityEngine.UI;

public class MenuButtonBinder : MonoBehaviour
{
	[Header( "Main Menu Buttons" )]
	[SerializeField] private Button _storyBtn;
	[SerializeField] private Button _fightBtn;
	[SerializeField] private Button _customizationBtn;

	[Header( "Match Type Buttons" )]
	[SerializeField] private Button _singlePlayerBtn;
	[SerializeField] private Button _coopBtn;
	[SerializeField] private Button _multiplayerBtn;
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
		FlowController.S_INSTANCE.RegisterMenuBinder( this );

		BindMainMenu();
		BindMatchTypeMenu();
		BindGameModeMenu();
	}

	// =====================
	// MAIN MENU
	// =====================

	private void BindMainMenu()
	{
		if (_fightBtn != null)
			_fightBtn.onClick.AddListener(
				() => FlowController.S_INSTANCE.OnFightSelected()
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
				() => FlowController.S_INSTANCE.SelectMatchType( MatchType.SinglePlayer )
			);

		if (_coopBtn != null)
			_coopBtn.onClick.AddListener(
				() => FlowController.S_INSTANCE.SelectMatchType( MatchType.CoOp )
			);

		if (_multiplayerBtn != null)
			_multiplayerBtn.onClick.AddListener(
				() => FlowController.S_INSTANCE.SelectMatchType( MatchType.Multiplayer )
			);
		if (_matchTypeBackBtn != null)
			_matchTypeBackBtn.onClick.AddListener(
				() => FlowController.S_INSTANCE.BackToMainMenu()
			);

	}

	// =====================
	// GAME MODE MENU
	// =====================

	private void BindGameModeMenu()
	{
		if (_solosBtn != null)
			_solosBtn.onClick.AddListener(
				() => FlowController.S_INSTANCE.SelectGameMode( GameMode.Solos )
			);

		if (_duosBtn != null)
			_duosBtn.onClick.AddListener(
				() => FlowController.S_INSTANCE.SelectGameMode( GameMode.Duos )
			);

		if (_quadsBtn != null)
			_quadsBtn.onClick.AddListener(
				() => FlowController.S_INSTANCE.SelectGameMode( GameMode.Quads )
			);

		if (_ffa4pBtn != null)
			_ffa4pBtn.onClick.AddListener(
				() => FlowController.S_INSTANCE.SelectGameMode( GameMode.FFA4P )
			);

		if (_ffa8pBtn != null)
			_ffa8pBtn.onClick.AddListener(
				() => FlowController.S_INSTANCE.SelectGameMode( GameMode.FFA8P )
			);
		if (_gameModeBackBtn != null)
			_gameModeBackBtn.onClick.AddListener(
				() => FlowController.S_INSTANCE.BackToMatchType()
			);
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
