using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_Gem_E_Crimson : InstantDamageInstance
{
	public float bossHealAmp;

	public ScalingValue healPerHit;

	public Transform[] scaledTransforms;

	public Transform[] scaledTransformsOnLocal;

	[NonSerialized]
	[SyncVar]
	public float sizeMultiplier;

	public float NetworksizeMultiplier
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
		[param: In]
		set
		{
		}
	}

	protected override void OnCreate()
	{
	}

	protected override void OnHit(Entity entity)
	{
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
	}
}
