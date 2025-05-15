using UnityEngine;

public class PropEnt_Stone_Nightmare : PropEntity
{
	public float rotSpeed;

	private EntityTransformModifier _entTransform;

	private float _angle;

	public override bool isRegularReward => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		_entTransform = base.Visual.GetNewTransformModifier();
		FxPlay(base.gameObject, this);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_entTransform != null)
		{
			_entTransform.Stop();
			_entTransform = null;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (_entTransform != null)
		{
			_angle += rotSpeed * dt;
			_entTransform.rotation = Quaternion.Euler(0f, _angle, 0f);
		}
	}

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
	}

	private void MirrorProcessed()
	{
	}
}
