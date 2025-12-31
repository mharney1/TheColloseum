using System.Collections;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CombatManager : MonoBehaviour
{
	public static CombatManager Instance
	{
		get; private set;
	}

	public void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy( gameObject );
			return;
		}
		Instance = this;
	}
	public IEnumerator ResolveAllPairs()
	{
		foreach (var pair in PairManager.Instance.GetAllPairs())
		{
			Character p1 = pair.Item1;
			Character p2 = pair.Item2;

			Vector3 midpoint = (p1.transform.position + p2.transform.position) * 0.5f;
			Vector3 dir1 = (midpoint - p1.transform.position).normalized;
			Vector3 dir2 = (midpoint - p2.transform.position).normalized;

			float stopDistance = 1f;
			Vector3 p1Target = midpoint - dir1 * stopDistance;
			Vector3 p2Target = midpoint - dir2 * stopDistance;

			p1.movement.MoveTo( p1Target );
			p2.movement.MoveTo( p2Target );
			float timeout = Time.time + 10f;
			yield return new WaitUntil( () => Time.time > timeout ||
				(!p1.movement.IsMoving() && !p2.movement.IsMoving()) );

			CombatOutcome outcome = CombatResolver.Resolve(
				p1.combat.GetChoice(),
				p2.combat.GetChoice(),
				p1.stats.GetExhaustion(),
				p2.stats.GetExhaustion() );

			ApplyOutcome( p1, p2, outcome );

			if (!p1.stats.IsDefeated() && !p2.stats.IsDefeated())
			{
				p1.movement.ReturnToAnchor();
				p2.movement.ReturnToAnchor();
			}
			else
			{
				if (p1.stats.IsDefeated())
				{
					yield return StartCoroutine( PairManager.Instance.RemovePair( p1.identity.pair ) );
					p1.movement.ClearAnchor();
				}
				else
				{
					yield return StartCoroutine( PairManager.Instance.RemovePair( p2.identity.pair ) );
					p2.movement.ClearAnchor();
				}
			}
			timeout = Time.time + 10f;
			yield return new WaitUntil( () => Time.time > timeout ||
				(!p1.movement.IsMoving() && !p2.movement.IsMoving()) );
		}
	}
	private void ApplyOutcome(Character p1, Character p2, CombatOutcome outcome)
	{
		p1.stats.ModifyStats( Mathf.RoundToInt( outcome.healthDeltaP1 ), outcome.exhaustionDeltaP1, outcome.dizzyP1);
		p2.stats.ModifyStats( Mathf.RoundToInt( outcome.healthDeltaP2 ), outcome.exhaustionDeltaP2, outcome.dizzyP2);	
	}
}
