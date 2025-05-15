using System;
using Steamworks;

public class SteamTicketForWebApi : IDisposable
{
	public string ticket;

	public HAuthTicket handle;

	public void Dispose()
	{
		if (handle.m_HAuthTicket != 0)
		{
			SteamUser.CancelAuthTicket(handle);
			handle.m_HAuthTicket = 0u;
		}
	}
}
