using System;
using Unity.Netcode;

public enum Choices : int
{
	None,
	Attack,
	Block,
	Counter,
	Rest
}

public enum Phases
{
	Load,
	Prepare,
	Action,
	Resolve,
	End
}
public enum DmgMultiplier
{
	None,
	Exposed,
	Block,
	Counter
}
public enum CombatUIState
{
	Hidden,
	Choosing,
	Chosen,
	Disabled
}
public struct EndData : INetworkSerializable
{
	public bool Tie;
	public bool TeamBased;
	public int[] WinningTeams;
	public ulong[] WinningPlayerIds;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue( ref Tie );
		serializer.SerializeValue( ref TeamBased);
		serializer.SerializeValue( ref WinningTeams );
		serializer.SerializeValue( ref WinningPlayerIds );
	}
}
public struct CombatOutcome
{
	public float healthDeltaP1;
	public float healthDeltaP2;

	public float exhaustionDeltaP1;
	public float exhaustionDeltaP2;

	public DmgMultiplier multiplierP1;
	public DmgMultiplier multiplierP2;

	public bool dizzyP1;
	public bool dizzyP2;
}
public interface ICharacterDecision
{
	/// Called when a character must decide a choice for the current phase
	void Decide(Character character);
}
