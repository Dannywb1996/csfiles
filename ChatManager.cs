using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class ChatManager : NetworkedManagerBase<ChatManager>
{
	public struct Message
	{
		public MessageType type;

		public string content;

		public string[] args;

		public string itemType;

		public int itemLevel;

		public Cost itemPrice;

		public string itemCustomData;
	}

	public enum MessageType : byte
	{
		Raw,
		Chat,
		WorldEvent,
		Notice,
		UnlockedAchievement
	}

	public const int ChatMessage_MaxLength = 100;

	public const int ChatMessage_RateLimitCount = 8;

	public const float ChatMessage_RateLimitTimeframe = 12f;

	public const float ChatMessage_RateLimitLockTime = 8f;

	public const float Emote_CooldownTime = 1.4f;

	public SafeAction<Message> ClientEvent_OnMessageReceived;

	public SafeAction<DewPlayer, string> ClientEvent_OnEmoteReceived;

	private List<uint> _mutedPlayers = new List<uint>();

	private Dictionary<int, float> _rates;

	private Dictionary<int, float> _lockChatTimes;

	private Dictionary<int, float> _lastEmoteTimes;

	private List<int> _keysBuffer;

	public const string PlayerNameColorHex = "#70d4ff";

	public const string ChatContentColorHex = "#e4edf0";

	public override void OnStartServer()
	{
		base.OnStartServer();
		_rates = new Dictionary<int, float>();
		_lockChatTimes = new Dictionary<int, float>();
		_lastEmoteTimes = new Dictionary<int, float>();
		_keysBuffer = new List<int>();
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!base.isServer)
		{
			return;
		}
		float delta = dt * 8f / 12f;
		_keysBuffer.Clear();
		_rates.Keys.CopyTo(_keysBuffer);
		foreach (int id in _keysBuffer)
		{
			float newValue = Mathf.MoveTowards(_rates[id], 0f, delta);
			if (newValue < 0.0001f)
			{
				_rates.Remove(id);
			}
			else
			{
				_rates[id] = newValue;
			}
		}
	}

	[Server]
	public bool IsChatLocked(NetworkConnectionToClient connection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Boolean ChatManager::IsChatLocked(Mirror.NetworkConnectionToClient)' called when server was not active");
			return default(bool);
		}
		if (_lockChatTimes.TryGetValue(connection.connectionId, out var time) && Time.time - time < 8f)
		{
			return true;
		}
		return false;
	}

	[Server]
	public bool IncrementRateAndCheck(NetworkConnectionToClient connection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Boolean ChatManager::IncrementRateAndCheck(Mirror.NetworkConnectionToClient)' called when server was not active");
			return default(bool);
		}
		if (NetworkServer.dontListen || NetworkServer.connections.Count <= 1)
		{
			return true;
		}
		if (IsChatLocked(connection))
		{
			return false;
		}
		if (_rates.TryGetValue(connection.connectionId, out var oldValue))
		{
			_rates[connection.connectionId] = oldValue + 1f;
		}
		else
		{
			_rates[connection.connectionId] = 1f;
		}
		if (_rates[connection.connectionId] > 8f)
		{
			_lockChatTimes[connection.connectionId] = Time.time;
			return false;
		}
		return true;
	}

	[Command(requiresAuthority = false)]
	public void CmdSendEmote(string emoteName, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(emoteName);
		SendCommandInternal("System.Void ChatManager::CmdSendEmote(System.String,Mirror.NetworkConnectionToClient)", -948322391, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	[Command(requiresAuthority = false)]
	public void CmdSendChatMessage(string content, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(content);
		SendCommandInternal("System.Void ChatManager::CmdSendChatMessage(System.String,Mirror.NetworkConnectionToClient)", 973844096, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	[Command(requiresAuthority = false)]
	public void CmdSendAchievementMessage(string achKey, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(achKey);
		SendCommandInternal("System.Void ChatManager::CmdSendAchievementMessage(System.String,Mirror.NetworkConnectionToClient)", 1471701451, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	public void SendChatLockedNotice(NetworkConnectionToClient target)
	{
		SendChatMessage(target, new Message
		{
			type = MessageType.Notice,
			content = "Chat_Notice_WaitBeforeChat"
		});
	}

	public bool IsPlayerMuted(DewPlayer player)
	{
		return _mutedPlayers.Contains(player.netId);
	}

	public bool IsPlayerMuted(uint playerNetId)
	{
		return _mutedPlayers.Contains(playerNetId);
	}

	[ClientRpc]
	public void BroadcastChatMessage(Message message)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_ChatManager_002FMessage(writer, message);
		SendRPCInternal("System.Void ChatManager::BroadcastChatMessage(ChatManager/Message)", 262622021, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	public void RpcShowEmote(string emoteName, DewPlayer sender)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(emoteName);
		writer.WriteNetworkBehaviour(sender);
		SendRPCInternal("System.Void ChatManager::RpcShowEmote(System.String,DewPlayer)", 783443695, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public void ShowMessageLocally(Message message)
	{
		try
		{
			ClientEvent_OnMessageReceived?.Invoke(message);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
	}

	[TargetRpc]
	public void SendChatMessage(NetworkConnection target, Message message)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_ChatManager_002FMessage(writer, message);
		SendTargetRPCInternal(target, "System.Void ChatManager::SendChatMessage(Mirror.NetworkConnection,ChatManager/Message)", -901792903, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	private static string SanitizeUserInput(string message)
	{
		if (message.Length > 100)
		{
			message = message.Substring(0, 100);
		}
		message = CaseInsensitiveReplace(message, "<noparse>", "");
		message = CaseInsensitiveReplace(message, "</noparse>", "");
		return message;
	}

	private static string CaseInsensitiveReplace(string message, string target, string replacement)
	{
		message = Regex.Replace(message, Regex.Escape(target), replacement.Replace("$", "$$"), RegexOptions.IgnoreCase);
		return message;
	}

	public void MutePlayer(DewPlayer player)
	{
		if (!_mutedPlayers.Contains(player.netId))
		{
			_mutedPlayers.Add(player.netId);
			ClientEvent_OnMessageReceived?.Invoke(new Message
			{
				type = MessageType.Notice,
				content = "Chat_Notice_PlayerMuted",
				args = new string[1] { player.playerName }
			});
		}
	}

	public void UnmutePlayer(DewPlayer player)
	{
		if (_mutedPlayers.Remove(player.netId))
		{
			ClientEvent_OnMessageReceived?.Invoke(new Message
			{
				type = MessageType.Notice,
				content = "Chat_Notice_PlayerUnmuted",
				args = new string[1] { player.playerName }
			});
		}
	}

	public static string GetDescribedPlayerName(DewPlayer player)
	{
		if (player.hero == null)
		{
			return player.playerName;
		}
		return player.playerName + " (" + DewLocalization.GetUIValue(player.hero.GetType().Name + "_Name") + ")";
	}

	public static string GetColoredDescribedPlayerName(DewPlayer player)
	{
		if (player.hero == null)
		{
			return player.playerName;
		}
		return "<color=#70d4ff>" + player.playerName + " (" + DewLocalization.GetUIValue(player.hero.GetType().Name + "_Name") + ")</color>";
	}

	public static string GetFormattedChatContent(string playerName, string content)
	{
		return "<color=#70d4ff>" + playerName + ":</color> <color=#e4edf0>" + content + "</color>";
	}

	public static string GetColoredSkillName(string typeName, int level)
	{
		string skillLevelTemplate = DewLocalization.GetSkillLevelTemplate(level);
		string sName = DewLocalization.GetSkillName(DewLocalization.GetSkillKey(typeName), 0);
		string itemName = string.Format(skillLevelTemplate, sName);
		string itemRarityColor = Dew.GetRarityColorHex(DewResources.GetByShortTypeName<SkillTrigger>(typeName).rarity);
		return "<color=" + itemRarityColor + ">" + itemName + "</color>";
	}

	public static string GetColoredGemName(string typeName, int quality)
	{
		Gem.QualityType qType = Gem.GetQualityType(quality);
		string gName = DewLocalization.GetGemName(DewLocalization.GetGemKey(typeName));
		string itemName = string.Format(DewLocalization.GetUIValue("InGame_Tooltip_GemName_" + qType), $"{gName} {quality}%");
		string itemRarityColor = Dew.GetRarityColorHex(DewResources.GetByShortTypeName<Gem>(typeName).rarity);
		return "<color=" + itemRarityColor + ">" + itemName + "</color>";
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdSendEmote__String__NetworkConnectionToClient(string emoteName, NetworkConnectionToClient sender)
	{
		DewPlayer p = sender.GetPlayer();
		if (p == null)
		{
			return;
		}
		float oldValue;
		if (!IncrementRateAndCheck(sender))
		{
			SendChatLockedNotice(sender);
		}
		else if (!_lastEmoteTimes.TryGetValue(sender.connectionId, out oldValue) || !(Time.unscaledTime - oldValue < 1.4f))
		{
			_lastEmoteTimes[sender.connectionId] = Time.unscaledTime;
			if (!(DewResources.GetByName<Emote>(emoteName) == null) && p.IsAllowedToUseItem(emoteName))
			{
				RpcShowEmote(emoteName, p);
			}
		}
	}

	protected static void InvokeUserCode_CmdSendEmote__String__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendEmote called on client.");
		}
		else
		{
			((ChatManager)obj).UserCode_CmdSendEmote__String__NetworkConnectionToClient(reader.ReadString(), senderConnection);
		}
	}

	protected void UserCode_CmdSendChatMessage__String__NetworkConnectionToClient(string content, NetworkConnectionToClient sender)
	{
		if (content == null)
		{
			return;
		}
		content = content.Trim();
		if (content.Length > 0)
		{
			if (!IncrementRateAndCheck(sender))
			{
				SendChatLockedNotice(sender);
				return;
			}
			Message msg = new Message
			{
				type = MessageType.Chat,
				content = "<noparse>" + SanitizeUserInput(content) + "</noparse>",
				args = new string[1] { sender.GetPlayer().netId.ToString() }
			};
			BroadcastChatMessage(msg);
		}
	}

	protected static void InvokeUserCode_CmdSendChatMessage__String__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendChatMessage called on client.");
		}
		else
		{
			((ChatManager)obj).UserCode_CmdSendChatMessage__String__NetworkConnectionToClient(reader.ReadString(), senderConnection);
		}
	}

	protected void UserCode_CmdSendAchievementMessage__String__NetworkConnectionToClient(string achKey, NetworkConnectionToClient sender)
	{
		if (!IncrementRateAndCheck(sender))
		{
			SendChatLockedNotice(sender);
		}
		else if (Dew.achievementsByName.ContainsKey(achKey))
		{
			Message msg = new Message
			{
				type = MessageType.UnlockedAchievement,
				args = new string[3]
				{
					sender.GetPlayer().playerName,
					achKey,
					sender.GetPlayer().netId.ToString()
				}
			};
			BroadcastChatMessage(msg);
		}
	}

	protected static void InvokeUserCode_CmdSendAchievementMessage__String__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendAchievementMessage called on client.");
		}
		else
		{
			((ChatManager)obj).UserCode_CmdSendAchievementMessage__String__NetworkConnectionToClient(reader.ReadString(), senderConnection);
		}
	}

	protected void UserCode_BroadcastChatMessage__Message(Message message)
	{
		if ((message.type != MessageType.Chat || message.args.Length != 1 || !uint.TryParse(message.args[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var netId1) || !IsPlayerMuted(netId1)) && (message.type != MessageType.UnlockedAchievement || message.args.Length != 3 || !uint.TryParse(message.args[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var netId2) || !IsPlayerMuted(netId2)))
		{
			ShowMessageLocally(message);
		}
	}

	protected static void InvokeUserCode_BroadcastChatMessage__Message(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC BroadcastChatMessage called on server.");
		}
		else
		{
			((ChatManager)obj).UserCode_BroadcastChatMessage__Message(GeneratedNetworkCode._Read_ChatManager_002FMessage(reader));
		}
	}

	protected void UserCode_RpcShowEmote__String__DewPlayer(string emoteName, DewPlayer sender)
	{
		if (IsPlayerMuted(sender.netId) || !sender.IsAllowedToUseItem(emoteName))
		{
			return;
		}
		try
		{
			ClientEvent_OnEmoteReceived?.Invoke(sender, emoteName);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
	}

	protected static void InvokeUserCode_RpcShowEmote__String__DewPlayer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowEmote called on server.");
		}
		else
		{
			((ChatManager)obj).UserCode_RpcShowEmote__String__DewPlayer(reader.ReadString(), reader.ReadNetworkBehaviour<DewPlayer>());
		}
	}

	protected void UserCode_SendChatMessage__NetworkConnection__Message(NetworkConnection target, Message message)
	{
		try
		{
			ClientEvent_OnMessageReceived?.Invoke(message);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
	}

	protected static void InvokeUserCode_SendChatMessage__NetworkConnection__Message(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC SendChatMessage called on server.");
		}
		else
		{
			((ChatManager)obj).UserCode_SendChatMessage__NetworkConnection__Message(NetworkClient.connection, GeneratedNetworkCode._Read_ChatManager_002FMessage(reader));
		}
	}

	static ChatManager()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(ChatManager), "System.Void ChatManager::CmdSendEmote(System.String,Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdSendEmote__String__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterCommand(typeof(ChatManager), "System.Void ChatManager::CmdSendChatMessage(System.String,Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdSendChatMessage__String__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterCommand(typeof(ChatManager), "System.Void ChatManager::CmdSendAchievementMessage(System.String,Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdSendAchievementMessage__String__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterRpc(typeof(ChatManager), "System.Void ChatManager::BroadcastChatMessage(ChatManager/Message)", InvokeUserCode_BroadcastChatMessage__Message);
		RemoteProcedureCalls.RegisterRpc(typeof(ChatManager), "System.Void ChatManager::RpcShowEmote(System.String,DewPlayer)", InvokeUserCode_RpcShowEmote__String__DewPlayer);
		RemoteProcedureCalls.RegisterRpc(typeof(ChatManager), "System.Void ChatManager::SendChatMessage(Mirror.NetworkConnection,ChatManager/Message)", InvokeUserCode_SendChatMessage__NetworkConnection__Message);
	}
}
