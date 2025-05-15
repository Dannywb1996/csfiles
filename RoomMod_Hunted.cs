using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class RoomMod_Hunted : RoomModifierBase
{
	[Header("General")]
	public Formula addedMirageChance;

	public Formula addedHunterChance;

	public Formula spawnedPopMultiplier;

	public Formula hunterDamageAmp;

	public Formula hunterHealthAmp;

	public Formula hunterSizeMultiplier;

	public Formula smartMonsterRatio;

	[Header("Artillery")]
	public bool enableArtillery = true;

	public bool endWhenRiftOpen;

	public Vector2 initDelay;

	public Formula intervalMinByHuntLevel;

	public Formula intervalMaxByHuntLevel;

	public Formula intervalMultiplierByNormalizedHealth;

	public float bigArtilleryRandomMag;

	public Formula bigArtilleryChanceByHuntLevel;

	public AnimationCurve bigArtilleryChanceMultiplierByArea;

	public Vector2Int smallArtilleryCount;

	public Formula smallArtilleryCountMultiplierByHuntLevel;

	public Vector2 smallArtilleryInterval;

	public float smallArtilleryRandomMag;

	public AnimationCurve smallArtilleryCountMultiplierByArea;

	public Formula artilleryStrengthByPlayers;

	public Formula artilleryStrengthByHuntLevel;

	public Formula artillerySpeedMultiplierByHuntLevel;

	private float _addedMirageChance;

	private float _addedHunterChance;

	private float _spawnedPopMultiplier;

	public override void OnStartServer()
	{
		base.OnStartServer();
		_addedMirageChance = addedMirageChance.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel);
		_addedHunterChance = addedHunterChance.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel);
		_spawnedPopMultiplier = spawnedPopMultiplier.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel);
		SingletonDewNetworkBehaviour<Room>.instance.monsters.addedMirageChance += _addedMirageChance;
		SingletonDewNetworkBehaviour<Room>.instance.monsters.addedHunterChance += _addedHunterChance;
		SingletonDewNetworkBehaviour<Room>.instance.monsters.spawnedPopMultiplier *= _spawnedPopMultiplier;
		RoomMonsters monsters = SingletonDewNetworkBehaviour<Room>.instance.monsters;
		monsters.onAfterSpawn = (Action<Entity>)Delegate.Combine(monsters.onAfterSpawn, new Action<Entity>(OnAfterSpawn));
		if (enableArtillery)
		{
			GameManager.CallOnReady(delegate
			{
				StartCoroutine(SmallArtilleryRoutine());
				StartCoroutine(BigArtilleryRoutine());
			});
		}
	}

	private void OnAfterSpawn(Entity obj)
	{
		if (obj.Status.HasStatusEffect<Se_HunterBuff>())
		{
			ApplyHunterStatBonusAndAIPrediction(obj, NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel);
		}
	}

	public void ApplyHunterStatBonusAndAIPrediction(Entity entity, int level)
	{
		float hpAmp = hunterHealthAmp.Evaluate(level);
		float dmgAmp = hunterDamageAmp.Evaluate(level);
		float sizeMult = hunterSizeMultiplier.Evaluate(level);
		entity.Status.AddStatBonus(new StatBonus
		{
			maxHealthPercentage = hpAmp * 100f,
			abilityPowerPercentage = dmgAmp * 100f,
			attackDamagePercentage = dmgAmp * 100f
		});
		entity.Visual.GetNewTransformModifier().scaleMultiplier = Vector3.one * sizeMult;
		entity.Control.outerRadius *= sizeMult;
		if (UnityEngine.Random.value < smartMonsterRatio.Evaluate(level))
		{
			entity.AI.predictionStrengthOverride = () => UnityEngine.Random.value;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer)
		{
			StopAllCoroutines();
			if (!(SingletonDewNetworkBehaviour<Room>.instance == null))
			{
				SingletonDewNetworkBehaviour<Room>.instance.monsters.addedMirageChance -= _addedMirageChance;
				SingletonDewNetworkBehaviour<Room>.instance.monsters.addedHunterChance -= _addedHunterChance;
				SingletonDewNetworkBehaviour<Room>.instance.monsters.spawnedPopMultiplier /= _spawnedPopMultiplier;
				RoomMonsters monsters = SingletonDewNetworkBehaviour<Room>.instance.monsters;
				monsters.onAfterSpawn = (Action<Entity>)Delegate.Remove(monsters.onAfterSpawn, new Action<Entity>(OnAfterSpawn));
			}
		}
	}

	private IEnumerator SmallArtilleryRoutine()
	{
		int level = NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel;
		float intervalMin = intervalMinByHuntLevel.Evaluate(level);
		float intervalMax = intervalMaxByHuntLevel.Evaluate(level);
		float countMultiplier = smallArtilleryCountMultiplierByHuntLevel.Evaluate(level) * smallArtilleryCountMultiplierByArea.Evaluate(SingletonDewNetworkBehaviour<Room>.instance.map.mapData.area);
		float speed = artillerySpeedMultiplierByHuntLevel.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel);
		yield return new WaitForSeconds(UnityEngine.Random.Range(initDelay.x, initDelay.y));
		while (true)
		{
			Hero hero = Dew.SelectRandomAliveHero();
			if (hero == null)
			{
				break;
			}
			int count = DewMath.RandomRoundToInt((float)UnityEngine.Random.Range(smallArtilleryCount.x, smallArtilleryCount.y) * countMultiplier);
			float strength = artilleryStrengthByPlayers.Evaluate(DewPlayer.humanPlayers.Count((DewPlayer p) => !p.hero.IsNullInactiveDeadOrKnockedOut())) * artilleryStrengthByHuntLevel.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel);
			for (int i = 0; i < count; i++)
			{
				if (endWhenRiftOpen && Rift.instance.isOpen && !Rift.instance.isLocked)
				{
					yield break;
				}
				Vector3 pos = AbilityTrigger.PredictPoint_Simple(UnityEngine.Random.value, hero, 1f) + UnityEngine.Random.insideUnitSphere.Flattened() * smallArtilleryRandomMag;
				pos = Dew.GetValidAgentDestination_Closest(hero.agentPosition, pos);
				CreateAbilityInstance(pos, null, default(CastInfo), delegate(Ai_HunterArtillery_Small ai)
				{
					ai.NetworkstrengthMultiplier = strength;
					ai.NetworkspeedMultiplier = speed;
				});
				yield return new WaitForSeconds(UnityEngine.Random.Range(smallArtilleryInterval.x, smallArtilleryInterval.y));
			}
			float multiplier = intervalMultiplierByNormalizedHealth.Evaluate(hero.normalizedHealth);
			yield return new WaitForSeconds(UnityEngine.Random.Range(intervalMin, intervalMax) * multiplier);
		}
	}

	private IEnumerator BigArtilleryRoutine()
	{
		int level = NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel;
		float intervalMin = intervalMinByHuntLevel.Evaluate(level);
		float intervalMax = intervalMaxByHuntLevel.Evaluate(level);
		float bigChance = bigArtilleryChanceByHuntLevel.Evaluate(level) * bigArtilleryChanceMultiplierByArea.Evaluate(SingletonDewNetworkBehaviour<Room>.instance.map.mapData.area);
		float speed = artillerySpeedMultiplierByHuntLevel.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel);
		yield return new WaitForSeconds(UnityEngine.Random.Range(initDelay.x, initDelay.y));
		while (true)
		{
			Hero hero = Dew.SelectRandomAliveHero();
			if (hero == null || (endWhenRiftOpen && Rift.instance.isOpen && !Rift.instance.isLocked))
			{
				break;
			}
			if (UnityEngine.Random.value < bigChance)
			{
				Vector3 pos = hero.agentPosition + UnityEngine.Random.insideUnitSphere.Flattened() * bigArtilleryRandomMag;
				pos = Dew.GetValidAgentDestination_Closest(hero.agentPosition, pos);
				float strength = artilleryStrengthByPlayers.Evaluate(DewPlayer.humanPlayers.Count((DewPlayer p) => !p.hero.IsNullInactiveDeadOrKnockedOut())) * artilleryStrengthByHuntLevel.Evaluate(NetworkedManagerBase<ZoneManager>.instance.currentHuntLevel);
				CreateAbilityInstance(pos, null, default(CastInfo), delegate(Ai_HunterArtillery_Big ai)
				{
					ai.NetworkstrengthMultiplier = strength;
					ai.NetworkspeedMultiplier = speed;
				});
			}
			float multiplier = intervalMultiplierByNormalizedHealth.Evaluate(hero.normalizedHealth);
			yield return new WaitForSeconds(UnityEngine.Random.Range(intervalMin, intervalMax) * multiplier);
		}
	}

	private void MirrorProcessed()
	{
	}
}
