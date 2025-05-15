using System;
using UnityEngine;

public class PropEntity : Entity, IProp
{
	public bool takeOneDamageOnHit;

	public float hitInvulnerableTime = 0.1f;

	public bool isUnstoppable;

	[SerializeField]
	private bool _scaleSpawnRateWithPlayers;

	public virtual bool isRegularReward => true;

	Quaternion? IProp.customSpawnRotation => Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

	public bool scaleSpawnRateWithPlayers => _scaleSpawnRateWithPlayers;

	protected override StaggerSettings GetStaggerSettings()
	{
		return new StaggerSettings
		{
			enabled = false
		};
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			if (isUnstoppable)
			{
				CreateBasicEffect(this, new UnstoppableEffect(), float.PositiveInfinity);
			}
			EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (takeOneDamageOnHit && hitInvulnerableTime > 0f)
		{
			CreateBasicEffect(this, new InvulnerableEffect(), hitInvulnerableTime, "prop_iframe");
		}
	}

	private void MirrorProcessed()
	{
	}
}
