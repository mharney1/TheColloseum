using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
	public static PlayerManager Instance
	{
		get; private set;
	}

	private int _expectedPlayers = 4;
	private readonly List<Character> _idlePlayers = new();

	public void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy( gameObject );
			return;
		}
	}
	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		if (!IsServer)
		{
			enabled = false;
			return;
		}
		Instance = this;
	}
	public bool AllPlayersRegistered()
	{
		return _idlePlayers.Count >= _expectedPlayers;
	}
	public void AddIdlePlayer(Character player)
	{
		if (IsServer && !_idlePlayers.Contains( player ))
		{
			_idlePlayers.Add( player );
		}
	}
	public void RemoveIdlePlayer(Character player)
	{
		if (IsServer && _idlePlayers.Contains( player ))
		{
			_idlePlayers.Remove( player );
		}
	}
	public void CleanIdlePlayers()
	{
		_idlePlayers.RemoveAll( p => p == null || !p.IsSpawned );
	}
	public List<Character> GetIdlePlayers()
	{
		return new List<Character>( _idlePlayers );
	}
	public int GetIdlePlayerCount()
	{
		return _idlePlayers.Count;
	}
	public IEnumerator IsMoving(Character c1, Character c2)
	{
		float timeout = Time.time + 10f;
		yield return new WaitUntil( () => Time.time > timeout
			|| ( (c1 == null || !c1.movement.IsMoving()) && (c2 == null || !c2.movement.IsMoving()) ) );
		Debug.Log( $"Pair {c1.name} / {c2.name} anchored" );
	}
}
