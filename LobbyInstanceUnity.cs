using Unity.Services.Lobbies.Models;

public class LobbyInstanceUnity : LobbyInstance
{
	public Lobby internalInstance;

	public override string id => internalInstance.Id;

	public override string name => internalInstance.Name;

	public override string difficulty => internalInstance.GetAttrOrDefault("difficulty");

	public override bool hasGameStarted => internalInstance.GetAttrBool("hasGameStarted");

	public override int currentPlayers => internalInstance.MaxPlayers - internalInstance.AvailableSlots;

	public override int maxPlayers => internalInstance.MaxPlayers;

	public override string version => internalInstance.GetAttrOrDefault("version");

	public override string hostAddress => internalInstance.GetAttrOrDefault("hostAddress");

	public override bool isInviteOnly => internalInstance.IsPrivate;

	public override string shortCode => internalInstance.LobbyCode;

	public override LobbyConnectionQuality connectionQuality => LobbyConnectionQuality.Unknown;
}
