#if UNITY_EDITOR

using UnityEngine;

public static class MenuTester{
	/// INITIAL MENU TEST
	/// <summary>
	/// Tests all combinations of match type and game mode for validity at game's start against
	/// preset rules. To be used as a comparison against the UI when navigating the menu.
	/// </summary>
	[ RuntimeInitializeOnLoadMethod]
	private static void RunStartupValidation()
	{
		Debug.Log("========== MENU SESSION VALIDATION START ==========");

		RunInitialMenuValidation();

		Debug.Log("========== MENU SESSION VALIDATION END ==========");
	}
	/// SESSION MUTABILITY TEST
	/// <summary>
	/// Test the full pipeline and modification of the game session to ensure that it is producing valid games sessions at all times.
	/// </summary>
	private static void RunInitialMenuValidation()
	{
		MatchConfigurationResolver configurationResolver = new();
		foreach (MatchType matchType in System.Enum.GetValues(typeof(MatchType)))
		{
			if (matchType == MatchType.None)
				continue;

			foreach (GameMode gameMode in System.Enum.GetValues(typeof(GameMode)))
			{
				if (gameMode == GameMode.None)
					continue;

				// SETUP SESSION
				GameSession session = GameSession.S_INSTANCE;
				session.ResetSession();

				session.matchType = matchType;
				session.gameMode = gameMode;

				//COMBINATION TEST
				bool validRules = MatchRules.IsCombinationValid(matchType, gameMode);

				Debug.Log( $"[COMBINATION RULE TEST] {matchType} + {gameMode} => " +
					$"{(validRules ? "VALID" : "INVALID")}" );

				configurationResolver.ConfigureSession(session);

				//PIPELINE TEST
				bool validSession = ValidateSession(false);

				Debug.Log( $"[SESSION PIPELINE TEST] {matchType} + {gameMode} => " +
					$"{(validSession ? "VALID" : "INVALID")}" );
			}
		}
		GameSession.S_INSTANCE.ResetSession();
	}
	/// SESSION VALIDATION
	/// <summary>
	/// Validates the aspects of the current session for empty and contradicting values.
	/// </summary>
	/// <returns></returns>
	public static bool ValidateSession(bool print)
	{
		GameSession session = GameSession.S_INSTANCE;

		bool isValid = true;

		isValid &= ValidateMatchType(session.matchType);
		isValid &= ValidateGameMode(session.gameMode);
		isValid &= ValidateParticipants(session.participants, session.humanPlayerCount, session.aiPlayerCount);
		isValid &= MatchRules.IsCombinationValid(session.matchType, session.gameMode);

		if (print)
		{
			if (isValid)
			{
				Debug.Log("[SESSION VALIDATION] SUCCESS");
			}
			else
			{
				Debug.LogError("[SESSION VALIDATION] FAILED");
			}
		}

		return isValid;
	}
	/// VALIDATION HELPERS
	/// <summary>
	/// Each of the methods below are used to test a different aspect of a session.
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	private static bool ValidateMatchType(MatchType type)
	{
		if (type == MatchType.None)
		{
			Debug.LogError(
				"[SESSION VALIDATION] MatchType is None"
			);

			return false;
		}

		return true;
	}
	private static bool ValidateGameMode(GameMode mode)
	{
		if (mode == GameMode.None)
		{
			Debug.LogError(
				"[SESSION VALIDATION] GameMode is None"
			);

			return false;
		}

		return true;
	}
	private static bool ValidateParticipants(int participants, int humanPlayers, int aiPlayers)
	{

		if (humanPlayers < 0 || aiPlayers < 0)
		{
			Debug.LogError(
				$"[SESSION VALIDATION] Negative participant count | " +
				$"Players: {humanPlayers} | AI: {aiPlayers}"
			);

			return false;
		}

		if (humanPlayers + aiPlayers != participants)
		{
			Debug.LogError(
				$"[SESSION VALIDATION] Participant mismatch | " +
				$"{humanPlayers} + {aiPlayers} != {participants}"
			);

			return false;
		}

		return true;
	}
}
#endif
