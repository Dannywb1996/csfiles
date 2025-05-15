using UnityEngine;

public class PropEnt_Stone_DreamDust : PropEntity
{
	public Formula amountByZoneIndex;

	public float rotSpeed;

	private EntityTransformModifier _entTransform;

	private float _angle;

	public override bool isRegularReward => true;

	protected override void OnCreate()
	{
		base.OnCreate();
		_entTransform = base.Visual.GetNewTransformModifier();
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
		if (base.isServer)
		{
			float floatAmount = amountByZoneIndex.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex);
			NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: false, DewMath.RandomRoundToInt(floatAmount), base.transform.position);
		}
	}

	private void MirrorProcessed()
	{
	}
}
