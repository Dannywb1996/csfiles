using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class ElementalStatusEffect : StackedStatusEffect
{
	[NonSerialized]
	[SyncVar]
	public float ampAmount;

	public ParticleSystem[] stackScaledEmissions;

	private ParticleSystem.EmissionModule[] _stackScaledEmissions;

	private float[] _originalROD;

	private float[] _originalROT;

	public float NetworkampAmount
	{
		get
		{
			return ampAmount;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref ampAmount, 2048uL, null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (stackScaledEmissions != null)
		{
			_stackScaledEmissions = new ParticleSystem.EmissionModule[stackScaledEmissions.Length];
			_originalROD = new float[stackScaledEmissions.Length];
			_originalROT = new float[stackScaledEmissions.Length];
			for (int i = 0; i < stackScaledEmissions.Length; i++)
			{
				ParticleSystem p = stackScaledEmissions[i];
				_stackScaledEmissions[i] = p.emission;
				_originalROD[i] = _stackScaledEmissions[i].rateOverDistanceMultiplier;
				_originalROT[i] = _stackScaledEmissions[i].rateOverTimeMultiplier;
			}
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		UpdateStackScaledEmissions();
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
		base.OnStackChange(oldStack, newStack);
		UpdateStackScaledEmissions();
	}

	private void UpdateStackScaledEmissions()
	{
		if (stackScaledEmissions != null)
		{
			for (int i = 0; i < _stackScaledEmissions.Length; i++)
			{
				ParticleSystem.EmissionModule p = _stackScaledEmissions[i];
				p.rateOverDistanceMultiplier = _originalROD[i] * (float)base.stack;
				p.rateOverTimeMultiplier = _originalROT[i] * (float)base.stack;
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(ampAmount);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x800L) != 0L)
		{
			writer.WriteFloat(ampAmount);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref ampAmount, null, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x800L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref ampAmount, null, reader.ReadFloat());
		}
	}
}
