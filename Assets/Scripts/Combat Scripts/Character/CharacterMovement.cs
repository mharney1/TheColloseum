using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class CharacterMovement : MonoBehaviour
{
	private Character _character;
	private NavMeshAgent _agent;
	private Transform _newAnchor;
	private Transform _currentAnchor;

	private bool _lockToAnchor;
	private bool _turning;
	private float _finalRotateSpeed = 4f;

	public void Awake()
	{
		if (!NetworkManager.Singleton.IsServer)
		{
			enabled = false;
			return;
		}

		_character = GetComponent<Character>();

		_agent = _character.GetComponent<NavMeshAgent>();
		if (_agent == null)
			_agent = _character.gameObject.AddComponent<NavMeshAgent>();

		_agent.speed = 10f;
		_agent.angularSpeed = 720f;
		_agent.acceleration = 10f;
		_agent.stoppingDistance = 0.02f;
		_agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
	}

	public void MoveTo(Vector3 target)
	{
		if (!_agent.enabled || !_agent.isOnNavMesh)
		{
			Debug.LogWarning($"{name} agent not ready");
			return;
		}

		_agent.Warp( _character.transform.position );
		_turning = true;
		_agent.isStopped = false;
		_agent.updatePosition = true;
		_agent.updateRotation = true;
		_agent.SetDestination( target );
	}

	public void MoveToAnchor(Transform anchor)
	{
		_lockToAnchor = true;
		_newAnchor = anchor;
		MoveTo( anchor.position );
	}

	public void ReturnToAnchor()
	{
		if (_currentAnchor != null)
		{
			_lockToAnchor = true;
			MoveTo( _currentAnchor.position );
		}
	}

	private void Update()
	{
		if (_agent == null)
			return;

		if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance && _turning)
		{
			_agent.isStopped = true;
			_agent.ResetPath();
			_agent.updatePosition = false;
			_agent.updateRotation = false;

			Vector3 toCenter = Vector3.zero - _character.transform.position;
			toCenter.y = 0f;

			if (toCenter.sqrMagnitude > 0.0001f)
			{
				Quaternion targetRot = Quaternion.LookRotation( toCenter.normalized );
				_character.transform.rotation = Quaternion.Slerp(
					_character.transform.rotation,
					targetRot,
					Time.deltaTime * _finalRotateSpeed
				);

				if (Quaternion.Angle( _character.transform.rotation, targetRot ) < 1f)
				{
					_turning = false;
					ApplyAnchor();
				}
			}
			else
			{
				_turning = false;
				ApplyAnchor();
			}
		}
	}

	private void ApplyAnchor()
	{
		if (_lockToAnchor)
		{
			_lockToAnchor = false;
			if (_newAnchor != null)
			{
				_character.transform.SetParent( _newAnchor );
				_currentAnchor = _newAnchor;
				_newAnchor = null;
			}

			_character.transform.localPosition = Vector3.zero;
			_character.transform.localRotation = Quaternion.identity;
		}
	}


	public void ClearAnchor()
	{
		_character.transform.SetParent(null, true);
		_currentAnchor = null;

	}

	public bool IsMoving()
	{
		return _agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance || _turning;
	}
}
