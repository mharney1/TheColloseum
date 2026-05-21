// CombatPhaseStateMachine.cs
using UnityEngine;

public class CombatStateMachine
{
	private readonly CombatDirector _combatDirector;
	public CombatDirector CombatDirector => _combatDirector;
	private ICombatState _current;
	public CombatState CurrentState{ get; private set; }
	public float TimeRemaining => _prepareState.TimeRemaining;

	private readonly LoadState _loadState;
	private readonly PrepareState _prepareState;
	private readonly ActionState _actionState;
	private readonly ResolveState _resolveState;
	private readonly EndState _endState;
	public LoadState LoadState => _loadState;
	public PrepareState PrepareState => _prepareState;
	public ActionState ActionState => _actionState;
	public ResolveState ResolveState => _resolveState;
	public EndState EndState => _endState;


	public CombatStateMachine(CombatDirector manager)
	{
		_combatDirector = manager;

		_loadState = new LoadState(this);
		_prepareState = new PrepareState(this);
		_actionState = new ActionState(this);
		_resolveState = new ResolveState(this);
		_endState = new EndState(this);

		SetState( _loadState );
	}

	public void Tick()
	{
		_current?.Tick();
	}

	public void SetState(ICombatState next)
	{
		_current?.Exit();
		_current = next;
		CurrentState = _current.State;
		_current.Enter();
	}
}
