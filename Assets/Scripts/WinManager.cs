using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class WinManager : NetworkBehaviour
{
	public static WinManager Instance
	{
		get; private set;
	}

	public static event Action<EndData> GameEnd;

	private List<Character> _lastAlive;
	private Modes _mode;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy( gameObject );
			return;
		}

		Instance = this;
		_mode = Modes.FFA;
	}

	public bool CheckWinConditions()
	{
		if (PairManager.Instance.GetPairCount() == 0)
		{
			if (PlayerManager.Instance.GetIdlePlayerCount() > 1)
			{
				List<Character> idle = PlayerManager.Instance.GetIdlePlayers();
				if (idle [ 0 ].identity.team != -1)
				{
					var teams = new HashSet<int>();
					foreach (var player in idle)
					{
						teams.Add( player.identity.team );
					}
					if (teams.Count <= 1)
					{
						Debug.Log( "1" );
						return true;
					}
				}
			}
			else
			{
				return true;
			}
		}
		return false;
	}
	public void GetLastAlive()
	{
		_lastAlive = PairManager.Instance.GetLastAlive();
	}
	public void FindWinners()
	{
		List<Character> idle = PlayerManager.Instance.GetIdlePlayers();
		bool tie = false;
		bool teamBased = (_mode == Modes.Team || _mode == Modes.MultiTeam);
		var teamSet = new HashSet<int>();
		var playerSet = new HashSet<ulong>();


		if (idle.Count == 0)
		{
			tie = true;

			foreach (Character player in _lastAlive)
			{
				if (teamBased)
					teamSet.Add( player.identity.team );
				else
					playerSet.Add( player.OwnerClientId );
			}
		}
		else
		{
			foreach (Character player in idle)
			{
				if (teamBased)
					teamSet.Add( player.identity.team );
				else
					playerSet.Add( player.OwnerClientId );
			}
		}
		AnnounceWinnersClientRpc( tie, teamBased, teamSet.ToArray(), playerSet.ToArray() );
	}
	[ClientRpc]
	private void AnnounceWinnersClientRpc(bool tie, bool teamBased, int [] winningTeams, ulong [] winningPlayerIds)
	{
		GameEnd?.Invoke( new EndData
		{
			Tie = tie,
			TeamBased = teamBased,
			WinningTeams = winningTeams,
			WinningPlayerIds = winningPlayerIds
		} );
	}
}
