using System.IO;
using UnityEngine;
using System.Collections.Generic;

public enum MatchType
{
	None,
	SinglePlayer,
	CoOp,
	Multiplayer,
	Custom
}
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

public class GameSession : MonoBehaviour
{
	public static GameSession S_INSTANCE;

	public MatchType matchType = MatchType.None;
	public GameMode gameMode = GameMode.None;
	public int participants = 0;
	public int humanPlayerCount = 0;
	public int aiPlayerCount = 0;
	public List<Player> players = new List<Player>();

	private void Awake()
	{
		if (S_INSTANCE == null)
		{
			S_INSTANCE = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}
	public void ResetSession()
	{
		gameMode = GameMode.None;
		matchType = MatchType.None;
		participants = 0;
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
}
