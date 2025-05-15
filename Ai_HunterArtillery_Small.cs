using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_HunterArtillery_Small : InstantDamageInstance
{
	public float slowDuration = 1f;

	public float slowAmount = 20f;

	public bool isSlowDecay;

	[NonSerialized]
	[SyncVar]
	public float speedMultiplier = 1f;

	public float NetworkspeedMultiplier
	{
		get
		{
			return speedMultiplier;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref speedMultiplier, 64uL, null);
		}
	}

	protected override void OnCreate()
	{
		ListReturnHandle<ParticleSystem> handle;
		foreach (ParticleSystem item in ((Component)this).GetComponentsInChildrenNonAlloc(out handle))
		{
			ParticleSystem.MainModule m = item.main;
			m.simulationSpeed *= speedMultiplier;
		}
		if (base.isServer)
		{
			damageDelay /= speedMultiplier;
		}
		handle.Return();
		base.OnCreate();
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new SlowEffect
		{
			decay = isSlowDecay,
			strength = slowAmount
		}, slowDuration, "ArtillerySlow", DuplicateEffectBehavior.UsePrevious);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(speedMultiplier);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x40L) != 0L)
		{
			writer.WriteFloat(speedMultiplier);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref speedMultiplier, null, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x40L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref speedMultiplier, null, reader.ReadFloat());
		}
	}
}
