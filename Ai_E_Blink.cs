using System;
using System.Collections;
using UnityEngine;

public class Ai_E_Blink : AbilityInstance
{
	public bool enableGoBack;

	public GameObject prepareEffect;

	public float blinkDelay;

	public DewCollider range;

	public GameObject explodeEffect;

	public GameObject hitEffect;

	public GameObject goBackPositionEffect;

	public GameObject halfDissolveEffect;

	public float reducedRatioPerEnemy;

	public ScalingValue damageAmount;

	public float backDuration;

	protected override IEnumerator OnCreateSequenced()
	{
		Vector3 teleportPos = Dew.GetValidAgentDestination_Closest(base.info.caster.agentPosition, base.info.point);
		base.rotation = Quaternion.LookRotation(teleportPos - base.position).Flattened();
		FxPlay(prepareEffect);
		if (!base.isServer)
		{
			yield break;
		}
		FxPlayNetworked(goBackPositionEffect);
		FxPlayNetworked(halfDissolveEffect, base.info.caster);
		((St_E_Blink)base.firstTrigger).backLocation = base.info.caster.agentPosition;
		Hero hero = base.info.caster as Hero;
		base.info.caster.Control.StartDaze(blinkDelay);
		yield return new SI.WaitForSeconds(blinkDelay);
		if (hero == null || hero.isKnockedOut || !hero.isActive)
		{
			Destroy();
			yield break;
		}
		Teleport(base.info.caster, teleportPos);
		FxPlayNetworked(explodeEffect, teleportPos, Quaternion.identity);
		range.transform.position = teleportPos;
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> entities = range.GetEntities(out handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Length; i++)
		{
			FxPlayNewNetworked(hitEffect, entities[i]);
			Damage(damageAmount).SetElemental(ElementalType.Fire).SetOriginPosition(teleportPos).Dispatch(entities[i]);
		}
		if (base.firstTrigger != null)
		{
			base.firstTrigger.ApplyCooldownReductionByRatio(reducedRatioPerEnemy * (float)entities.Length);
		}
		handle.Return();
		if (!enableGoBack)
		{
			Destroy();
			yield break;
		}
		base.firstTrigger.SetCharge(1, 0);
		base.firstTrigger.ChangeConfigTimedOnce(1, backDuration, delegate
		{
			Destroy();
		}, base.Destroy);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(goBackPositionEffect);
		FxStop(halfDissolveEffect);
	}

	private void MirrorProcessed()
	{
	}
}
