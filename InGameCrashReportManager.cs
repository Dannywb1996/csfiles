using System;
using System.Collections;
using System.Globalization;
using Mirror;
using UnityEngine;
using UnityEngine.CrashReportHandler;

public class InGameCrashReportManager : ManagerBase<InGameCrashReportManager>
{
	public enum CustomParamKey
	{
		cheated,
		hero,
		room,
		room_index,
		room_mods,
		current_zone_cleared_nodes,
		cleared_combat_rooms,
		zone,
		zone_index,
		players,
		network,
		elapsed_seconds,
		equipment,
		local_hero_health_ratio
	}

	public override bool shouldRegisterUpdates => false;

	private void Set(CustomParamKey key, string value)
	{
		CrashReportHandler.SetUserMetadata(key.ToString(), value);
	}

	private void Set(CustomParamKey key, float value)
	{
		CrashReportHandler.SetUserMetadata(key.ToString(), value.ToString(CultureInfo.InvariantCulture));
	}

	private void Set(CustomParamKey key, int value)
	{
		CrashReportHandler.SetUserMetadata(key.ToString(), value.ToString(CultureInfo.InvariantCulture));
	}

	private void Start()
	{
		GameManager.CallOnReady(delegate
		{
			DewNetworkManager dewNetworkManager = DewNetworkManager.instance;
			dewNetworkManager.onHumanPlayerRemove = (Action<DewPlayer>)Delegate.Combine(dewNetworkManager.onHumanPlayerRemove, new Action<DewPlayer>(UpdatePlayerCount));
			NetworkedManagerBase<ConsoleManager>.instance.ClientEvent_OnCheatEnabledChanged += new Action<bool>(UpdateCheated);
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += new Action(UpdateZone);
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(UpdateRoom);
			DewPlayer.local.ClientEvent_OnHeroChanged += new Action<Hero, Hero>(UpdateHero);
			UpdatePlayerCount(null);
			Set(CustomParamKey.cheated, 0);
			UpdateZone();
			UpdateRoom(default(EventInfoLoadRoom));
			UpdateHero(null, null);
			if (NetworkServer.dontListen)
			{
				Set(CustomParamKey.network, "solo");
			}
			else
			{
				Set(CustomParamKey.network, NetworkServer.active ? "host" : "client");
			}
			StartCoroutine(Routine());
		});
		IEnumerator Routine()
		{
			while (true)
			{
				if (NetworkedManagerBase<GameManager>.softInstance != null)
				{
					Set(CustomParamKey.elapsed_seconds, (int)NetworkedManagerBase<GameManager>.softInstance.elapsedGameTime);
				}
				if (DewPlayer.local != null && DewPlayer.local.hero != null)
				{
					Set(CustomParamKey.local_hero_health_ratio, DewPlayer.local.hero.currentHealth / DewPlayer.local.hero.maxHealth);
				}
				yield return new WaitForSeconds(1f);
			}
		}
	}

	private void UpdateHero(Hero _, Hero __)
	{
		try
		{
			Set(CustomParamKey.hero, DewPlayer.local.hero.GetType().Name);
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
	}

	private void UpdateRoom(EventInfoLoadRoom _)
	{
		try
		{
			string mods = "";
			int count = NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers.Count;
			for (int i = 0; i < count; i++)
			{
				ModifierData m = NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers[i];
				mods = ((i != count - 1) ? (mods + m.type.Replace("RoomMod_", "") + ",") : (mods + m.type.Replace("RoomMod_", "")));
			}
			Set(CustomParamKey.room, SingletonDewNetworkBehaviour<Room>.instance.name);
			Set(CustomParamKey.room_index, NetworkedManagerBase<ZoneManager>.instance.currentRoomIndex);
			Set(CustomParamKey.current_zone_cleared_nodes, NetworkedManagerBase<ZoneManager>.instance.currentZoneClearedNodes);
			Set(CustomParamKey.cleared_combat_rooms, NetworkedManagerBase<ZoneManager>.instance.clearedCombatRooms);
			Set(CustomParamKey.room_mods, mods);
			Set(CustomParamKey.equipment, new AnalyticsEquipmentData(DewPlayer.local.hero).ToBase64());
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
	}

	private void UpdateZone()
	{
		try
		{
			Set(CustomParamKey.zone, NetworkedManagerBase<ZoneManager>.instance.currentZone.name);
			Set(CustomParamKey.zone_index, NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
	}

	private void UpdateCheated(bool obj)
	{
		try
		{
			if (obj)
			{
				Set(CustomParamKey.cheated, 1);
			}
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
	}

	private void UpdatePlayerCount(DewPlayer _)
	{
		try
		{
			Set(CustomParamKey.players, DewPlayer.humanPlayers.Count);
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
	}

	private void OnDestroy()
	{
		try
		{
			CustomParamKey[] array = (CustomParamKey[])Enum.GetValues(typeof(CustomParamKey));
			foreach (CustomParamKey k in array)
			{
				Set(k, null);
			}
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
	}
}
