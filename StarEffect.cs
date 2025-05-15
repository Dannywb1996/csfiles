using System;
using UnityEngine;

public class StarEffect : StatusEffect
{
	public StarType type;

	public int requiredLevel;

	public Sprite starIcon;

	public int maxStarLevel = 1;

	public int[] price = new int[1];

	public virtual Type heroType => null;

	public virtual Type skillType => null;

	public virtual bool isMovementSkillType => false;

	public Hero hero => base.victim as Hero;

	public DewPlayer player
	{
		get
		{
			if (!(base.victim == null))
			{
				return base.victim.owner;
			}
			return null;
		}
	}

	private void MirrorProcessed()
	{
	}
}
