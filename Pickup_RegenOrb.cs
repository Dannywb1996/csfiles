using System;
using UnityEngine;

public class Pickup_RegenOrb : PickupInstance
{
	public DewCollider range;

	protected override void OnPickup(Hero hero)
	{
		base.OnPickup(hero);
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> entities = range.GetEntities(out handle, delegate(Entity ent)
		{
			EntityRelation relation = hero.GetRelation(ent);
			if (ent is Hero { isKnockedOut: not false })
			{
				return false;
			}
			return relation == EntityRelation.Ally || relation == EntityRelation.Self;
		}, new CollisionCheckSettings
		{
			includeUncollidable = true
		});
		handle.Return();
		ReadOnlySpan<Entity> readOnlySpan = entities;
		for (int num = 0; num < readOnlySpan.Length; num++)
		{
			Entity e = readOnlySpan[num];
			CreateAbilityInstance<Ai_RegenOrb_Projectile>(base.position, Quaternion.identity, new CastInfo(hero, e));
		}
	}

	protected override bool CanBeUsedBy(Hero hero)
	{
		return base.CanBeUsedBy(hero);
	}

	private void MirrorProcessed()
	{
	}
}
