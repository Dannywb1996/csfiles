using System;
using System.Collections;
using UnityEngine;

public class Ai_C_Purgatory : AbilityInstance
{
	public float delay;

	public GameObject explodeEffect;

	public ScalingValue damage;

	public GameObject hitEffect;

	public DewCollider range;

	public float knockupStrength = 0.7f;

	protected override IEnumerator OnCreateSequenced()
	{
		base.transform.SetPositionAndRotation(base.info.point, Quaternion.identity);
		if (!base.isServer)
		{
			yield break;
		}
		yield return new SI.WaitForSeconds(delay);
		FxPlayNetworked(explodeEffect);
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> list = DewPhysics.OverlapCircleAllEntities(out handle, base.transform.position, 20f, tvDefaultUsefulEffectTargets, new CollisionCheckSettings
		{
			includeUncollidable = true
		});
		ArrayReturnHandle<Entity> handle2;
		ReadOnlySpan<Entity> entities = range.GetEntities(out handle2, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Length; i++)
		{
			Entity v = entities[i];
			v.Visual.KnockUp(knockupStrength, isFriendly: false);
			Damage(damage).SetDirection(Vector3.up).Dispatch(v);
			FxPlayNewNetworked(hitEffect, v);
			for (int j = 0; j < ((!v.IsAnyBoss()) ? 1 : 3); j++)
			{
				Entity entity = Dew.SelectRandomWeightedInList(list, (Entity e) => Mathf.Clamp(1f - e.currentHealth / e.maxHealth, 0.001f, 1f));
				if (entity != null)
				{
					CreateAbilityInstance(base.transform.position, null, new CastInfo(base.info.caster, entity), delegate(Ai_C_Purgatory_Heal heal)
					{
						heal.SetCustomStartPosition(v.Visual.GetCenterPosition());
					});
				}
			}
		}
		handle2.Return();
		handle.Return();
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
