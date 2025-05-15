using System;
using Mirror;

public static class NetworkConnectionToClientExtension
{
	public static DewPlayer GetPlayer(this NetworkConnectionToClient conn)
	{
		if (!NetworkServer.active)
		{
			throw new Exception("Invalid operation");
		}
		foreach (DewPlayer player in DewPlayer.humanPlayers)
		{
			if (conn == player.connectionToClient)
			{
				return player;
			}
		}
		return null;
	}

	public static Hero GetHero(this NetworkConnectionToClient conn)
	{
		if (!NetworkServer.active)
		{
			throw new Exception("Invalid operation");
		}
		foreach (DewPlayer player in DewPlayer.humanPlayers)
		{
			if (conn == player.connectionToClient)
			{
				return player.hero;
			}
		}
		return null;
	}
}
