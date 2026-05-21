using System.Collections.Generic;
using UnityEngine;

public class PrepareState : ICombatState
{
	private readonly CombatStateMachine _stateMachine;
	public CombatState State => _state;
	private CombatState _state = CombatState.Prepare;

	private const float C_START_TIME = 15f;
	private const float C_SHORT_TIME = 3f;

	private CombatTimer _timer = new( C_START_TIME, C_SHORT_TIME );
	public float TimeRemaining => _timer.TimeRemaining;

	private readonly List<Character> _undecided = new();

	public PrepareState(CombatStateMachine stateMachine)
	{
		_stateMachine = stateMachine;
	}

	public void Enter()
	{
		Debug.Log("[PREPARE] Enter");

		_timer.Reset();

		_undecided.Clear();

		PairManager.Instance.GetCombatants( _undecided );
	}

	public void Tick()
	{
		_timer.Tick();

		if ( _undecided.Count == 0 )
			_timer.Shorten();

		if ( !_timer.IsFinished() )
			return;

		ApplyDefaults();

		_stateMachine.SetState( _stateMachine.ActionState );
	}

	public void Exit()
	{
		Debug.Log("[PREPARE] Exit");
	}

	private void ApplyDefaults()
	{
		foreach (var c in _undecided)
		{
			if (c == null)
				continue;

			c.combat.SetChoiceServerRpc(Choices.Attack);
		}

		_undecided.Clear();
	}

	public void RemoveUndecided(Character player)
	{
		_undecided.Remove(player);
	}
}
