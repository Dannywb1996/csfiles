using Mirror;
using UnityEngine;

public class Se_Elm_Cold : ElementalStatusEffect
{
	public float heroCCMultiplier;

	public float bossCCMultiplier;

	private SlowEffect _slow;

	private CrippleEffect _cripple;

	private float _knownAmp;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			float mult = 1f;
			if (base.victim is Hero)
			{
				mult = heroCCMultiplier;
			}
			else if (base.victim.IsAnyBoss())
			{
				mult = bossCCMultiplier;
			}
			_slow = DoSlow((float)base.stack * 35f * mult);
			_cripple = DoCripple((float)base.stack * 25f * mult);
			base.victim.Status.hasCold = true;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer && !(base.victim == null) && base.victim.Status.isAlive)
		{
			base.victim.Status.hasCold = false;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (base.isServer && base.info.caster != null && ampAmount != _knownAmp)
		{
			UpdateEffectStrength();
		}
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
		base.OnStackChange(oldStack, newStack);
		if (base.isServer)
		{
			UpdateEffectStrength();
		}
	}

	[Server]
	private void UpdateEffectStrength()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Se_Elm_Cold::UpdateEffectStrength()' called when server was not active");
			return;
		}
		_knownAmp = GetColdEffectAmp();
		_slow.strength = (float)base.stack * 35f * (1f + GetColdEffectAmp());
		_cripple.strength = (float)base.stack * 25f * (1f + GetColdEffectAmp());
	}

	private float GetColdEffectAmp()
	{
		return ampAmount;
	}

	private void MirrorProcessed()
	{
	}
}
