using System.Collections;
using UnityEngine;

public class Ai_E_ClutchesOfMalice : AbilityInstance
{
	public ScalingValue maxTargetCount;

	public Transform[] scaledTransforms;

	public float maxSustainTime;

	public DewCollider range;

	public float additionalRootTime;

	public GameObject fxStart;

	public GameObject fxCancel;

	public GameObject fxComplete;

	public float customStartHeight;

	protected override IEnumerator OnCreateSequenced()
	{
		St_E_ClutchesOfMalice st_E_ClutchesOfMalice = base.firstTrigger as St_E_ClutchesOfMalice;
		if (st_E_ClutchesOfMalice != null)
		{
			Transform[] array = scaledTransforms;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].localScale *= GetValue(st_E_ClutchesOfMalice.scale);
			}
		}
		if (!base.isServer)
		{
			yield break;
		}
		Vector3 startPos = base.info.point + Vector3.up * customStartHeight;
		range.transform.position = base.info.point;
		float endTime = Time.time + maxSustainTime;
		ArrayReturnHandle<Entity> handle;
		Entity[] array2 = range.GetEntities(out handle, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			sortComparer = CollisionCheckSettings.Random
		}).ToArray();
		int num = 0;
		Entity[] array3 = array2;
		for (int i = 0; i < array3.Length; i++)
		{
			if (array3[i] is Monster)
			{
				num++;
			}
		}
		if (num == 0)
		{
			FxPlayNetworked(fxCancel, startPos, null);
			base.firstTrigger.ApplyCooldownReductionByRatio(0.8f);
			Destroy();
			yield break;
		}
		float interval = maxSustainTime / (float)num;
		if (interval > 0.05f)
		{
			interval = 0.05f;
		}
		FxPlayNetworked(fxStart, startPos, null);
		int pulledMonsters = 0;
		int maxCount = Mathf.RoundToInt(GetValue(maxTargetCount));
		Entity[] array4 = array2;
		foreach (Entity entity in array4)
		{
			if (pulledMonsters >= maxCount)
			{
				break;
			}
			if (entity is Monster && !entity.IsNullInactiveDeadOrKnockedOut())
			{
				CreateAbilityInstance(startPos, null, new CastInfo(base.info.caster, entity), delegate(Ai_E_ClutchesOfMalice_ReachAndPull b)
				{
					b.endSyncTime = endTime;
				});
				CreateStatusEffect(entity, delegate(Se_E_ClutchesOfMalice_Root b)
				{
					b.duration = maxSustainTime + additionalRootTime;
				});
				pulledMonsters++;
				yield return new SI.WaitForSeconds(interval);
			}
		}
		handle.Return();
		yield return new SI.WaitForSeconds(endTime - Time.time);
		FxStopNetworked(fxStart);
		FxPlayNetworked(fxComplete, startPos, null);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
