using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_C_Pew_Projectile : StandardProjectile
{
	[NonSerialized]
	[SyncVar]
	internal float chargeAmount;

	public ScalingValue damageMin;

	public ScalingValue damageMax;

	public float critThreshold;

	public float procCoefficient;

	public bool doKnockback;

	public Knockback targetKnockback;

	public DewAudioSource[] adjustedSounds;

	public AnimationCurve soundPitch;

	public AnimationCurve soundVolume;

	public Transform[] scaleAdjustedTransforms;

	public AnimationCurve transformScale;

	public Light[] adjustedLights;

	public AnimationCurve lightIntensity;

	public FxCameraShake[] adjustedShakes;

	public AnimationCurve shakeAmplitude;

	public float NetworkchargeAmount
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

	protected override void OnEntity(EntityHit hit)
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
