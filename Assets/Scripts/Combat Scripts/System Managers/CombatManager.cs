using System.Collections;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
	public static CombatManager Instance{ get; private set; }
	public bool Resolving;
	public bool Resolved;

	public void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy( gameObject );
			return;
		}
		Instance = this;
	}

	/// ACTION RESOLUTION
	/// <summary>
	/// These methods act as the core orchestrators for action resolution.
	/// </summary>
	public void BeginResolvingPairs()
	{
		if (Resolving)
			return;

		StartCoroutine(ResolvePairsRoutine());
	}

	public IEnumerator ResolvePairsRoutine()
	{
		Resolving = true;
		Resolved = false;
		try
		{
			var pairs = PairManager.Instance.GetPairs();
			foreach (var pair in pairs)
			{
				Character p1 = pair.Item1; 
				Character p2 = pair.Item2;

				yield return Engage(p1, p2);

				CombatOutcome outcome = CombatResolver.Resolve(
					p1.combat.GetChoice(),
					p2.combat.GetChoice(),
					p1.stats.GetExhaustion(),
					p2.stats.GetExhaustion());

				ApplyOutcome(p1, p2, outcome);

				yield return Disengage(p1, p2);
			}
			Resolved = true;
		}
		finally
		{
			Resolving = false;
		}
	}

	private void ApplyOutcome(Character p1, Character p2, CombatOutcome outcome)
	{
		p1.stats.ModifyStats( Mathf.RoundToInt( outcome.healthDeltaP1 ), outcome.exhaustionDeltaP1, outcome.dizzyP1);
		p2.stats.ModifyStats( Mathf.RoundToInt( outcome.healthDeltaP2 ), outcome.exhaustionDeltaP2, outcome.dizzyP2);	
	}

	/// MOVEMENT
	/// <summary>
	/// These methods handle movement to and from combat.
	/// </summary>
	private IEnumerator Engage(Character p1, Character p2)
	{
		Vector3 midpoint = (p1.transform.position + p2.transform.position) * 0.5f;
		Vector3 dir1 = (midpoint - p1.transform.position).normalized;
		Vector3 dir2 = (midpoint - p2.transform.position).normalized;

		float stopDistance = 1f;
		Vector3 p1Target = midpoint - dir1 * stopDistance;
		Vector3 p2Target = midpoint - dir2 * stopDistance;

		p1.movement.MoveTo(p1Target);
		p2.movement.MoveTo(p2Target);

		yield return WaitForMovement(p1, p2);
	}

	private IEnumerator Disengage(Character p1, Character p2)
	{
		if (!p1.stats.IsDefeated() && !p2.stats.IsDefeated())
		{
			p1.movement.ReturnToAnchor();
			p2.movement.ReturnToAnchor();
		}
		else
		{
			if (p1.stats.IsDefeated())
			{
				yield return StartCoroutine(PairManager.Instance.RemovePair(p1.identity.GetPair()));
			}
			else
			{
				yield return StartCoroutine(PairManager.Instance.RemovePair(p2.identity.GetPair()));
			}
		}
		yield return WaitForMovement(p1, p2);
	}

	private IEnumerator WaitForMovement( Character p1, Character p2 )
	{
		float timeout = Time.time + 10f;
		yield return new WaitUntil(() => Time.time > timeout ||
			((p1 == null || !p1.movement.IsMoving()) && (p2 == null || !p2.movement.IsMoving())));
	}
}
