using Steamworks;

public class LobbyInstanceSteam : LobbyInstance
{
	internal CSteamID _id;

	internal string _name;

	internal string _difficulty;

	internal bool _hasGameStarted;

	internal int _currentPlayers;

	internal int _maxPlayers;

	internal string _version;

	internal string _hostAddress;

	internal bool _isInviteOnly;

	internal string _shortCode;

	internal LobbyConnectionQuality _connectionQuality;

	public override string id => _id.m_SteamID.ToString();

	public override string name => _name;

	public override string difficulty => _difficulty;

	public override bool hasGameStarted => _hasGameStarted;

	public override int currentPlayers => _currentPlayers;

	public override int maxPlayers => _maxPlayers;

	public override string version => _version;

	public override string hostAddress => _hostAddress;

	public override bool isInviteOnly => _isInviteOnly;

	public override string shortCode => _shortCode;

	public override LobbyConnectionQuality connectionQuality => _connectionQuality;
}
