using System.Collections;
using UnityEngine;

public class Ai_Mon_DarkCave_DarkElemental_Barrage_Spawner : AbilityInstance
{
	public DewCollider targetRange;

	public int shootCount;

	public float maxDeviation;

	public Vector2 landTime;

	public float interval;

	public float minRangeFromSelf = 2.5f;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!base.isServer)
		{
			yield break;
		}
		DestroyOnDeath(base.info.caster);
		targetRange.transform.position = base.info.target.position;
		ArrayReturnHandle<Entity> handle;
		Entity[] ents = targetRange.GetEntities(out handle, tvDefaultHarmfulEffectTargets, new CollisionCheckSettings
		{
			includeUncollidable = true,
			sortComparer = CollisionCheckSettings.Random
		}).ToArray();
		handle.Return();
		if (ents.Length == 0)
		{
			Destroy();
			yield break;
		}
		Vector3[] lastKnownPositions = new Vector3[ents.Length];
		for (int i = 0; i < shootCount; i++)
		{
			int num = Random.Range(0, ents.Length);
			Entity entity = ents[num];
			if (entity != null)
			{
				lastKnownPositions[num] = entity.position;
			}
			Vector3 pos = lastKnownPositions[num] + Random.onUnitSphere.Flattened().normalized * (Random.value * maxDeviation);
			Vector3 vector = (pos - base.info.caster.position).Flattened();
			if (vector.sqrMagnitude < minRangeFromSelf * minRangeFromSelf)
			{
				vector = vector.normalized * minRangeFromSelf;
				pos = base.info.caster.position + vector;
			}
			pos = Dew.GetPositionOnGround(pos);
			CreateAbilityInstance(base.info.caster.position, Quaternion.identity, new CastInfo(base.info.caster, pos), delegate(Ai_Mon_DarkCave_DarkElemental_Barrage_Arrow a)
			{
				a.initialSpeed = Vector3.Distance(base.info.caster.position, pos) / Random.Range(landTime.x, landTime.y);
				a.targetSpeed = a.initialSpeed;
				a.acceleration = 0f;
			});
			yield return new SI.WaitForSeconds(interval);
		}
		Destroy();
	}

	private void MirrorProcessed()
	{
	}
}
