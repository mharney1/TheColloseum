public class MatchConfigurationResolver
{
	public void ConfigureSession(GameSession session)
	{
		ResolveTotalSlots(session);
		ResolveParticipants(session);
	}
	private void ResolveTotalSlots(GameSession session)
	{
		int players = 0;
		switch (session.gameMode)
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
		session.participants = players;
	}
	private void ResolveParticipants(GameSession session)
	{
		int total = session.participants;
		int players = 0;
		int ai = 0;
		switch (session.matchType)
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
			case MatchType.Custom:
				players = total;
				ai = 0;
				break;
			default:
				players = 0;
				ai = 0;
				break;
		}
		session.humanPlayerCount = players;
		session.aiPlayerCount = ai;
	}
}
