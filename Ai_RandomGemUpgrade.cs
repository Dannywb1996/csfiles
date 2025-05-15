using System;
using System.Collections.Generic;
using UnityEngine;

public class Ai_RandomGemUpgrade : StandardProjectile
{
	public int addedQuality = 50;

	public int addedLevel = 1;

	public float deviateMag = 3f;

	private Vector3 _deviateVector;

	[NonSerialized]
	public Actor upgradeTarget;

	protected override void OnPrepare()
	{
		base.OnPrepare();
		SetCustomStartPosition(base.position);
	}

	protected override void OnCreate()
	{
		_deviateVector = UnityEngine.Random.insideUnitSphere * deviateMag;
		if (_deviateVector.y < 0f)
		{
			_deviateVector.y *= -1f;
		}
		base.OnCreate();
	}

	protected override Vector3 PositionSolver(float dt)
	{
		return base.PositionSolver(dt) + Mathf.Sin(base.normalizedPosition * MathF.PI) * _deviateVector;
	}

	protected override void OnComplete()
	{
		base.OnComplete();
		if (!(base.targetEntity is Hero h))
		{
			return;
		}
		if (upgradeTarget.IsNullOrInactive())
		{
			if (h.Skill.gems.Count > 0)
			{
				List<Gem> gems = new List<Gem>(h.Skill.gems.Values);
				upgradeTarget = Dew.SelectRandomWeightedInList(gems, (Gem gem) => gem.quality);
			}
			else
			{
				List<SkillTrigger> skills = new List<SkillTrigger>();
				foreach (KeyValuePair<int, AbilityTrigger> ability in h.Ability.abilities)
				{
					if (ability.Value is SkillTrigger { isLevelUpEnabled: not false } skill)
					{
						skills.Add(skill);
					}
				}
				upgradeTarget = Dew.SelectRandomWeightedInList(skills, (SkillTrigger skillTrigger) => skillTrigger.level);
			}
		}
		if (upgradeTarget is Gem g)
		{
			g.quality += addedQuality;
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemUpgraded(h, g);
		}
		if (upgradeTarget is SkillTrigger s)
		{
			s.level += addedLevel;
			NetworkedManagerBase<ClientEventManager>.instance.InvokeOnItemUpgraded(h, s);
		}
	}

	private void MirrorProcessed()
	{
	}
}
