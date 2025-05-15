using System;
using System.Collections;
using UnityEngine;

public class Ai_C_Starfall : AbilityInstance
{
	public float minStartPos;

	public float maxStartPos;

	public float interval;

	public DewCollider range;

	public GameObject shootEffect;

	public GameObject shootEffectCaster;

	public ScalingValue dmgPerStar;

	public ScalingValue baseCount;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!base.isServer)
		{
			yield break;
		}
		DestroyOnDeath(base.info.caster);
		int count = Mathf.RoundToInt(GetValue(baseCount));
		for (int i = 0; i < count; i++)
		{
			ArrayReturnHandle<Entity> handle;
			ReadOnlySpan<Entity> list = DewPhysics.OverlapCircleAllEntities(out handle, base.info.caster.position, range.radius, tvDefaultHarmfulEffectTargets);
			if (list.Length > 0)
			{
				Entity entity = Dew.SelectRandomWeightedInList(list, (Entity e) => e.maxHealth);
				Vector3 normalized = UnityEngine.Random.insideUnitSphere.normalized;
				Vector3 vector = base.info.caster.Visual.GetCenterPosition() + normalized * UnityEngine.Random.Range(minStartPos, maxStartPos);
				FxPlayNewNetworked(shootEffect, vector, Quaternion.identity);
				FxPlayNewNetworked(shootEffectCaster, base.info.caster);
				if (!entity.IsNullInactiveDeadOrKnockedOut())
				{
					CreateAbilityInstance(entity.position, null, new CastInfo(base.info.caster, entity), delegate(Ai_C_Starfall_Projectile b)
					{
						b.dmgFactor = dmgPerStar;
					});
				}
			}
			yield return new SI.WaitForSeconds(interval);
			handle.Return();
			handle = default(ArrayReturnHandle<Entity>);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
