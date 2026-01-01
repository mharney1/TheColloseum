using UnityEngine;
using UnityEngine.UI;
public class FlowController : MonoBehaviour
{
	public static FlowController S_INSTANCE;
	private MenuButtonBinder _menuBinder;

	[Header( "Menu Panels" )]
	public GameObject mainMenuPanel;
	public GameObject matchTypePanel;
	public GameObject gameModePanel;

	private void Awake()
	{
		if (S_INSTANCE == null)
		{
			S_INSTANCE = this;
			DontDestroyOnLoad( gameObject );
		}
		else
		{
			Destroy( gameObject );
		}
	}
	public void RegisterMenuBinder(MenuButtonBinder binder)
	{
		Debug.Assert(
			_menuBinder == null,
			"MenuButtonBinder registered more than once"
		);

		_menuBinder = binder;
	}
	public void OnFightSelected()
	{
		GameSession.S_INSTANCE.ResetSession();

		mainMenuPanel.SetActive( false );
		matchTypePanel.SetActive( true );
	}
	public void SelectMatchType(MatchType type)
	{
		GameSession.S_INSTANCE.matchType = type;

		matchTypePanel.SetActive( false );
		gameModePanel.SetActive( true );

		_menuBinder.FilterGameModes( type );
	}
	public void BackToMainMenu()
	{
		GameSession.S_INSTANCE.ResetSession();

		matchTypePanel.SetActive( false );
		mainMenuPanel.SetActive( true );
	}
	public void SelectGameMode(GameMode mode)
	{
		GameSession.S_INSTANCE.gameMode = mode;
		ResolveTotalSlots();
		ResolveParticipants();
		GameSession.S_INSTANCE.DebugPrint();
	}

	public void BackToMatchType()
	{
		GameSession.S_INSTANCE.matchType = MatchType.None;

		gameModePanel.SetActive( false );
		matchTypePanel.SetActive( true );
	}
	public void BackToGameMode()
	{
		GameSession.S_INSTANCE.gameMode = GameMode.None;

		//Placeholder will need to return to the mainmenu scene and set gamemodepanel active
	}
	private void ResolveTotalSlots()
	{
		int players = 0;
		switch ( GameSession.S_INSTANCE.gameMode )
		{
			case GameMode.Solos:
				players = 2;
				break;
			case GameMode.Duos:
				players = 4;
				break;
			case GameMode.Quads:
				players = 8;
				break;
			case GameMode.FFA4P:
				players = 4;
				break;
			case GameMode.FFA8P:
				players = 8;
				break;
			default:
				players = 0;
				break;
		}
		GameSession.S_INSTANCE.totalPlayerCount = players;
	}
	private void ResolveParticipants()
	{
		int total = GameSession.S_INSTANCE.totalPlayerCount;
		int players = 0;
		int ai = 0;
		switch (GameSession.S_INSTANCE.matchType)
		{
			case MatchType.Multiplayer:
				players = total;
				ai = 0;
				break;
			case MatchType.CoOp:
				players = total / 2;
				ai = total / 2;
				break;
			case MatchType.SinglePlayer:
				players = 1;
				ai = total - 1;
				break;
			default:
				players = 0;
				ai = 0;
				break;
		}
		GameSession.S_INSTANCE.humanPlayerCount = players;
		GameSession.S_INSTANCE.aiPlayerCount = ai;
	}
}
