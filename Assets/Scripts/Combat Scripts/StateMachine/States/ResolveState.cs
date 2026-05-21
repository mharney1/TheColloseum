using UnityEngine;

public class ResolveState : ICombatState
{
	private readonly CombatStateMachine _stateMachine;
	public CombatState State => _state;
	private CombatState _state = CombatState.Resolve;

	public ResolveState(CombatStateMachine stateMachine)
	{
		_stateMachine = stateMachine;
	}

	public void Enter()
	{
		Debug.Log("[RESOLVE] Enter");
		PairManager.Instance.BeginCreatingPairs();
		_stateMachine.CombatDirector.WinService.CacheLastAlive();
	}

	public void Tick()
	{
		if (!PairingComplete())
			return;

		if (_stateMachine.CombatDirector.WinService.CheckWinConditions())
		{
			_stateMachine.SetState( _stateMachine.EndState );
		}
		else
		{
			_stateMachine.SetState( _stateMachine.PrepareState );
		}
	}

	public void Exit()
	{
		Debug.Log("[RESOLVE] Exit");
	}

	private bool PairingComplete()
	{
		return (PairManager.Instance.Paired && PairManager.Instance.AreCharactersReady());
	}
}
