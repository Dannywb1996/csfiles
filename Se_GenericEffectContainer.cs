using System.Runtime.InteropServices;
using Mirror;

public class Se_GenericEffectContainer : StatusEffect
{
	public BasicEffect effect;

	public float duration;

	[SyncVar]
	internal string _id;

	public string id => _id;

	public string Network_id
	{
		get
		{
			return _id;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _id, 512uL, null);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			SetTimer(duration);
			DoBasicEffect(effect);
		}
	}

	public override string GetActorReadableName()
	{
		return base.GetActorReadableName() + " '" + id + "' (" + effect.GetType().Name + ")";
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteString(_id);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x200L) != 0L)
		{
			writer.WriteString(_id);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref _id, null, reader.ReadString());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x200L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _id, null, reader.ReadString());
		}
	}
}
