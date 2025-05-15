using System;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;

public static class LobbyDetailAttributesExtension
{
	public static AttributeDataValue ToAttrDataValue(this object o)
	{
		if (o is string s)
		{
			return new AttributeDataValue
			{
				AsUtf8 = s
			};
		}
		if (o is double d)
		{
			return new AttributeDataValue
			{
				AsDouble = d
			};
		}
		if (o is float f)
		{
			return new AttributeDataValue
			{
				AsDouble = f
			};
		}
		if (o is int i)
		{
			return new AttributeDataValue
			{
				AsInt64 = i
			};
		}
		if (o is long l)
		{
			return new AttributeDataValue
			{
				AsInt64 = l
			};
		}
		if (o is bool b)
		{
			return new AttributeDataValue
			{
				AsBool = b
			};
		}
		throw new ArgumentException("o");
	}

	public static string GetAttributeString(this LobbyDetails lobby, string key)
	{
		LobbyDetailsCopyAttributeByKeyOptions opt = new LobbyDetailsCopyAttributeByKeyOptions
		{
			AttrKey = key
		};
		if (lobby.CopyAttributeByKey(ref opt, out var attrName) == Result.Success && attrName.HasValue && attrName.Value.Data.HasValue)
		{
			return attrName.Value.Data.Value.Value.AsUtf8;
		}
		return "";
	}

	public static long GetAttributeLong(this LobbyDetails lobby, string key)
	{
		LobbyDetailsCopyAttributeByKeyOptions opt = new LobbyDetailsCopyAttributeByKeyOptions
		{
			AttrKey = key
		};
		if (lobby.CopyAttributeByKey(ref opt, out var attrName) == Result.Success && attrName.HasValue && attrName.Value.Data.HasValue)
		{
			return attrName.Value.Data.Value.Value.AsInt64.GetValueOrDefault();
		}
		return 0L;
	}

	public static bool GetAttributeBool(this LobbyDetails lobby, string key)
	{
		LobbyDetailsCopyAttributeByKeyOptions opt = new LobbyDetailsCopyAttributeByKeyOptions
		{
			AttrKey = key
		};
		if (lobby.CopyAttributeByKey(ref opt, out var attrName) == Result.Success && attrName.HasValue && attrName.Value.Data.HasValue)
		{
			return attrName.Value.Data.Value.Value.AsBool == true;
		}
		return false;
	}
}
