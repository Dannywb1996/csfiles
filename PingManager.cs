using System;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class PingManager : NetworkedManagerBase<PingManager>
{
	[Serializable]
	public struct PingPrefabBindings
	{
		public PingInstance world;

		public PingUIInstance ui;
	}

	public struct Ping
	{
		public DewPlayer sender;

		public PingType type;

		public NetworkBehaviour target;

		public Vector3 position;

		public int itemIndex;

		public bool IsValid()
		{
			if (sender == null)
			{
				return false;
			}
			switch (type)
			{
			case PingType.Move:
				return true;
			case PingType.Entity:
				if (target is Entity e)
				{
					return e.isActive;
				}
				return false;
			case PingType.Interactable:
				if (target is IItem item && (item.owner != null || item.handOwner != null))
				{
					return false;
				}
				if (target is Actor a2)
				{
					return a2.isActive;
				}
				if (target != null)
				{
					return target.isActiveAndEnabled;
				}
				return false;
			case PingType.EquippedItem:
				if (target is Actor a1)
				{
					return a1.isActive;
				}
				return false;
			case PingType.ShopItem:
				if (target is PropEnt_Merchant_Jonas s)
				{
					return s.isActive;
				}
				return false;
			case PingType.WorldNode:
				if (itemIndex >= 0 && itemIndex < NetworkedManagerBase<ZoneManager>.instance.nodes.Count)
				{
					return !NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition;
				}
				return false;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public enum PingType : byte
	{
		Move,
		Entity,
		Interactable,
		EquippedItem,
		ShopItem,
		ChoiceItem,
		WorldNode
	}

	public PingPrefabBindings move;

	public PingPrefabBindings enemy;

	public PingPrefabBindings interest;

	public PingPrefabBindings worldNode;

	public Transform pingIndicatorParent;

	public Transform pingIndicatorAlwaysOnTopParent;

	private Dictionary<DewPlayer, GameObject> _previousPings;

	private float _pingStartTime;

	public override void OnStart()
	{
		base.OnStart();
		_previousPings = new Dictionary<DewPlayer, GameObject>();
		_previousPings.Clear();
	}

	[Command(requiresAuthority = false)]
	public void CmdSendPing(Ping ping, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_PingManager_002FPing(writer, ping);
		SendCommandInternal("System.Void PingManager::CmdSendPing(PingManager/Ping,Mirror.NetworkConnectionToClient)", -309149973, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	public void BroadcastPing(Ping ping)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_PingManager_002FPing(writer, ping);
		SendRPCInternal("System.Void PingManager::BroadcastPing(PingManager/Ping)", 1814865367, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void ShowPingChatMessage(Ping ping)
	{
		string itemType = "";
		int itemLevel = 0;
		Cost itemPrice = default(Cost);
		string itemCustomData = null;
		string content;
		switch (ping.type)
		{
		case PingType.Move:
		case PingType.WorldNode:
		{
			string moveTemplate = GetPingUIValue((ping.type == PingType.Move) ? "Chat_Ping_Move" : "Chat_Ping_MoveWorldMap");
			string mapKey = DewInput.GetReadableTextForCurrentMode(DewSave.profile.controls.worldMap);
			ChatManager.Message moveMsg = new ChatManager.Message
			{
				type = ChatManager.MessageType.Raw,
				content = "<color=#e4edf0>" + string.Format(moveTemplate, "<color=#70d4ff>" + ping.sender.playerName + "</color>", mapKey) + "</color>"
			};
			NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(moveMsg);
			return;
		}
		case PingType.Entity:
		{
			Entity ent = (Entity)ping.target;
			string format = GetPingUIValue(accentColor: DewPlayer.local.GetTeamRelation(ent) switch
			{
				TeamRelation.Own => "#52def7", 
				TeamRelation.Neutral => "#faea75", 
				TeamRelation.Enemy => "#fc8888", 
				TeamRelation.Ally => "#52def7", 
				_ => throw new ArgumentOutOfRangeException(), 
			}, key: (ent == ping.sender.hero) ? "Chat_Ping_Entity_Self" : "Chat_Ping_Entity");
			string entName = DewLocalization.GetUIValue(ent.GetType().Name + "_Name");
			if (ent is Hero)
			{
				entName = ent.owner.playerName + " (" + entName + ")";
			}
			content = string.Format(format, entName, Mathf.RoundToInt(ent.normalizedHealth * 100f));
			break;
		}
		case PingType.Interactable:
			if (ping.target is SkillTrigger skill)
			{
				string rarityColor = Dew.GetRarityColorHex(skill.rarity);
				string skillName = DewLocalization.GetSkillName(skill, 0);
				skillName = string.Format(DewLocalization.GetSkillLevelTemplate(skill.level), skillName);
				content = "<color=" + rarityColor + ">" + skillName + "</color>";
				itemType = skill.GetType().Name;
				itemLevel = skill.level;
			}
			else if (ping.target is Gem gem)
			{
				string rarityColor2 = Dew.GetRarityColorHex(gem.rarity);
				Gem.QualityType qType2 = Gem.GetQualityType(gem.quality);
				string gName2 = DewLocalization.GetGemName(gem);
				gName2 = string.Format(DewLocalization.GetUIValue("InGame_Tooltip_GemName_" + qType2), $"{gName2} {gem.quality}%");
				content = "<color=" + rarityColor2 + ">" + gName2 + "</color>";
				itemType = gem.GetType().Name;
				itemLevel = gem.quality;
			}
			else
			{
				IInteractable interactable = (IInteractable)ping.target;
				string iTemplate = GetPingUIValue(interactable.CanInteract(ping.sender.hero) ? "Chat_Ping_Interactable_On" : "Chat_Ping_Interactable_Off", "#ffffff");
				string iName = "???";
				try
				{
					iName = ((!(interactable is IShrineCustomName custom)) ? ((!(interactable is ICustomInteractable customInter)) ? DewLocalization.GetUIValue(interactable.GetType().Name + "_Name") : customInter.nameRawText) : custom.GetRawName());
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				content = string.Format(iTemplate, iName);
			}
			break;
		case PingType.EquippedItem:
		{
			string rarityColor3 = Dew.GetRarityColorHex((ping.target is SkillTrigger s) ? s.rarity : ((Gem)ping.target).rarity);
			if (ping.target is SkillTrigger { owner: var owner } skill2)
			{
				string skillName2 = DewLocalization.GetSkillName(skill2, 0);
				skill2.owner.Skill.TryGetSkillLocation(skill2, out var loc);
				Gem firstGem = skill2.owner.Skill.GetFirstGem(loc);
				if (firstGem != null)
				{
					skillName2 = string.Format(DewLocalization.GetGemTemplate(DewLocalization.GetGemKey(firstGem.GetType())), skillName2);
				}
				skillName2 = string.Format(DewLocalization.GetSkillLevelTemplate(skill2.level), skillName2);
				float cooldownTime = Mathf.Ceil(skill2.currentConfigCooldownTime);
				string msgTemplate = ((owner.owner != ping.sender) ? GetPingUIValue("Chat_Ping_Skill_Others", rarityColor3) : ((skill2.currentConfigCurrentCharge <= 0) ? ((skill2.type != SkillType.Ultimate) ? GetPingUIValue("Chat_Ping_Skill_Cooldown", rarityColor3) : GetPingUIValue("Chat_Ping_Skill_Cooldown_Percentage", rarityColor3)) : ((skill2.currentConfig.maxCharges <= 1) ? GetPingUIValue("Chat_Ping_Skill_Ready", rarityColor3) : GetPingUIValue("Chat_Ping_Skill_ReadyMultiple", rarityColor3))));
				string playerName = ChatManager.GetColoredDescribedPlayerName(owner.owner);
				content = string.Format(msgTemplate, skillName2, cooldownTime, skill2.currentConfigCurrentCharge, playerName, Mathf.RoundToInt(100f - skill2.currentConfigCooldownTime / skill2.currentConfigMaxCooldownTime * 100f));
				itemType = skill2.GetType().Name;
				itemLevel = skill2.level;
			}
			else
			{
				if (!(ping.target is Gem { owner: var owner2 } gem2))
				{
					return;
				}
				Gem.QualityType qType3 = Gem.GetQualityType(gem2.quality);
				string gName3 = DewLocalization.GetGemName(gem2);
				gName3 = string.Format(DewLocalization.GetUIValue("InGame_Tooltip_GemName_" + qType3), $"{gName3} {gem2.quality}%");
				string pingUIValue = GetPingUIValue((owner2.owner != ping.sender) ? "Chat_Ping_Gem_Others" : "Chat_Ping_Gem", rarityColor3);
				string playerName2 = ChatManager.GetColoredDescribedPlayerName(owner2.owner);
				content = string.Format(pingUIValue, gName3, playerName2);
				itemType = gem2.GetType().Name;
				itemLevel = gem2.quality;
			}
			break;
		}
		case PingType.ShopItem:
		{
			MerchandiseData item2 = ((PropEnt_Merchant_Base)ping.target).merchandises[ping.sender.netId][ping.itemIndex];
			itemCustomData = item2.customData;
			string itemName2;
			string itemRarityColor2;
			if (item2.type == MerchandiseType.Skill)
			{
				string skillLevelTemplate2 = DewLocalization.GetSkillLevelTemplate(item2.level);
				string sName2 = DewLocalization.GetSkillName(DewLocalization.GetSkillKey(item2.itemName), 0);
				itemName2 = string.Format(skillLevelTemplate2, sName2);
				itemRarityColor2 = Dew.GetRarityColorHex(DewResources.GetByShortTypeName<SkillTrigger>(item2.itemName).rarity);
			}
			else if (item2.type == MerchandiseType.Gem)
			{
				Gem.QualityType qType4 = Gem.GetQualityType(item2.level);
				string gName4 = DewLocalization.GetGemName(DewLocalization.GetGemKey(item2.itemName));
				itemName2 = string.Format(DewLocalization.GetUIValue("InGame_Tooltip_GemName_" + qType4), $"{gName4} {item2.level}%");
				itemRarityColor2 = Dew.GetRarityColorHex(DewResources.GetByShortTypeName<Gem>(item2.itemName).rarity);
			}
			else
			{
				if (item2.type == MerchandiseType.Souvenir || item2.type != MerchandiseType.Treasure)
				{
					return;
				}
				itemName2 = DewLocalization.GetTreasureName(DewLocalization.GetTreasureKey(item2.itemName));
				itemRarityColor2 = Dew.GetHex(new Color(0.4f, 1f, 0.4f));
			}
			string template2 = GetPingUIValue((item2.count > 0) ? "Chat_Ping_ShopItem_HasStock" : "Chat_Ping_ShopItem_OutOfStock", itemRarityColor2);
			content = "<color=#a7bfc4>" + string.Format(template2, itemName2, item2.count, item2.price) + "</color>";
			itemType = item2.itemName;
			itemLevel = item2.level;
			itemPrice = item2.price;
			break;
		}
		case PingType.ChoiceItem:
		{
			ChoiceShrine shrine = (ChoiceShrine)ping.target;
			ChoiceShrineItem item = shrine.choices[ping.itemIndex];
			string itemName;
			string itemRarityColor;
			if (item.typeName.StartsWith("St_"))
			{
				string skillLevelTemplate = DewLocalization.GetSkillLevelTemplate(item.level);
				string sName = DewLocalization.GetSkillName(DewLocalization.GetSkillKey(item.typeName), 0);
				itemName = string.Format(skillLevelTemplate, sName);
				itemRarityColor = Dew.GetRarityColorHex(DewResources.GetByShortTypeName<SkillTrigger>(item.typeName).rarity);
			}
			else
			{
				if (!item.typeName.StartsWith("Gem_"))
				{
					return;
				}
				Gem.QualityType qType = Gem.GetQualityType(item.level);
				string gName = DewLocalization.GetGemName(DewLocalization.GetGemKey(item.typeName));
				itemName = string.Format(DewLocalization.GetUIValue("InGame_Tooltip_GemName_" + qType), $"{gName} {item.level}%");
				itemRarityColor = Dew.GetRarityColorHex(DewResources.GetByShortTypeName<Gem>(item.typeName).rarity);
			}
			string template = GetPingUIValue("Chat_Ping_ChoiceShrine", itemRarityColor);
			string shrineName = DewLocalization.GetUIValue(shrine.GetType().Name + "_Name");
			content = "<color=#a7bfc4>" + string.Format(template, itemName, shrineName) + "</color>";
			itemType = item.typeName;
			itemLevel = item.level;
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
		ChatManager.Message msg = new ChatManager.Message
		{
			type = ChatManager.MessageType.Chat,
			content = "<color=#bcccd1>" + content + "</color>",
			args = new string[1] { ping.sender.netId.ToString() },
			itemType = itemType,
			itemLevel = itemLevel,
			itemPrice = itemPrice,
			itemCustomData = itemCustomData
		};
		NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(msg);
	}

	public static string GetPingUIValue(string key, string accentColor = "#63e3ff")
	{
		return DewLocalization.GetUIValue(key).Replace("[", "<color=" + accentColor + ">").Replace("]", "</color>");
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdSendPing__Ping__NetworkConnectionToClient(Ping ping, NetworkConnectionToClient sender)
	{
		if (!NetworkedManagerBase<ChatManager>.instance.IncrementRateAndCheck(sender))
		{
			NetworkedManagerBase<ChatManager>.instance.SendChatLockedNotice(sender);
			return;
		}
		ping.sender = sender.GetPlayer();
		switch (ping.type)
		{
		default:
			return;
		case PingType.Entity:
			if (!(ping.target is Entity { isActive: not false }) || ping.target.netId == 0)
			{
				return;
			}
			break;
		case PingType.Interactable:
			if (ping.target == null || !(ping.target is IInteractable) || ping.target.netId == 0)
			{
				return;
			}
			break;
		case PingType.EquippedItem:
			if (ping.target == null || (!(ping.target is SkillTrigger) && !(ping.target is Gem)))
			{
				return;
			}
			break;
		case PingType.ShopItem:
		{
			if (!(ping.target is PropEnt_Merchant_Base shop) || ping.itemIndex < 0 || !shop.merchandises.TryGetValue(ping.sender.netId, out var list) || ping.itemIndex >= list.Length)
			{
				return;
			}
			break;
		}
		case PingType.ChoiceItem:
			if (!(ping.target is ChoiceShrine shrine) || ping.itemIndex < 0 || ping.itemIndex >= shrine.choices.Count)
			{
				return;
			}
			break;
		case PingType.WorldNode:
			if (ping.itemIndex < 0 || ping.itemIndex >= NetworkedManagerBase<ZoneManager>.instance.nodes.Count)
			{
				return;
			}
			break;
		case PingType.Move:
			break;
		}
		if (ping.target is Gem g && g.tempOwner == sender.GetPlayer())
		{
			g.tempOwner = null;
		}
		if (ping.target is SkillTrigger st && st.tempOwner == sender.GetPlayer())
		{
			st.tempOwner = null;
		}
		BroadcastPing(ping);
	}

	protected static void InvokeUserCode_CmdSendPing__Ping__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendPing called on client.");
		}
		else
		{
			((PingManager)obj).UserCode_CmdSendPing__Ping__NetworkConnectionToClient(GeneratedNetworkCode._Read_PingManager_002FPing(reader), senderConnection);
		}
	}

	protected void UserCode_BroadcastPing__Ping(Ping ping)
	{
		if (NetworkedManagerBase<ChatManager>.instance.IsPlayerMuted(ping.sender))
		{
			return;
		}
		if (_previousPings.ContainsKey(ping.sender) && _previousPings[ping.sender] != null)
		{
			UnityEngine.Object.Destroy(_previousPings[ping.sender]);
			_previousPings.Remove(ping.sender);
		}
		ShowPingChatMessage(ping);
		PingPrefabBindings prefabs;
		switch (ping.type)
		{
		default:
			return;
		case PingType.Move:
			prefabs = move;
			break;
		case PingType.Entity:
		{
			Entity ent = (Entity)ping.target;
			prefabs = ((!DewPlayer.local.CheckEnemyOrNeutral(ent)) ? interest : enemy);
			break;
		}
		case PingType.Interactable:
			prefabs = interest;
			break;
		case PingType.WorldNode:
			prefabs = worldNode;
			break;
		case PingType.EquippedItem:
		case PingType.ShopItem:
		case PingType.ChoiceItem:
			return;
		}
		if (prefabs.world != null)
		{
			PingInstance newInstance = UnityEngine.Object.Instantiate(prefabs.world);
			newInstance._ping = ping;
			_previousPings[ping.sender] = newInstance.gameObject;
			if (prefabs.ui != null)
			{
				PingUIInstance newUiInstance = UnityEngine.Object.Instantiate(prefabs.ui, prefabs.ui.useAlwaysOnTopUIParent ? pingIndicatorAlwaysOnTopParent : pingIndicatorParent);
				newUiInstance._ping = ping;
				newInstance._uiInstance = newUiInstance.gameObject;
			}
		}
		else if (prefabs.ui != null)
		{
			PingUIInstance newUiInstance2 = UnityEngine.Object.Instantiate(prefabs.ui, prefabs.ui.useAlwaysOnTopUIParent ? pingIndicatorAlwaysOnTopParent : pingIndicatorParent);
			newUiInstance2._ping = ping;
			_previousPings[ping.sender] = newUiInstance2.gameObject;
			UnityEngine.Object.Destroy(newUiInstance2.gameObject, 8f);
		}
	}

	protected static void InvokeUserCode_BroadcastPing__Ping(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC BroadcastPing called on server.");
		}
		else
		{
			((PingManager)obj).UserCode_BroadcastPing__Ping(GeneratedNetworkCode._Read_PingManager_002FPing(reader));
		}
	}

	static PingManager()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(PingManager), "System.Void PingManager::CmdSendPing(PingManager/Ping,Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdSendPing__Ping__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterRpc(typeof(PingManager), "System.Void PingManager::BroadcastPing(PingManager/Ping)", InvokeUserCode_BroadcastPing__Ping);
	}
}
