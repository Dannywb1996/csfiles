using System.Collections.Generic;
using UnityEngine;

public class Ai_Mon_DarkCave_CaveBat_Dash : AbilityInstance
{
	public DewCollider range;

	public ScalingValue damage;

	public Dash dash;

	public GameObject hitEffect;

	private readonly List<Entity> _affected;

	protected override void OnCreate()
	{
	}

	protected override void ActiveFrameUpdate()
	{
	}

	protected override void ActiveLogicUpdate(float dt)
	{
	}

	private void MirrorProcessed()
	{
	}
}
