using System;
using Mirror;

public class DewStarItemOld
{
	private StatBonus _genericBonus;

	public virtual Type affectedSkill { get; }

	public virtual bool affectsMovementSkill { get; }

	public virtual int maxLevel => 1;

	public bool isServer => NetworkServer.active;

	public bool isActive { get; internal set; }

	public DewPlayer player { get; internal set; }

	public int level { get; internal set; }

	public Hero hero
	{
		get
		{
			if (player == null)
			{
				return null;
			}
			return player.hero;
		}
	}

	public bool IsSkillRelated(string skillName)
	{
		if (!(affectedSkill != null) || !(affectedSkill.Name == skillName))
		{
			if (affectsMovementSkill)
			{
				return skillName.Contains("_M_");
			}
			return false;
		}
		return true;
	}

	public bool IsSkillRelated(SkillTrigger skill)
	{
		return IsSkillRelated(skill.GetType().Name);
	}

	public virtual bool ShouldInitInGame()
	{
		return true;
	}

	public virtual void OnStartInGame()
	{
	}

	public virtual void OnStopInGame()
	{
		if (isServer && _genericBonus != null && hero != null)
		{
			hero.Status.RemoveStatBonus(_genericBonus);
		}
		_genericBonus = null;
	}

	public StatBonus Star_GetGenericStatBonusOfHero()
	{
		if (!isServer)
		{
			throw new InvalidOperationException("Cannot get stat bonus on clients");
		}
		if (_genericBonus != null)
		{
			return _genericBonus;
		}
		if (hero == null)
		{
			return null;
		}
		_genericBonus = new StatBonus();
		hero.Status.AddStatBonus(_genericBonus);
		return _genericBonus;
	}
}
