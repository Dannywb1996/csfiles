using System;
using System.Runtime.InteropServices;
using Mirror;

public class Quest_KillToLiftCurse : DewQuest
{
	[NonSerialized]
	[SyncVar]
	public CurseStatusEffect targetEffect;

	protected NetworkBehaviourSyncVar ___targetEffectNetId;

	public CurseStatusEffect NetworktargetEffect
	{
		get
		{
			return GetSyncVarNetworkBehaviour(___targetEffectNetId, ref targetEffect);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref targetEffect, 32uL, null, ref ___targetEffectNetId);
		}
	}

	public override bool IsVisibleLocally()
	{
		return NetworktargetEffect.victim.isOwned;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		string key = DewLocalization.GetCurseKey(NetworktargetEffect.GetType().Name);
		base.questTitleRaw = NetworktargetEffect.GetName();
		base.questShortDescriptionRaw = DewLocalization.GetCurseShortDescription(key).ToText(NetworktargetEffect);
		base.questDetailedDescriptionRaw = DewLocalization.GetCurseDescription(key).ToText(NetworktargetEffect);
		if (base.isServer)
		{
			CurseStatusEffect networktargetEffect = NetworktargetEffect;
			networktargetEffect.CurseEvent_OnProgressChanged = (Action)Delegate.Combine(networktargetEffect.CurseEvent_OnProgressChanged, new Action(ClientEventOnProgressChanged));
			UpdateProgress();
		}
	}

	private void ClientEventOnProgressChanged()
	{
		UpdateProgress();
	}

	private void UpdateProgress()
	{
		base.progressType = NetworktargetEffect.progressType;
		base.currentProgress = (NetworktargetEffect.requiredAmount - NetworktargetEffect.currentProgress).ToString("#,###");
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteNetworkBehaviour(NetworktargetEffect);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteNetworkBehaviour(NetworktargetEffect);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref targetEffect, null, reader, ref ___targetEffectNetId);
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x20L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref targetEffect, null, reader, ref ___targetEffectNetId);
		}
	}
}
