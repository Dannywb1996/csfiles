using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_E_StygianRush : AbilityInstance
{
	public ChargingChannelData channel;

	public ScalingValue minDamage;

	public ScalingValue maxDamage;

	public float lengthMin;

	public float lengthMax;

	public float cooldownRefundRatioOnCancel;

	public float fullGraceThreshold;

	public bool doUnstoppable;

	public AnimationCurve shakeMultiplier;

	public AnimationCurve pitchMultiplier;

	public AnimationCurve volumeMultiplier;

	private ChargingChannel _channel;

	private StatusEffect _unstoppable;

	[SyncVar]
	private float _chargeAmount;

	[SyncVar]
	private float _angle;

	public float Network_chargeAmount
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

	public float Network_angle
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

	private void OnCanceled()
	{
	}

	protected override void OnDestroyActor()
	{
	}

	protected override void ActiveLogicUpdate(float dt)
	{
	}

	private void DoRush(ChargingChannel obj)
	{
	}

	private float GetLength()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
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
