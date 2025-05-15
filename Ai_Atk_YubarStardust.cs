using System;

public class Ai_Atk_YubarStardust : AttackProjectile
{
	[NonSerialized]
	public Entity chainTarget0;

	[NonSerialized]
	public Entity chainTarget1;

	[NonSerialized]
	public Entity chainTarget2;

	[NonSerialized]
	public float chainStrength;

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		if (!(hit.entity != base.info.target))
		{
			TryChainTarget(chainTarget0);
			TryChainTarget(chainTarget1);
			TryChainTarget(chainTarget2);
		}
		void TryChainTarget(Entity target)
		{
			if (!target.IsNullInactiveDeadOrKnockedOut() && !(target == base.info.target))
			{
				isMain = false;
				strength = chainStrength;
				OnEntity(new EntityHit
				{
					entity = target,
					point = target.Visual.GetCenterPosition()
				});
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
