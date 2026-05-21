using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PairManager : MonoBehaviour
{
	public static PairManager Instance
	{
		get; private set;
	}

	public bool Pairing;
	public bool Paired;
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
		Instance = this;
	}

	/// PAIR CONSTRUCTORS
	/// <summary>
	/// These methods work together to create pairs.
	/// </summary>
	public void BeginCreatingPairs()
	{
		if (Pairing)
			return;

		StartCoroutine(CreatePairsRoutine());
	}

	public IEnumerator CreatePairsRoutine()
	{
		Pairing = true;
		Paired = false;
		try
		{

			CombatDirector.S_INSTANCE.PlayerManager.CleanIdlePlayers();
			bool PairMade = false;
			while (TryCreatingPair())
			{
				PairMade = true;
			}
			if (PairMade)
			{
				yield return StartCoroutine(AnchorManager.Instance.AnchorPairs(new Dictionary<int, (Character, Character)>(_pairs)));
			}

			Paired = true;
		}
		finally
		{
			Pairing = false;
		}
	}

	private bool TryCreatingPair()
	{
		var idle = CombatDirector.S_INSTANCE.PlayerManager.GetIdlePlayers();
		for (int i = 0; i < idle.Count - 1; i++)
		{
			for (int j = i + 1; j < idle.Count; j++)
			{
				Debug.Log($"{CanPair(idle [ i ], idle [ j ])} {idle [ i ].name} {idle [ j ].name} ");
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
		return p1.identity.GetTeam() == Team.None || p1.identity.GetTeam() != p2.identity.GetTeam();
	}

	private void CreatePair(Character p1, Character p2)
	{
		Debug.Log("Pair Created");
		int key = _freeKeys.Count > 0 ? _freeKeys.Dequeue() : _nextKey++;
		_pairs [ key ] = (p1, p2);

		p1.combat.SetOpponentClientRPC(p2.identity.GetCharacterID());
		p1.identity.SetPair( key );
		p2.combat.SetOpponentClientRPC(p1.identity.GetCharacterID());
		p2.identity.SetPair( key );

		CombatDirector.S_INSTANCE.PlayerManager.RemoveIdlePlayer( p1 );
		CombatDirector.S_INSTANCE.PlayerManager.RemoveIdlePlayer( p2 );
	}

	/// PAIR DECONSTRUCTORS
	/// <summary>
	/// These methods work together to dissolve pairs.
	/// </summary>
	public IEnumerator RemovePair(int key)
	{
		(Character c1, Character c2) = _pairs [ key ];

		bool done1 = false;
		bool done2 = false;

		StartCoroutine(RemoveRoutine(c1, () => done1 = true));
		StartCoroutine(RemoveRoutine(c2, () => done2 = true));

		yield return new WaitUntil(() => done1 && done2);

		_pairs.Remove( key );
		_freeKeys.Enqueue( key );
	}

	private IEnumerator RemoveRoutine( Character c, System.Action onComplete)
	{
		yield return RemoveCharacterFromPair(c);

		onComplete?.Invoke();
	}

	public IEnumerator RemoveCharacterFromPair(Character player)
	{

		Vector3 direction = (player.transform.position - Vector3.zero).normalized;
		Vector3 targetPosition = Vector3.zero + direction * (AnchorManager.Instance.GetRadius() + 2f);

		player.movement.MoveTo( targetPosition );

		player.identity.SetPair( -1 );
		player.combat.ClearOpponentClientRpc();

		NetworkObject anchor = player.transform.parent?.GetComponent<NetworkObject>();

		player.movement.ClearAnchor();

		if (anchor != null && anchor.IsSpawned)
		{
			anchor.Despawn();
		}

		yield return new WaitUntil( () => !player.movement.IsMoving() );

		if (!player.stats.IsDefeated())
		{
			CombatDirector.S_INSTANCE.PlayerManager.AddIdlePlayer( player );
		}
	}

	/// GETTERS
	/// <summary>
	/// These methods expose internal data to be used by other classes.
	/// </summary>
	public int GetPairCount()
	{
		return _pairs.Count;
	}

	public List<(Character, Character)> GetPairs()
	{
		return new List<(Character, Character)>(_pairs.Values);
	}

	public void GetCombatants( List<Character> undecided )
	{
		foreach (var pair in _pairs.Values)
		{
			if(!undecided.Contains( pair.Item1 ))
				undecided.Add( pair.Item1 );
			if(!undecided.Contains( pair.Item2 ))
				undecided.Add( pair.Item2 );
		}
	}

	public List<Character> GetLastAlive()
	{
		var idle = CombatDirector.S_INSTANCE.PlayerManager.GetIdlePlayers();
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

	public bool AreCharactersReady()
	{
		foreach (var pair in _pairs.Values)
		{
			if (pair.Item1.movement.IsMoving())
				return false;

			if (pair.Item2.movement.IsMoving())
				return false;
		}
		return true;
	}
}
