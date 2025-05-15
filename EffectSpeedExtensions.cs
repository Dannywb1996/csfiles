using UnityEngine;

public static class EffectSpeedExtensions
{
	public static void ApplySpeedMultiplier(this GameObject o, float multiplier)
	{
		if (o == null)
		{
			return;
		}
		ListReturnHandle<ParticleSystem> h0;
		foreach (ParticleSystem item in o.GetComponentsInChildrenNonAlloc(out h0))
		{
			ParticleSystem.MainModule m = item.main;
			m.simulationSpeed *= multiplier;
		}
		h0.Return();
		ListReturnHandle<IEffectWithSpeed> h1;
		foreach (IEffectWithSpeed item2 in o.GetComponentsInChildrenNonAlloc(out h1))
		{
			item2.ApplySpeedMultiplier(multiplier);
		}
		h1.Return();
	}
}
