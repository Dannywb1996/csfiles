using UnityEngine;

public static class ParticleSystemExtensions
{
	public static void TintMainColor(this ParticleSystem ps, Color color)
	{
		ParticleSystem.MainModule m = ps.main;
		ParticleSystem.MinMaxGradient sc = m.startColor;
		sc.color *= color;
		m.startColor = sc;
	}

	public static void SetMainColor(this ParticleSystem ps, Color color)
	{
		ParticleSystem.MainModule m = ps.main;
		ParticleSystem.MinMaxGradient sc = m.startColor;
		sc.color = color;
		m.startColor = sc;
	}

	public static Color GetMainColor(this ParticleSystem ps)
	{
		return ps.main.startColor.color;
	}
}
