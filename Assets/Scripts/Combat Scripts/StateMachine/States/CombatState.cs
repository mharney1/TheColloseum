// CombatState.cs
public enum CombatState
{
	Load,
	Prepare,
	Action,
	Resolve,
	End
}
// ICombatPhaseState.cs
public interface ICombatState
{
	CombatState State{ get; }
	void Enter();
	void Tick();
	void Exit();
}
