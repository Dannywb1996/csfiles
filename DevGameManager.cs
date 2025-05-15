using System;
using IngameDebugConsole;
using Mirror;
using Mirror.RemoteCalls;
using Steamworks;
using UnityEngine;

public class DevGameManager : GameManager
{
	public Zone startZonePrefab;

	[Command(requiresAuthority = false)]
	public void SpawnHero(Type heroType, int level, HeroLoadoutData loadout, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteType(heroType);
		writer.WriteInt(level);
		GeneratedNetworkCode._Write_HeroLoadoutData(writer, loadout);
		SendCommandInternal("System.Void DevGameManager::SpawnHero(System.Type,System.Int32,HeroLoadoutData,Mirror.NetworkConnectionToClient)", -150208123, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		NetworkedManagerBase<ConsoleManager>.instance.EnableCheats();
		NetworkedManagerBase<ZoneManager>.instance.LoadZone(startZonePrefab);
		Debug.Log($"User Steam ID {SteamUser.GetSteamID()}");
	}

	public override void OnLateStart()
	{
		base.OnLateStart();
		SpawnHero(DewResources.GetByShortTypeName("Hero_Mist").GetType(), 1, new HeroLoadoutData());
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		InGameUIManager.instance.SetState("Playing");
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		InGameUIManager.instance.SetState("Loading");
	}

	public void HostGame()
	{
		DebugLogConsole.ExecuteCommand("host");
	}

	public void StartLocalGame()
	{
		DebugLogConsole.ExecuteCommand("local");
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_SpawnHero__Type__Int32__HeroLoadoutData__NetworkConnectionToClient(Type heroType, int level, HeroLoadoutData loadout, NetworkConnectionToClient sender)
	{
		DewPlayer player = sender.GetPlayer();
		if (player.hero != null)
		{
			Dew.Destroy(player.hero.gameObject);
		}
		Vector3 spawnPos = ((NetworkedManagerBase<ZoneManager>.instance.currentRoom == null) ? Vector3.zero : NetworkedManagerBase<ZoneManager>.instance.currentRoom.GetHeroSpawnPosition());
		Hero newHero = (player.hero = Dew.SpawnHero((Hero)DewResources.GetByType(heroType), spawnPos, Quaternion.identity, player, level, loadout));
		player.controllingEntity = newHero;
	}

	protected static void InvokeUserCode_SpawnHero__Type__Int32__HeroLoadoutData__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command SpawnHero called on client.");
		}
		else
		{
			((DevGameManager)obj).UserCode_SpawnHero__Type__Int32__HeroLoadoutData__NetworkConnectionToClient(reader.ReadType(), reader.ReadInt(), GeneratedNetworkCode._Read_HeroLoadoutData(reader), senderConnection);
		}
	}

	static DevGameManager()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(DevGameManager), "System.Void DevGameManager::SpawnHero(System.Type,System.Int32,HeroLoadoutData,Mirror.NetworkConnectionToClient)", InvokeUserCode_SpawnHero__Type__Int32__HeroLoadoutData__NetworkConnectionToClient, requiresAuthority: false);
	}
}
