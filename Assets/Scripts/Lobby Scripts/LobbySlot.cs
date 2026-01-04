using UnityEngine;

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
public class LobbySlot
{
	public int index;
	public SlotStatus status;

	public Team team;
	public bool isReady;
	public bool isHost = false;

	// Human-only data (safe to be null)
	public string displayName;
	public Sprite profileImage;

	public bool IsOccupied()
	{
		return status != SlotStatus.Open;
	}
}
