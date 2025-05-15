public static class EntityCheck
{
	public static bool IsNullInactiveDeadOrKnockedOut(this Entity e)
	{
		if (!(e == null) && e.isActive && !e.Status.isDead)
		{
			if (e is Hero h)
			{
				return h.isKnockedOut;
			}
			return false;
		}
		return true;
	}

	public static bool IsAnyBoss(this Entity e)
	{
		if (!(e is BossMonster))
		{
			if (e is Monster m)
			{
				if (m.type != Monster.MonsterType.Boss)
				{
					return m.type == Monster.MonsterType.MiniBoss;
				}
				return true;
			}
			return false;
		}
		return true;
	}
}
