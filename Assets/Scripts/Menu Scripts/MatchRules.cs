public class MatchRules
{
	public static bool IsCombinationValid(MatchType type, GameMode mode)
	{
		if (mode == GameMode.Solos)
		{
			if (type == MatchType.CoOp)
				return false;
			return true;
		}
		if (mode == GameMode.Duos)
		{
			return true;
		}
		if (mode == GameMode.Quads)
		{
			return true;
		}
		if (mode == GameMode.FFA4P)
		{
			if (type == MatchType.CoOp)
				return false;
			return true;
		}
		if (mode == GameMode.FFA8P)
		{
			if (type == MatchType.CoOp)
				return false;
			return true;
		}
		return false;
	}
}
