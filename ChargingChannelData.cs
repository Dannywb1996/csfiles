using System;
using Mirror;
using UnityEngine;

[Serializable]
public class ChargingChannelData
{
	public DewAnimationClip chargeAnim;

	public GameObject chargeEffect;

	public DewAnimationClip chargeFullAnim;

	public GameObject chargeFullEffect;

	public DewAnimationClip castAnim;

	public GameObject castEffect;

	public DewAnimationClip completeAnim;

	public GameObject completeEffect;

	public DewAnimationClip cancelAnim;

	public GameObject cancelEffect;

	public float chargeFullDuration = 1f;

	public float completeDuration = 4f;

	public float castDazeDuration;

	public float completeDazeDuration;

	public AbilitySelfValidator selfValidator;

	public AbilityTargetValidator targetValidator;

	public CastMethodData castMethod;

	public float angleSpeedLimit = -1f;

	public bool setFillAmount = true;

	public bool showOnScreenTimer;

	public bool completeWhenCast = true;

	public bool showCastIndicator = true;

	public bool canMove;

	public float selfSlowAmount;

	public bool canCancel;

	public bool rotateForwardWhenCharging;

	public bool rotateForwardWhenCast;

	public bool rotateForwardWhenComplete;

	public float rotateForwardOverrideRoationTime = 0.25f;

	public bool castOnButtonRelease;

	public ChargingChannel Get(NetworkIdentity effectParent)
	{
		return new ChargingChannel
		{
			_effectParent = effectParent,
			castMethod = new CastMethodData(castMethod),
			_data = this
		};
	}
}
