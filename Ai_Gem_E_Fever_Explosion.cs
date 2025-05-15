using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Gem_E_Fever_Explosion : InstantDamageInstance
{
	public Transform[] limitedScaledTransforms;

	public Transform[] scaledTransforms;

	public FxPointLight light;

	[NonSerialized]
	[SyncVar]
	public float explosionRadius = 1f;

	[NonSerialized]
	[SyncVar]
	public float damageAmp;

	public float NetworkexplosionRadius
	{
		get
		{
			return explosionRadius;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref explosionRadius, 64uL, null);
		}
	}

	public float NetworkdamageAmp
	{
		get
		{
			return damageAmp;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref damageAmp, 128uL, null);
		}
	}

	protected override void OnCreate()
	{
		Transform[] array = limitedScaledTransforms;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].localScale *= Mathf.Clamp(explosionRadius, 0f, 5f);
		}
		array = scaledTransforms;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].localScale *= explosionRadius;
		}
		light.intensityMultiplier *= explosionRadius;
		light.rangeMultiplier *= explosionRadius;
		GetComponentInChildren<FxCameraShake>().amplitude *= explosionRadius * 1.1f;
		DewAudioSource componentInChildren = GetComponentInChildren<DewAudioSource>();
		componentInChildren.volumeMultiplier = Mathf.Clamp(0.3f + explosionRadius * 0.1f, 0f, 1f);
		componentInChildren.pitchMultiplier = Mathf.Clamp(2f - explosionRadius * 0.2f, 0.75f, 1.7f);
		base.OnCreate();
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if (damageAmp > 0.25f)
		{
			dmg.SetAttr(DamageAttribute.IsCrit);
		}
		dmg.ApplyAmplification(damageAmp);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(explosionRadius);
			writer.WriteFloat(damageAmp);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x40L) != 0L)
		{
			writer.WriteFloat(explosionRadius);
		}
		if ((base.syncVarDirtyBits & 0x80L) != 0L)
		{
			writer.WriteFloat(damageAmp);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref explosionRadius, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref damageAmp, null, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x40L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref explosionRadius, null, reader.ReadFloat());
		}
		if ((num & 0x80L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref damageAmp, null, reader.ReadFloat());
		}
	}
}
