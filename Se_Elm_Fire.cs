using UnityEngine;

public class Se_Elm_Fire : ElementalStatusEffect
{
	public float heroMultiplier;

	public float bossMultiplier;

	private float _lastDamageTick;

	private float _damageMultiplier;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			_lastDamageTick = Time.time;
			DoFireDamage();
			base.victim.Status.fireStack = base.stack;
			float scalingFactor = NetworkedManagerBase<GameManager>.instance.difficulty.scalingFactor;
			if (base.victim is Hero h)
			{
				_damageMultiplier = 1f + h.Status.scalingStats.maxHealthPercentage * 0.01f * (float)h.Status.level * 2f;
			}
			else if (base.victim is BossMonster)
			{
				_damageMultiplier = NetworkedManagerBase<GameManager>.instance.GetBossMonsterHealthMultiplierByScaling(scalingFactor);
			}
			else if (base.victim is Monster { type: Monster.MonsterType.MiniBoss })
			{
				_damageMultiplier = NetworkedManagerBase<GameManager>.instance.GetMiniBossMonsterHealthMultiplierByScaling(scalingFactor);
			}
			else
			{
				_damageMultiplier = NetworkedManagerBase<GameManager>.instance.GetRegularMonsterHealthMultiplierByScaling(scalingFactor);
			}
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (base.isServer && !(base.victim is Monster { isSleeping: not false }) && Time.time - _lastDamageTick > 0.25f)
		{
			_lastDamageTick += 0.25f;
			DoFireDamage();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer)
		{
			if (base.victim != null && base.victim.Status.isAlive && Time.time - _lastDamageTick > 0.125f)
			{
				DoFireDamage();
			}
			if (base.victim != null && base.victim.Status.isAlive)
			{
				base.victim.Status.fireStack = 0;
			}
		}
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
		base.OnStackChange(oldStack, newStack);
		if ((newStack != 0 || !base.victim.Status.isDead) && base.isServer)
		{
			base.victim.Status.fireStack = newStack;
		}
	}

	private void DoFireDamage()
	{
		float multiplier = _damageMultiplier;
		multiplier *= 1f + (float)(base.stack - 1) * 0.29999995f;
		DamageData data = new DamageData(DamageData.SourceType.Default, 3.5f * multiplier, 0f);
		data.ApplyAmplification(ampAmount);
		if (base.victim is Hero)
		{
			data.ApplyRawMultiplier(heroMultiplier);
		}
		if (base.victim is Monster { type: Monster.MonsterType.Boss })
		{
			data.ApplyRawMultiplier(bossMultiplier);
		}
		data.SetAttr(DamageAttribute.DamageOverTime);
		DealDamage(data, base.victim);
	}

	private void MirrorProcessed()
	{
	}
}
