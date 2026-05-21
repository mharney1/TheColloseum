using UnityEngine;

public class EndState : ICombatState
{
	private readonly CombatStateMachine _stateMachine;
	public CombatState State => _state;
	private CombatState _state = CombatState.End;

	public EndState(CombatStateMachine stateMachine)
	{
		_stateMachine = stateMachine;
	}

	public void Enter()
	{
		Debug.Log("[END] Enter");

		EndData data = _stateMachine.CombatDirector.WinService.BuildEndData();

		_stateMachine.CombatDirector.AnnounceGameEnd(data);
	}

	public void Tick()
	{
	}

	public void Exit()
	{
		Debug.Log("[END] Exit");
	}
}
