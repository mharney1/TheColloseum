using UnityEngine;

public class MenuUIManager : MonoBehaviour 
{
	[Header( "Panels" )]
	[SerializeField] private GameObject _mainMenuPanel;
	[SerializeField] private GameObject _matchTypePanel;
	[SerializeField] private GameObject _gameModePanel;

	[SerializeField] private MenuButtonBinder _binder;
	private MatchConfigurationResolver _configurationResolver = new();

	public void OnFightSelected()
	{
		GameSession.S_INSTANCE.ResetSession();

		_mainMenuPanel.SetActive( false );
		_matchTypePanel.SetActive( true );
	}

	public void SelectMatchType(MatchType type)
	{
		GameSession.S_INSTANCE.matchType = type;

		_matchTypePanel.SetActive( false );
		_gameModePanel.SetActive( true );

		_binder.RefreshGameModeAvailability( type );
	}

	public void BackToMainMenu()
	{
		GameSession.S_INSTANCE.ResetSession();

		_matchTypePanel.SetActive( false );
		_mainMenuPanel.SetActive( true );
	}

	public void SelectGameMode(GameMode mode)
	{
		GameSession.S_INSTANCE.gameMode = mode;
		_configurationResolver.ConfigureSession(GameSession.S_INSTANCE);

#if UNITY_EDITOR
		MenuTester.ValidateSession(true);
#endif

		NetworkBootstrap.S_INSTANCE.QuickMatch();
	}

	public void BackToMatchType()
	{
		GameSession.S_INSTANCE.matchType = MatchType.None;

		_gameModePanel.SetActive( false );
		_matchTypePanel.SetActive( true );
	}
}
