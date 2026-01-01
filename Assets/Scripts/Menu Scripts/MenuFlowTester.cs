#if UNITY_EDITOR
using UnityEngine;

public class MenuFlowTester : MonoBehaviour
{
	[ContextMenu( "Test All Menu Combinations" )]
	private void TestAllCombinations()
	{
		GameSession gameSession = new GameSession();
		foreach (MatchType matchType in System.Enum.GetValues( typeof( MatchType ) ))
		{
			if (matchType == MatchType.None)
				continue;

			foreach (GameMode gameMode in System.Enum.GetValues( typeof( GameMode ) ))
			{
				if (gameMode == GameMode.None)
					continue;

				gameSession.matchType = matchType;
				gameSession.gameMode = gameMode;
				bool valid = gameSession.IsSelectionValid();

				Debug.Log(
					$"[TEST] {matchType} + {gameMode} => {(valid ? "VALID" : "INVALID")}"
				);
			}
		}
	}
}
public static class GameSessionValidator
{
	public static bool ValidateFullSession()
	{
		if (!ValidateMatchType())
			return false;
		if (!ValidateGameMode())
			return false;
		if (!ValidateTotalSlots())
			return false;
		if (!ValidateParticipants())
			return false;
		Debug.Log( "Session Test Successful" );
		return true;
	}
	public static bool ValidateMatchType()
	{
		if (GameSession.S_INSTANCE.matchType == MatchType.None)
		{
			Debug.LogError( "Validation Failed: MatchType is None" );
			return false;
		}
		return true;
	}

	public static bool ValidateGameMode()
	{
		if (GameSession.S_INSTANCE.gameMode == GameMode.None)
		{
			Debug.LogError( "Validation Failed: GameMode is None" );
			return false;
		}
		return true;
	}

	public static bool ValidateTotalSlots()
	{
		if (GameSession.S_INSTANCE.totalPlayerCount <= 0)
		{
			Debug.LogError(
				$"Validation Failed: totalPlayerCount invalid ({GameSession.S_INSTANCE.totalPlayerCount})"
			);
			return false;
		}
		return true;
	}

	public static bool ValidateParticipants()
	{
		int total = GameSession.S_INSTANCE.totalPlayerCount;
		int players = GameSession.S_INSTANCE.humanPlayerCount;
		int ai = GameSession.S_INSTANCE.aiPlayerCount;

		if (players < 0 || ai < 0)
		{
			Debug.LogError(
				$"Validation Failed: Negative participant count (players:{players}, ai:{ai})"
			);
			return false;
		}

		if (players + ai != total)
		{
			Debug.LogError(
				$"Validation Failed: players + ai != total ({players}+{ai}!={total})"
			);
			return false;
		}

		return true;
	}
}
#endif
