using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PairManager : NetworkBehaviour
{
	public static PairManager Instance
	{
		get; private set;
	}

	private int _nextKey = 0;
	private readonly Queue<int> _freeKeys = new Queue<int>();
	private readonly Dictionary<int, (Character, Character)> _pairs = new();

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
	public List<(Character, Character)> GetAllPairs()
	{
		return new List<(Character, Character)>( _pairs.Values );
	}
	public int GetPairCount()
	{
		return _pairs.Count;
	}
	public IEnumerator CreatePossiblePairs()
	{
		PlayerManager.Instance.CleanIdlePlayers();
		bool PairMade = false;
		while (TryCreatingPair())
		{
			PairMade = true;
		}
		if (PairMade)
		{
			yield return StartCoroutine( AnchorManager.Instance.AnchorPairs( new Dictionary<int, (Character, Character)>( _pairs ) ) );
		}
	}
	private bool TryCreatingPair()
	{
		var idle = PlayerManager.Instance.GetIdlePlayers();
		for (int i = 0; i < idle.Count - 1; i++)
		{
			for (int j = i + 1; j < idle.Count; j++)
			{
				if (!CanPair( idle [ i ], idle [ j ] ))
					continue;

				CreatePair( idle [ i ], idle [ j ] );
				return true;
			}
		}
		return false;
	}
	private bool CanPair(Character p1, Character p2)
	{
		return p1.identity.team == -1 || p1.identity.team != p2.identity.team;
	}
	private void CreatePair(Character p1, Character p2)
	{
		int key = _freeKeys.Count > 0 ? _freeKeys.Dequeue() : _nextKey++;
		_pairs [ key ] = (p1, p2);

		p1.combat.SetOpponentClientRPC( p2.OwnerClientId );
		p1.identity.SetPair( key );
		p2.combat.SetOpponentClientRPC( p1.OwnerClientId );
		p2.identity.SetPair( key );

		PlayerManager.Instance.RemoveIdlePlayer( p1 );
		PlayerManager.Instance.RemoveIdlePlayer( p2 );
	}
	public IEnumerator RemovePair(int key)
	{
		(Character c1, Character c2) = _pairs [ key ];

		Coroutine r1 = StartCoroutine( RemoveCharacterFromPair( c1 ) );
		Coroutine r2 = StartCoroutine( RemoveCharacterFromPair( c2 ) );

		yield return r1;
		yield return r2;

		_pairs.Remove( key );
		_freeKeys.Enqueue( key );
	}
	public IEnumerator RemoveCharacterFromPair(Character player)
	{

		Vector3 direction = (player.transform.position - Vector3.zero).normalized;
		Vector3 targetPosition = Vector3.zero + direction * (AnchorManager.Instance.GetRadius() + 2f);

		player.movement.MoveTo( targetPosition );

		player.identity.SetPair( -1 );
		player.combat.ClearOpponentClientRpc();

		NetworkObject anchor = player.transform.parent?.GetComponent<NetworkObject>();

		player.transform.SetParent( null, true );

		if (anchor != null && anchor.IsSpawned)
		{
			anchor.Despawn();
		}

		yield return new WaitUntil( () => !player.movement.IsMoving() );

		if (!player.stats.IsDefeated())
		{
			PlayerManager.Instance.AddIdlePlayer( player );
		}
	}
	public void GetCombatants()
	{
		foreach (var pair in _pairs.Values)
		{
			PhaseManager.Instance.AddUndecided( pair.Item1 );
			PhaseManager.Instance.AddUndecided( pair.Item2 );
		}
	}
	public List<Character> GetLastAlive()
	{
		var idle = PlayerManager.Instance.GetIdlePlayers();
		var alive = new List<Character>(
			GetPairCount() * 2 + idle.Count
		);

		foreach (var pair in _pairs.Values)
		{
			alive.Add( pair.Item1 );
			alive.Add( pair.Item2 );
		}
		alive.AddRange( idle );

		return alive;
	}
}
