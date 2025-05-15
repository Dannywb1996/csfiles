using UnityEngine;

public interface ISpawnableAsMiniBoss
{
	static void GiveGenericMiniBossBonus(Entity entity)
	{
		float sizeMultiplier = 1.3f;
		entity.Visual.GetNewTransformModifier().scaleMultiplier = Vector3.one * sizeMultiplier;
		entity.Control.outerRadius *= sizeMultiplier;
		entity.Control.innerRadius *= sizeMultiplier;
		StatBonus bonus = new StatBonus
		{
			maxHealthFlat = 850f,
			abilityHasteFlat = 50f,
			attackSpeedPercentage = 15f,
			abilityPowerPercentage = 20f,
			attackDamagePercentage = 20f
		};
		entity.Status.AddStatBonus(bonus);
		foreach (AbilityTrigger value in entity.Ability.abilities.Values)
		{
			TriggerConfig[] configs = value.configs;
			for (int i = 0; i < configs.Length; i++)
			{
				configs[i].postDelay *= 0.5f;
			}
		}
		entity.CreateBasicEffect(entity, new UnstoppableEffect(), float.PositiveInfinity, "MiniBossUnstoppable");
	}

	void OnBeforeSpawnAsMiniBoss();

	void OnCreateAsMiniBoss();
}
