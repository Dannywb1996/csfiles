using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class GameSettingsManager : NetworkedManagerBase<GameSettingsManager>
{
	[CompilerGenerated]
	[SyncVar(hook = "OnDifficultyChanged")]
	private string _003Cdifficulty_003Ek__BackingField;

	public Action<string, string> onDifficultyChanged;

	public readonly SyncList<string> lucidDreams = new SyncList<string>();

	public Action onLucidDreamsChanged;

	public int localPlayerDejavuCost;

	public string difficulty
	{
		[CompilerGenerated]
		get
		{
			return _003Cdifficulty_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cdifficulty_003Ek__BackingField = value;
		}
	} = "diffNormal";

	public string Network_003Cdifficulty_003Ek__BackingField
	{
		get
		{
			return difficulty;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref difficulty, 1uL, OnDifficultyChanged);
		}
	}

	private void OnDifficultyChanged(string oldName, string newName)
	{
		onDifficultyChanged?.Invoke(oldName, newName);
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		lucidDreams.Callback += delegate
		{
			onLucidDreamsChanged?.Invoke();
		};
		OnDifficultyChanged(null, difficulty);
	}

	[Server]
	public void SetDifficulty(string newDiff)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameSettingsManager::SetDifficulty(System.String)' called when server was not active");
			return;
		}
		Network_003Cdifficulty_003Ek__BackingField = newDiff;
		if (ManagerBase<LobbyManager>.instance.isLobbyLeader)
		{
			ManagerBase<LobbyManager>.instance.service.SetDifficulty(newDiff);
		}
	}

	[Server]
	public void AddLucidDream(string type)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameSettingsManager::AddLucidDream(System.String)' called when server was not active");
		}
		else if (!(DewResources.GetByShortTypeName<LucidDream>(type) == null) && !lucidDreams.Contains(type))
		{
			lucidDreams.Add(type);
		}
	}

	[Server]
	public void RemoveLucidDream(string type)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameSettingsManager::RemoveLucidDream(System.String)' called when server was not active");
		}
		else
		{
			lucidDreams.Remove(type);
		}
	}

	[Server]
	public void ClearLucidDreams()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameSettingsManager::ClearLucidDreams()' called when server was not active");
		}
		else
		{
			lucidDreams.Clear();
		}
	}

	public GameSettingsManager()
	{
		InitSyncObject(lucidDreams);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteString(difficulty);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteString(difficulty);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref difficulty, OnDifficultyChanged, reader.ReadString());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref difficulty, OnDifficultyChanged, reader.ReadString());
		}
	}
}
