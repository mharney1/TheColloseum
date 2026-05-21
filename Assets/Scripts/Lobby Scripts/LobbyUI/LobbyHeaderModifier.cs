using TMPro;
using UnityEngine;

public class LobbyHeaderModifier
{
	private readonly TextMeshProUGUI _matchTypeText;
	private readonly TextMeshProUGUI _gameModeText;
	private readonly TextMeshProUGUI _lobbyStatusText;
	private readonly TextMeshProUGUI _timerText;

	public LobbyHeaderModifier(
		TextMeshProUGUI matchTypeText,
		TextMeshProUGUI gameModeText,
		TextMeshProUGUI lobbyStatusText,
		TextMeshProUGUI timerText
	)
	{
		_matchTypeText = matchTypeText;
		_gameModeText = gameModeText;
		_lobbyStatusText = lobbyStatusText;
		_timerText = timerText;
	}

	public void SetHeader(LobbyManager lobby)
	{
		var session = GameSession.S_INSTANCE;

		if (session == null)
			return;

		_matchTypeText.text = session.matchType.ToString();

		_gameModeText.text = session.gameMode.ToString();

		UpdateHeader(lobby);
	}

	public void UpdateHeader(LobbyManager lobby)
	{
		if (lobby == null)
			return;

		_lobbyStatusText.text = ((LobbyState)lobby.CurrentState.Value).ToString();

		_timerText.text = FormatTimer(lobby.TimeRemaining.Value);
	}

	private string FormatTimer(float seconds)
	{
		int secs = Mathf.FloorToInt(seconds);

		return $"{secs:00}";
	}
}
