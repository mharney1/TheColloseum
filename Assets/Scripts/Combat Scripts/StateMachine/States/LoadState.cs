using UnityEngine;

public class LoadState : ICombatState
{
	private readonly CombatStateMachine _stateMachine;
	public CombatState State => _state;
	private CombatState _state = CombatState.Load;

	private bool _pairsStarted;
	private bool _playersRegistered = false;
	private bool _playerManagerInitialized = false;

	public LoadState(CombatStateMachine stateMachine)
	{
		_stateMachine = stateMachine;
	}

	public void Enter()
	{
		Debug.Log("[LOAD] Enter");

		CombatResolver.Initialize();

		_pairsStarted = false;
	}

	public void Tick()
	{
		if (_stateMachine.CombatDirector.PlayerManager == null)
			return;

		if (!_playerManagerInitialized)
		{
			_stateMachine.CombatDirector.PlayerManager.Initialize();
			_playerManagerInitialized = true;
		}

		if (!_playersRegistered)
			if (_stateMachine.CombatDirector.PlayerManager.AllPlayersRegistered())
				_playersRegistered = true;
			else
				return;

		if (!IsReady())
			return;

		if (!_pairsStarted)
		{
			PairManager.Instance.BeginCreatingPairs();
			_pairsStarted= true;
		}
		if (!IsReady2())
			return;

		_stateMachine.SetState( _stateMachine.PrepareState );
	}

	public void Exit()
	{
		Debug.Log("[LOAD] Exit");
	}
	private bool IsReady()
	{
		return (PairManager.Instance != null && _stateMachine.CombatDirector.PlayerManager != null);
	}
	private bool IsReady2()
	{
		return (CombatManager.Instance != null && PairManager.Instance.Paired &&
			PairManager.Instance.AreCharactersReady());
	}
}
