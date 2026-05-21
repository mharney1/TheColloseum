using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyButtonBinder
{
	private readonly Button _readyButton;
	private readonly Button _leaveButton;
	private readonly Button _startButton;

	public LobbyButtonBinder( Button readyButton, Button leaveButton, Button startButton )
	{
		_readyButton = readyButton;
		_leaveButton = leaveButton;
		_startButton = startButton;
	}

	public void Bind(LobbyManager lobby)
	{
		_readyButton.onClick.AddListener(() =>
		{
			lobby.ToggleReadyServerRpc();
		});

		_leaveButton.onClick.AddListener(async () =>
		{
			await NetworkBootstrap.S_INSTANCE.LeaveGame();

			SceneLoader.S_INSTANCE.LoadScene(
				"Main_Menu"
			);
		});
	}

	public void Unbind()
	{
		_readyButton.onClick.RemoveAllListeners();
		_leaveButton.onClick.RemoveAllListeners();
		_startButton.onClick.RemoveAllListeners();
	}
}
