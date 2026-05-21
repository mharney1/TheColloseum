public class Player
{
	public PlayerIdentity Identity { get; set; }
}

public class PlayerIdentity
{
	private string _playerID;
	private ulong _clientID;
	private string _username;
	private string _groupID;
	private bool _isHost;
	private bool _isAI;
	private Team _team;


	public string GetPlayerID() => _playerID;
	public void SetPlayerID(string newID) => _playerID = newID;
	public ulong GetClientID() => _clientID;
	public void SetClientID(ulong newID) => _clientID = newID;
	public string GetUsername() => _username;
	public void SetUsername(string newUsername) => _username = newUsername;
	public string GetGroupID() => _groupID;
	public void SetGroupID(string newGroupID) => _groupID = newGroupID;
	public bool GetHost() => _isHost;
	public void SetHost(bool isHost) => _isHost = isHost;
	public bool GetAI() => _isAI;
	public void SetAI(bool isAI) => _isAI = isAI;
	public Team GetTeam() => _team;
	public void SetTeam(Team team) => _team = team;
}
