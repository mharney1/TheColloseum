using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AnchorManager : MonoBehaviour
{
	public static AnchorManager Instance
	{
		get; private set;
	}

	[Header( "Circle Settings" )]
	private readonly float _radius = 5f;
	[SerializeField] private NetworkObject _anchorPrefab;
	private readonly float _rotationSpeed = 25f; // degrees per second
	private readonly Dictionary<ulong, Transform> _assignments = new();
	private readonly List<NetworkObject> _anchors = new();

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy( gameObject );
			return;
		}
		Instance = this;
	}
	private void Update()
	{
		if (PhaseManager.Instance == null || (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer) )
			return;

		if (PhaseManager.Instance.CurrentPhase.Value == Phases.Prepare)
		{
			transform.Rotate(
				Vector3.up,
				_rotationSpeed * Time.deltaTime,
				Space.World
			);
		}
	}
	public IEnumerator AnchorPairs(Dictionary<int, (Character, Character)> pairs)
	{
		GenerateAnchors( pairs.Count );
		AssignAnchors( pairs );

		foreach (var pair in pairs.Values)
		{
			pair.Item1.movement.MoveToAnchor( GetAnchorFor( pair.Item1 ) );
			pair.Item2.movement.MoveToAnchor( GetAnchorFor( pair.Item2) );

		}
		yield return new WaitUntil( () =>
		{
			foreach (var pair in pairs.Values)
			{
				if (pair.Item1.movement.IsMoving() || pair.Item2.movement.IsMoving())
					return false;
			}
			return true;
		} );
	}
	private void GenerateAnchors(int pairCount)
	{
		if (!NetworkManager.Singleton.IsServer)
			return;

		// Clear existing anchors first
		foreach (var anchor in _anchors)
		{
			if (anchor != null && anchor.IsSpawned)
				anchor.Despawn();
		}
		_anchors.Clear();
		_assignments.Clear();

		// Each pair = 2 anchors (across from each other)
		int totalAnchors = pairCount * 2;
		float angleStep = 360f / totalAnchors;

		for (int i = 0; i < totalAnchors; i++)
		{
			NetworkObject anchorObj = Instantiate( _anchorPrefab ).GetComponent<NetworkObject>();
			anchorObj.name = $"Anchor_{i}";
			float angle = i * angleStep * Mathf.Deg2Rad;
			anchorObj.transform.localPosition = new Vector3(
					Mathf.Cos( angle ),
					0,
					Mathf.Sin( angle )
				) * _radius;

			anchorObj.transform.localRotation = Quaternion.LookRotation( -anchorObj.transform.localPosition.normalized, Vector3.up );

			anchorObj.Spawn();
			anchorObj.transform.SetParent( transform, false );
			_anchors.Add( anchorObj );
		}
		Debug.Assert( _anchors.Count == pairCount * 2, "Anchor count doesn't match character count." );
		Debug.Assert( _anchors.Count % 2 == 0, "Anchor count is not an even number." );
	}
	private void AssignAnchors(Dictionary<int, (Character, Character)> pairs)
	{
		if (!NetworkManager.Singleton.IsServer)
			return;

		int i = 0;
		foreach (var pair in pairs.Values)
		{
			_assignments [ pair.Item1.OwnerClientId ] = _anchors [ i ].transform;
			_assignments [ pair.Item2.OwnerClientId ] = _anchors [ i + pairs.Count ].transform;
			Debug.Assert( Vector3.Distance(
				_anchors [ i ].transform.localPosition, -_anchors [ i + pairs.Count ].transform.localPosition ) < 0.01f,
				"Anchor symmetry broken." );
			i++;

		}
	}
	private Transform GetAnchorFor(Character p)
	{
		ulong playerID = p.OwnerClientId;
		if (_assignments.TryGetValue( playerID, out Transform anchor ))
			return anchor;

		Debug.Assert( false, $"No anchor assigned for player {p.name}" );
		return null;
	}
	public int GetAnchorCount() => _anchors.Count;
	public float GetRadius()
	{
		return _radius;
	}
}
