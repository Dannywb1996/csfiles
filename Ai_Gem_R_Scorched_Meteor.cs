using UnityEngine;

public class Ai_Gem_R_Scorched_Meteor : InstantDamageInstance
{
	public Transform fallTransform;

	public float fallSpeed;

	protected override void OnCreate()
	{
		mainEffect.transform.rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
		base.OnCreate();
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		fallTransform.position += fallTransform.forward * (fallSpeed * Time.deltaTime);
	}

	private void MirrorProcessed()
	{
	}
}
