using Unity.Services.Lobbies.Models;

public static class UnityLobbyExtensions
{
	public static string GetAttrOrDefault(this Lobby lobby, string key, string defaultValue = null)
	{
		if (lobby == null)
		{
			return null;
		}
		if (!lobby.Data.TryGetValue(key, out var val))
		{
			return defaultValue;
		}
		if (!string.IsNullOrEmpty(val.Value))
		{
			return val.Value;
		}
		return defaultValue;
	}

	public static int GetAttrInt(this Lobby lobby, string key)
	{
		if (lobby == null)
		{
			return 0;
		}
		if (!lobby.Data.TryGetValue(key, out var val))
		{
			return 0;
		}
		if (int.TryParse(val.Value, out var res))
		{
			return res;
		}
		return 0;
	}

	public static bool GetAttrBool(this Lobby lobby, string key)
	{
		return lobby.GetAttrInt(key) == 1;
	}
}
