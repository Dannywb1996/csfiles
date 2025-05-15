using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class InGameConstellationManager : NetworkedManagerBase<InGameConstellationManager>
{
	private Dictionary<DewPlayer, List<DewStarItemOld>> _activeStars = new Dictionary<DewPlayer, List<DewStarItemOld>>();

	private readonly List<DewPlayer> _didStars = new List<DewPlayer>();

	public override void OnStartClient()
	{
		base.OnStartClient();
		GameManager.CallOnReady(delegate
		{
			DewNetworkManager dewNetworkManager = DewNetworkManager.instance;
			dewNetworkManager.onHumanPlayerAdd = (Action<DewPlayer>)Delegate.Combine(dewNetworkManager.onHumanPlayerAdd, new Action<DewPlayer>(OnHumanPlayerAdd));
			DewNetworkManager dewNetworkManager2 = DewNetworkManager.instance;
			dewNetworkManager2.onHumanPlayerRemove = (Action<DewPlayer>)Delegate.Combine(dewNetworkManager2.onHumanPlayerRemove, new Action<DewPlayer>(OnHumanPlayerRemove));
			foreach (DewPlayer current in DewPlayer.humanPlayers)
			{
				OnHumanPlayerAdd(current);
			}
			SendStarsToServer();
		});
	}

	private void OnHumanPlayerAdd(DewPlayer obj)
	{
		obj.ClientEvent_OnHeroChanged += new Action<Hero, Hero>(ClientEventOnHeroChanged);
	}

	private void ClientEventOnHeroChanged(Hero arg1, Hero arg2)
	{
		if (base.isServer)
		{
			_didStars.Remove(arg2.owner);
			RpcRefreshStarsOfPlayer(arg2.owner);
		}
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		if (DewNetworkManager.instance != null)
		{
			DewNetworkManager dewNetworkManager = DewNetworkManager.instance;
			dewNetworkManager.onHumanPlayerAdd = (Action<DewPlayer>)Delegate.Remove(dewNetworkManager.onHumanPlayerAdd, new Action<DewPlayer>(OnHumanPlayerAdd));
			DewNetworkManager dewNetworkManager2 = DewNetworkManager.instance;
			dewNetworkManager2.onHumanPlayerRemove = (Action<DewPlayer>)Delegate.Remove(dewNetworkManager2.onHumanPlayerRemove, new Action<DewPlayer>(OnHumanPlayerRemove));
		}
		DewPlayer[] array = _activeStars.Keys.ToArray();
		foreach (DewPlayer p in array)
		{
			if (_activeStars.TryGetValue(p, out var list))
			{
				for (int i2 = list.Count - 1; i2 >= 0; i2--)
				{
					RemoveStar(p, i2);
				}
				list.Clear();
			}
		}
		_activeStars.Clear();
	}

	private void OnHumanPlayerRemove(DewPlayer obj)
	{
		obj.ClientEvent_OnHeroChanged -= new Action<Hero, Hero>(ClientEventOnHeroChanged);
		RemoveAllStarsOfPlayer(obj);
	}

	private void RemoveAllStarsOfPlayer(DewPlayer obj)
	{
		int removed = 0;
		if (_activeStars.TryGetValue(obj, out var list))
		{
			removed = list.Count;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				RemoveStar(obj, i);
			}
			list.Clear();
			_activeStars.Remove(obj);
		}
		Debug.Log($"Removed {removed} stars of {ChatManager.GetDescribedPlayerName(obj)}");
	}

	[ClientRpc]
	private void RpcRefreshStarsOfPlayer(DewPlayer player)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(player);
		SendRPCInternal("System.Void InGameConstellationManager::RpcRefreshStarsOfPlayer(DewPlayer)", -1952388308, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void SendStarsToServer()
	{
		List<string> list = new List<string>();
		List<int> levels = new List<int>();
		foreach (KeyValuePair<string, DewProfile.StarData> p in DewSave.profile.stars)
		{
			if (p.Value.level > 0)
			{
				list.Add(p.Key);
				levels.Add(p.Value.level);
			}
		}
		CmdAddStars(list.ToArray(), levels.ToArray());
	}

	[Command(requiresAuthority = false)]
	private void CmdAddStars(string[] stars, int[] levels, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_System_002EString_005B_005D(writer, stars);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, levels);
		SendCommandInternal("System.Void InGameConstellationManager::CmdAddStars(System.String[],System.Int32[],Mirror.NetworkConnectionToClient)", -944410488, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcAddStarsToEveryone(string[] stars, int[] levels, DewPlayer player)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_System_002EString_005B_005D(writer, stars);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, levels);
		writer.WriteNetworkBehaviour(player);
		SendRPCInternal("System.Void InGameConstellationManager::RpcAddStarsToEveryone(System.String[],System.Int32[],DewPlayer)", 1881006049, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private bool AddStar(DewPlayer player, DewStarItemOld star, int level)
	{
		if (player == null || star == null)
		{
			return false;
		}
		star.player = player;
		star.level = level;
		try
		{
			if (!star.ShouldInitInGame())
			{
				return false;
			}
			star.isActive = true;
			star.OnStartInGame();
		}
		catch (Exception exception)
		{
			Debug.LogWarning($"Star {star.GetType()} excluded from game because of following exception:");
			Debug.LogException(exception);
			if (star.isActive)
			{
				try
				{
					star.OnStopInGame();
				}
				catch (Exception exception2)
				{
					Debug.LogWarning($"Additional exception occured while stopping {star.GetType()}:");
					Debug.LogException(exception2);
				}
				star.isActive = false;
			}
			return false;
		}
		if (!_activeStars.TryGetValue(player, out var list))
		{
			list = new List<DewStarItemOld>();
			_activeStars.Add(player, list);
		}
		list.Add(star);
		return true;
	}

	private void RemoveStar(DewPlayer player, int index)
	{
		if ((object)player != null && _activeStars.TryGetValue(player, out var list) && index >= 0 && index < list.Count)
		{
			DewStarItemOld star = list[index];
			try
			{
				star.OnStopInGame();
			}
			catch (Exception exception)
			{
				Debug.LogWarning("Exception occured while stopping " + star.GetType().Name + ":");
				Debug.LogException(exception);
			}
			star.isActive = false;
			list.RemoveAt(index);
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcRefreshStarsOfPlayer__DewPlayer(DewPlayer player)
	{
		RemoveAllStarsOfPlayer(player);
		if (player.isLocalPlayer)
		{
			SendStarsToServer();
		}
	}

	protected static void InvokeUserCode_RpcRefreshStarsOfPlayer__DewPlayer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRefreshStarsOfPlayer called on server.");
		}
		else
		{
			((InGameConstellationManager)obj).UserCode_RpcRefreshStarsOfPlayer__DewPlayer(reader.ReadNetworkBehaviour<DewPlayer>());
		}
	}

	protected void UserCode_CmdAddStars__String_005B_005D__Int32_005B_005D__NetworkConnectionToClient(string[] stars, int[] levels, NetworkConnectionToClient sender)
	{
		DewPlayer player = sender.GetPlayer();
		if (player == null || _didStars.Contains(player) || stars.Length > Dew.allOldStars.Count + 10)
		{
			return;
		}
		_didStars.Add(player);
		for (int i = 0; i < stars.Length; i++)
		{
			string s = stars[i];
			if (Dew.oldStarsByName.TryGetValue(s, out var type))
			{
				DewStarItemOld obj = (DewStarItemOld)Activator.CreateInstance(type.GetType());
				obj.player = player;
				obj.level = levels[i];
				if (obj.ShouldInitInGame())
				{
					player.stars.Add(new PlayerStarItem
					{
						type = stars[i],
						level = levels[i]
					});
				}
			}
		}
		RpcAddStarsToEveryone(stars, levels, player);
	}

	protected static void InvokeUserCode_CmdAddStars__String_005B_005D__Int32_005B_005D__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAddStars called on client.");
		}
		else
		{
			((InGameConstellationManager)obj).UserCode_CmdAddStars__String_005B_005D__Int32_005B_005D__NetworkConnectionToClient(GeneratedNetworkCode._Read_System_002EString_005B_005D(reader), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader), senderConnection);
		}
	}

	protected void UserCode_RpcAddStarsToEveryone__String_005B_005D__Int32_005B_005D__DewPlayer(string[] stars, int[] levels, DewPlayer player)
	{
		if (player == null)
		{
			return;
		}
		int added = 0;
		for (int i = 0; i < stars.Length; i++)
		{
			string s = stars[i];
			if (Dew.oldStarsByName.TryGetValue(s, out var type))
			{
				int l = Mathf.Clamp(levels[i], 1, type.maxLevel);
				DewStarItemOld newStar = (DewStarItemOld)Activator.CreateInstance(type.GetType());
				if (AddStar(player, newStar, l))
				{
					added++;
				}
			}
		}
		Debug.Log($"Added {added} stars of {ChatManager.GetDescribedPlayerName(player)}");
	}

	protected static void InvokeUserCode_RpcAddStarsToEveryone__String_005B_005D__Int32_005B_005D__DewPlayer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAddStarsToEveryone called on server.");
		}
		else
		{
			((InGameConstellationManager)obj).UserCode_RpcAddStarsToEveryone__String_005B_005D__Int32_005B_005D__DewPlayer(GeneratedNetworkCode._Read_System_002EString_005B_005D(reader), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader), reader.ReadNetworkBehaviour<DewPlayer>());
		}
	}

	static InGameConstellationManager()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(InGameConstellationManager), "System.Void InGameConstellationManager::CmdAddStars(System.String[],System.Int32[],Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdAddStars__String_005B_005D__Int32_005B_005D__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterRpc(typeof(InGameConstellationManager), "System.Void InGameConstellationManager::RpcRefreshStarsOfPlayer(DewPlayer)", InvokeUserCode_RpcRefreshStarsOfPlayer__DewPlayer);
		RemoteProcedureCalls.RegisterRpc(typeof(InGameConstellationManager), "System.Void InGameConstellationManager::RpcAddStarsToEveryone(System.String[],System.Int32[],DewPlayer)", InvokeUserCode_RpcAddStarsToEveryone__String_005B_005D__Int32_005B_005D__DewPlayer);
	}
}
