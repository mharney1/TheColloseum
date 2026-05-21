using System;
using Unity.Netcode;

public enum SlotStatus
{
	Open,
	Human,
	AI
}
public enum Team
{
	None,
	TeamA,
	TeamB
}
[System.Serializable]
public struct SlotData : INetworkSerializable, IEquatable<SlotData>
{
	public Team team;
	public SlotStatus status;
	public ulong clientId;
	public bool isReady;
	public bool isHost;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref team);
		serializer.SerializeValue(ref status);
		serializer.SerializeValue(ref clientId);
		serializer.SerializeValue(ref isReady);
		serializer.SerializeValue(ref isHost);
	}
	public bool Equals(SlotData other)
	{
		return team == other.team &&
			   status == other.status &&
			   clientId == other.clientId &&
			   isReady == other.isReady &&
			   isHost == other.isHost;
	}
	public override bool Equals(object obj)
	{
		return obj is SlotData other && Equals(other);
	}
	public override int GetHashCode()
	{
		return HashCode.Combine(team, status, clientId, isReady, isHost);
	}
}
