using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class CurseStatusEffect : StatusEffect
{
	public Action CurseEvent_OnProgressChanged;

	public HatredStrengthType availableStrengths = HatredStrengthType.Mild | HatredStrengthType.Potent | HatredStrengthType.Powerful;

	public float chanceWeight = 1f;

	[CompilerGenerated]
	[SyncVar]
	private HatredStrengthType _003CcurrentStrength_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int _003CrequiredAmount_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int _003CcurrentProgress_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private QuestProgressType _003CprogressType_003Ek__BackingField;

	private KillTracker _tracker;

	[NonSerialized]
	[SyncVar]
	public Quest_KillToLiftCurse quest;

	protected NetworkBehaviourSyncVar ___questNetId;

	public HatredStrengthType currentStrength
	{
		[CompilerGenerated]
		get
		{
			return _003CcurrentStrength_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcurrentStrength_003Ek__BackingField = value;
		}
	} = HatredStrengthType.Potent;

	public int requiredAmount
	{
		[CompilerGenerated]
		get
		{
			return _003CrequiredAmount_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CrequiredAmount_003Ek__BackingField = value;
		}
	}

	public int currentProgress
	{
		[CompilerGenerated]
		get
		{
			return _003CcurrentProgress_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcurrentProgress_003Ek__BackingField = value;
		}
	}

	public QuestProgressType progressType
	{
		[CompilerGenerated]
		get
		{
			return _003CprogressType_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CprogressType_003Ek__BackingField = value;
		}
	}

	public HatredStrengthType Network_003CcurrentStrength_003Ek__BackingField
	{
		get
		{
			return currentStrength;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref currentStrength, 512uL, null);
		}
	}

	public int Network_003CrequiredAmount_003Ek__BackingField
	{
		get
		{
			return requiredAmount;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref requiredAmount, 1024uL, null);
		}
	}

	public int Network_003CcurrentProgress_003Ek__BackingField
	{
		get
		{
			return currentProgress;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref currentProgress, 2048uL, null);
		}
	}

	public QuestProgressType Network_003CprogressType_003Ek__BackingField
	{
		get
		{
			return progressType;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref progressType, 4096uL, null);
		}
	}

	public Quest_KillToLiftCurse Networkquest
	{
		get
		{
			return GetSyncVarNetworkBehaviour(___questNetId, ref quest);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref quest, 8192uL, null, ref ___questNetId);
		}
	}

	public float GetValue(float[] values)
	{
		return values.GetValue(currentStrength);
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		string curseName = GetName();
		if (base.victim.isOwned)
		{
			InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_Curse_YouHaveBeenCursed");
		}
		string color = "#f57162";
		string message = string.Format(DewLocalization.GetUIValue("Chat_PlayerCursed"), "<color=white>" + ChatManager.GetDescribedPlayerName(base.victim.owner) + "</color>", "<color=white>" + curseName + "</color>");
		NetworkedManagerBase<ChatManager>.instance.ShowMessageLocally(new ChatManager.Message
		{
			type = ChatManager.MessageType.Raw,
			content = "<color=" + color + ">" + message + "</color>"
		});
		icon = Resources.Load<Sprite>("Sprites/texCurse");
		iconOrder = 1;
		if (base.isServer)
		{
			base.NetworkshowIcon = true;
			_tracker = base.victim.TrackKills(8f, OnKill);
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
			((Hero)base.victim).ClientHeroEvent_OnKnockedOut += new Action<EventInfoKill>(OnKnockOut);
			Networkquest = NetworkedManagerBase<QuestManager>.instance.StartQuest(delegate(Quest_KillToLiftCurse q)
			{
				q.NetworktargetEffect = this;
			});
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer)
		{
			if (!Networkquest.IsNullOrInactive())
			{
				Networkquest.CompleteQuest();
				Networkquest = null;
			}
			if (_tracker != null)
			{
				_tracker.Stop();
				_tracker = null;
			}
			if (NetworkedManagerBase<ZoneManager>.instance != null)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoaded);
			}
			if (base.victim is Hero h)
			{
				h.ClientHeroEvent_OnKnockedOut -= new Action<EventInfoKill>(OnKnockOut);
			}
		}
	}

	public virtual bool IsViable(Entity target)
	{
		return true;
	}

	private void OnKnockOut(EventInfoKill obj)
	{
		if (base.isActive)
		{
			if (!Networkquest.IsNullOrInactive())
			{
				Networkquest.FailQuest(QuestFailReason.NotSpecified);
			}
			Destroy();
		}
	}

	private void OnKill(EventInfoKill obj)
	{
		if (!base.isActive || !(obj.victim is Monster) || progressType != QuestProgressType.Kills)
		{
			return;
		}
		if (obj.victim is BossMonster)
		{
			for (int i = 0; i < 10; i++)
			{
				IncrementProgress();
			}
		}
		else
		{
			IncrementProgress();
		}
	}

	private void ClientEventOnRoomLoaded(EventInfoLoadRoom _)
	{
		if (!base.isActive || progressType != QuestProgressType.Travel)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(delegate
		{
			if (!SingletonDewNetworkBehaviour<Room>.instance.isRevisit)
			{
				IncrementProgress();
			}
		});
	}

	private void IncrementProgress()
	{
		if (!this.IsNullOrInactive())
		{
			currentProgress++;
			try
			{
				CurseEvent_OnProgressChanged?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			if (currentProgress >= requiredAmount)
			{
				Destroy();
			}
		}
	}

	public new T GetValue<T>(T[] arr)
	{
		return arr.GetClamped(currentStrength.GetValueIndex());
	}

	public string GetName()
	{
		string key = DewLocalization.GetCurseKey(GetType().Name);
		if (currentStrength == availableStrengths)
		{
			return DewLocalization.GetCurseName(key);
		}
		return currentStrength switch
		{
			HatredStrengthType.None => DewLocalization.GetCurseName(key), 
			HatredStrengthType.Mild => DewLocalization.GetCurseName(key) + " I", 
			HatredStrengthType.Potent => DewLocalization.GetCurseName(key) + " II", 
			HatredStrengthType.Powerful => DewLocalization.GetCurseName(key) + " III", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_HatredStrengthType(writer, currentStrength);
			writer.WriteInt(requiredAmount);
			writer.WriteInt(currentProgress);
			GeneratedNetworkCode._Write_QuestProgressType(writer, progressType);
			writer.WriteNetworkBehaviour(Networkquest);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x200L) != 0L)
		{
			GeneratedNetworkCode._Write_HatredStrengthType(writer, currentStrength);
		}
		if ((base.syncVarDirtyBits & 0x400L) != 0L)
		{
			writer.WriteInt(requiredAmount);
		}
		if ((base.syncVarDirtyBits & 0x800L) != 0L)
		{
			writer.WriteInt(currentProgress);
		}
		if ((base.syncVarDirtyBits & 0x1000L) != 0L)
		{
			GeneratedNetworkCode._Write_QuestProgressType(writer, progressType);
		}
		if ((base.syncVarDirtyBits & 0x2000L) != 0L)
		{
			writer.WriteNetworkBehaviour(Networkquest);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref currentStrength, null, GeneratedNetworkCode._Read_HatredStrengthType(reader));
			GeneratedSyncVarDeserialize(ref requiredAmount, null, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref currentProgress, null, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref progressType, null, GeneratedNetworkCode._Read_QuestProgressType(reader));
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref quest, null, reader, ref ___questNetId);
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x200L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref currentStrength, null, GeneratedNetworkCode._Read_HatredStrengthType(reader));
		}
		if ((num & 0x400L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref requiredAmount, null, reader.ReadInt());
		}
		if ((num & 0x800L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref currentProgress, null, reader.ReadInt());
		}
		if ((num & 0x1000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref progressType, null, GeneratedNetworkCode._Read_QuestProgressType(reader));
		}
		if ((num & 0x2000L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref quest, null, reader, ref ___questNetId);
		}
	}
}
