using UnityEngine;

public class Ai_L_ShoutOfOblivion : InstantDamageInstance
{
	public float damageAmpPerThreatLevel = 0.2f;

	public float stunDuration = 3f;

	public float currentDamageAmp
	{
		get
		{
			if (!(NetworkedManagerBase<ZoneManager>.instance != null))
			{
				return 0f;
			}
			return damageAmpPerThreatLevel * (float)Mathf.Clamp(NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel, 0, 5);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			base.info.caster.Control.StartDisplacement(new DispByDestination
			{
				destination = base.info.caster.agentPosition - base.info.forward * 4f,
				duration = 0.4f,
				ease = DewEase.EaseOutQuad,
				isFriendly = true,
				rotateForward = false,
				isCanceledByCC = false,
				canGoOverTerrain = false
			});
		}
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
		base.OnBeforeDispatchDamage(ref dmg, target);
		if (!(currentDamageAmp <= 0.001f))
		{
			dmg.ApplyAmplification(currentDamageAmp);
			dmg.SetAttr(DamageAttribute.IsCrit);
		}
	}

	protected override void OnHit(Entity entity)
	{
		base.OnHit(entity);
		CreateBasicEffect(entity, new StunEffect(), stunDuration);
	}

	private void MirrorProcessed()
	{
	}
}
