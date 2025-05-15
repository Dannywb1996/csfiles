public abstract class LobbyInstance
{
	public bool isLobbyLeader;

	public abstract string id { get; }

	public abstract string name { get; }

	public abstract string difficulty { get; }

	public abstract bool hasGameStarted { get; }

	public abstract int currentPlayers { get; }

	public abstract int maxPlayers { get; }

	public abstract string version { get; }

	public abstract string hostAddress { get; }

	public abstract bool isInviteOnly { get; }

	public abstract string shortCode { get; }

	public abstract LobbyConnectionQuality connectionQuality { get; }

	public override string ToString()
	{
		return GetType().Name + " - " + id;
	}
}
