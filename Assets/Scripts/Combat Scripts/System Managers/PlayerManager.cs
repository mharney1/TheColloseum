using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
	private int _expectedPlayers;
	private readonly List<Character> _idlePlayers = new();

	/// SET UP
	/// <summary>
	/// The below methods are used to prepare the initial idle player list.
	/// </summary>
	public void Initialize()
	{
		_expectedPlayers = GameSession.S_INSTANCE.participants;

		Debug.Log( $"[PLAYER REGISTRATION] {_expectedPlayers} players expected.");
	}

	public bool AllPlayersRegistered()
	{
		return _idlePlayers.Count >= _expectedPlayers;
	}

	/// MODIFIERS
	/// <summary>
	/// The below methods are used to modify the idle list.
	/// </summary>
	public void AddIdlePlayer(Character player)
	{
		if (_idlePlayers.Contains(player))
			return;

		_idlePlayers.Add(player);

		Debug.Log($"[PLAYER REGISTRATION] {_idlePlayers.Count} / {_expectedPlayers} players registerd.");
	}
	public void RemoveIdlePlayer(Character player)
	{
			_idlePlayers.Remove( player );
	}

	public void CleanIdlePlayers()
	{
		_idlePlayers.RemoveAll( p => p == null || !p.IsSpawned );
	}

	/// GETTERS
	/// <summary>
	/// The following methods are used to gether information from the player manager.
	/// </summary>
	public List<Character> GetIdlePlayers()
	{
		return new List<Character>( _idlePlayers );
	}

	public int GetIdlePlayerCount()
	{
		return _idlePlayers.Count;
	}
}
