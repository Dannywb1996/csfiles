public class Se_HeroBleedingOut : StatusEffect
{
	private class Ad_BleedOuts
	{
		public int count;
	}

	private AbilityLockHandle _handle;

	public Hero heroVictim => base.victim as Hero;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			_handle = heroVictim.Ability.GetNewAbilityLockHandle();
			_handle.LockAllAbilitiesCast();
			DoSlow(75f);
			DoUncollidable();
			DoInvisible();
			heroVictim.takenHealProcessor.Add(Processor);
			if (!heroVictim.TryGetData<Ad_BleedOuts>(out var data))
			{
				data = new Ad_BleedOuts();
				heroVictim.AddData(data);
			}
			data.count++;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer)
		{
			if (heroVictim != null)
			{
				heroVictim.takenHealProcessor.Remove(Processor);
			}
			if (_handle != null)
			{
				_handle.Stop();
				_handle = null;
			}
		}
	}

	private void Processor(ref HealData data, Actor actor, Entity target)
	{
		data.ApplyRawMultiplier(0f);
	}

	private void MirrorProcessed()
	{
	}
}
