using System.IO;
using UnityEngine;

public enum GameMode
{
	//keep all teammodes at the bottom with duos first for winmanager( previously line 67)
	None,
	Solos,
	FFA4P,
	FFA8P,
	Duos,
	Quads
}
public enum MatchType
{
	None,
	SinglePlayer,
	CoOp,
	Multiplayer,
	Custom
}
public enum LobbyType
{
	Public,
	Custom
}
public enum LobbyVisibility
{
	Public,
	Private
}


public class GameSession : MonoBehaviour
{
	public static GameSession S_INSTANCE;

	public GameMode gameMode = GameMode.None;
	public MatchType matchType = MatchType.None;
	public int totalPlayerCount = 0;
	public int humanPlayerCount = 0;
	public int aiPlayerCount = 0;

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
	public void ResetSession()
	{
		gameMode = GameMode.None;
		matchType = MatchType.None;
		totalPlayerCount = 0;
		humanPlayerCount = 0;
		aiPlayerCount = 0;
}
	public bool IsSelectionValid()
	{
		if (matchType == MatchType.None)
			return false;

		if (gameMode == GameMode.None)
			return false;

		switch (matchType)
		{
			case MatchType.CoOp:
				return gameMode == GameMode.Duos
					|| gameMode == GameMode.Quads;

			case MatchType.SinglePlayer:
			case MatchType.Multiplayer:
				return true;
		}

		return false;
	}
	public void DebugPrint()
	{
		bool isValid = IsSelectionValid();
		GameSessionValidator.ValidateFullSession();
		Debug.Log(
			$"[GameSession]\n" +
			$"MatchType: {matchType}\n" +
			$"GameMode: {gameMode}\n" +
			$"Total Slots: {totalPlayerCount}\n" +
			$"Humans: {humanPlayerCount}\n" +
			$"AI: {aiPlayerCount}\n" +
			$"VALID: {isValid}"
		);
	}
}
