using UnityEngine;

public class ActionState : ICombatState
{
	private readonly CombatStateMachine _stateMachine;
	public CombatState State => _state;
	private CombatState _state = CombatState.Action;

	public ActionState(CombatStateMachine sm)
	{
		_stateMachine = sm;
	}

	public void Enter()
	{
		Debug.Log("[ACTION] Enter");
		CombatManager.Instance.BeginResolvingPairs();
	}

	public void Tick()
	{
		if (!CombatManager.Instance.Resolved)
			return;

		_stateMachine.SetState( _stateMachine.ResolveState );
	}

	public void Exit()
	{
		Debug.Log("[ACTION] Exit");
	}
}
