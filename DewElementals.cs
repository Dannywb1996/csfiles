using System;

public static class DewElementals
{
	public const float FireDamageInterval = 0.25f;

	public const float FireDamageBase = 3.5f;

	public const float FireDamageMultiplierPerStack = 1.3f;

	public const float ColdSlowPerStack = 35f;

	public const float ColdCripplePerStack = 25f;

	public const float LightDamageAmpPerStack = 0.05f;

	public const float DarkDamageAmpPerStack = 0.05f;

	public const float TemperatureChangeDamage = 50f;

	internal static Type GetSeType(ElementalType type)
	{
		return type switch
		{
			ElementalType.Fire => typeof(Se_Elm_Fire), 
			ElementalType.Cold => typeof(Se_Elm_Cold), 
			ElementalType.Light => typeof(Se_Elm_Light), 
			ElementalType.Dark => typeof(Se_Elm_Dark), 
			_ => throw new Exception($"Unknown ElementalType: {type}"), 
		};
	}
}
