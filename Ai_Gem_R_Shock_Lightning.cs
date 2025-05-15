using System;
using System.Collections.Generic;
using UnityEngine;

public class Ai_Gem_R_Shock_Lightning : StandardProjectile
{
	public float chainDelay;

	public DewCollider chainRange;

	public ScalingValue chainDamage;

	public float firstProcCoefficient;

	public float chainProcCoefficient;

	public bool canChainToSelf;

	public GameObject firstEffect;

	[NonSerialized]
	public int maxHitCount;

	private Dictionary<Entity, int> _affectedEntities;

	private int _chainedCount;

	protected override void OnCreate()
	{
	}

	protected override void OnComplete()
	{
	}

	private void MirrorProcessed()
	{
	}
}
