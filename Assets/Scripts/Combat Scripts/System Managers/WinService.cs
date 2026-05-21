using System.Collections.Generic;

public class WinService
{
	private List<Character> _lastAlive = new();

	private GameMode _mode;
	private CombatDirector _combatDirector;

	public WinService( CombatDirector combatDirector, GameMode mode )
	{
		_mode = mode;
		_combatDirector = combatDirector;
	}

	public bool CheckWinConditions()
	{
		if (PairManager.Instance.GetPairCount() != 0)
			return false;

		if (_combatDirector.PlayerManager.GetIdlePlayerCount() > 1)
		{
			List<Character> idle = _combatDirector.PlayerManager.GetIdlePlayers();

			if (idle [ 0 ].identity.GetTeam() != Team.None)
			{
				HashSet<Team> teams = new();

				foreach (Character p in idle)
				{
					teams.Add(
						p.identity.GetTeam()
					);
				}
				return teams.Count <= 1;
			}
			return false;
		}
		return true;
	}

	public void CacheLastAlive()
	{
		_lastAlive.Clear();

		_lastAlive.AddRange( PairManager.Instance.GetLastAlive() );
	}

	public EndData BuildEndData()
	{
		List<Character> idle = _combatDirector.PlayerManager.GetIdlePlayers();

		bool tie = false;

		bool teamBased = (int)_mode >= (int)GameMode.Duos;

		HashSet<Team> teams = new();
		HashSet<int> players = new();

		if (idle.Count == 0)
		{
			tie = true;

			foreach (Character p in _lastAlive)
			{
				if ( teamBased )
					teams.Add(p.identity.GetTeam());
				else
					players.Add( p.identity.GetCharacterID() );
			} 
		}
		else
		{
			foreach (Character p in idle)
			{
				if ( teamBased )
					teams.Add( p.identity.GetTeam() );
				else
					players.Add( p.identity.GetCharacterID() );
			}
		}

		return new EndData
		{
			Tie = tie,
			TeamBased = teamBased,
			WinningTeams = new List<Team>(teams).ToArray(),
			WinningPlayerIds = new List<int>(players).ToArray()
		};
	}
}
