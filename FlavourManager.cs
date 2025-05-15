using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlavourManager : NetworkedManagerBase<FlavourManager>, ISettingsChangedCallback
{
	private class Ad_DamageShake
	{
		public bool isStale;

		public EntityTransformModifier modifier;

		public float intensity;
	}

	public Vector2 shakeDamageRatioRange;

	public AnimationCurve shakeIntensity;

	public AnimationCurve shakeDuration;

	public float onStunDurationMultiplier;

	public float shakeInterval;

	public float deviateAngle;

	public GameObject hitStopDealDamage;

	public float hitTimeStopCooldownTime = 10f;

	public bool disableOnLesser;

	public Vector2 damageRatioRange;

	public AnimationCurve chanceByRatio;

	public float killingBlowDamageMultiplier;

	public float minDamageAmount;

	public float everyoneVicinityRadius;

	public bool ignoreAssists;

	public GameObject killFeedback;

	public GameObject killFeedbackLesser;

	public GameObject killFeedbackMiniBoss;

	public GameObject killFeedbackBoss;

	public FxVolume[] screenKillEffects;

	private float _lastHitStopTime = float.NegativeInfinity;

	private List<float> _originalStrengths = new List<float>();

	protected override void Awake()
	{
		base.Awake();
		FxVolume[] array = screenKillEffects;
		foreach (FxVolume v in array)
		{
			_originalStrengths.Add(v.targetStrength);
		}
	}

	private void UpdateScreenKillEffectsStrength()
	{
		if (_originalStrengths.Count > 0)
		{
			for (int i = 0; i < screenKillEffects.Length; i++)
			{
				screenKillEffects[i].targetStrength = _originalStrengths[i] * DewSave.profile.gameplay.killScreenEffectsStrength;
			}
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		GameManager.CallOnReady(delegate
		{
			UpdateScreenKillEffectsStrength();
			NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += (Action<EventInfoDamage>)delegate(EventInfoDamage info)
			{
				if (!(info.actor is ElementalStatusEffect))
				{
					StartCoroutine(Routine());
				}
				IEnumerator Routine()
				{
					float normalized = (info.damage.amount + info.damage.discardedAmount) / (info.victim.maxHealth + info.victim.Status.currentShield);
					normalized = (normalized - shakeDamageRatioRange.x) / (shakeDamageRatioRange.y - shakeDamageRatioRange.x);
					float intensity = shakeIntensity.Evaluate(normalized);
					float duration = shakeDuration.Evaluate(normalized);
					Ad_DamageShake dmg = null;
					if (!info.victim.TryGetData<Ad_DamageShake>(out dmg))
					{
						dmg = new Ad_DamageShake
						{
							intensity = intensity,
							isStale = false,
							modifier = info.victim.Visual.GetNewTransformModifier()
						};
						info.victim.AddData(dmg);
					}
					else
					{
						if (!(dmg.intensity < intensity))
						{
							yield break;
						}
						dmg.isStale = true;
						dmg.modifier.Stop();
						info.victim.RemoveData<Ad_DamageShake>();
						dmg = new Ad_DamageShake
						{
							intensity = intensity,
							isStale = false,
							modifier = info.victim.Visual.GetNewTransformModifier()
						};
						info.victim.AddData(dmg);
					}
					float remaining = duration;
					int sign = 1;
					while (remaining > 0f)
					{
						if (dmg.isStale || info.victim == null)
						{
							yield break;
						}
						Vector3 direction = ((!info.damage.direction.HasValue) ? UnityEngine.Random.insideUnitCircle.normalized.ToXZ() : (Quaternion.Euler(0f, UnityEngine.Random.Range(0f - deviateAngle, deviateAngle), 0f) * info.damage.direction.Value));
						sign *= -1;
						dmg.modifier.worldOffset = direction * ((float)sign * (remaining / duration) * dmg.intensity);
						remaining -= ((info.victim.Status.hasStun || info.victim.Control.isAirborne) ? (shakeInterval / onStunDurationMultiplier) : shakeInterval);
						yield return new WaitForSeconds(shakeInterval);
					}
					if (info.victim != null)
					{
						info.victim.RemoveData<Ad_DamageShake>();
						dmg.modifier.Stop();
					}
				}
			};
			if (ignoreAssists)
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnDeath += (Action<EventInfoKill>)delegate(EventInfoKill info)
				{
					if (info.victim is Monster m && !(info.actor == null))
					{
						Hero hero = info.actor.FindFirstOfType<Hero>();
						if (!(hero == null) && hero.isOwned)
						{
							PlayKillFeedback(m);
						}
					}
				};
			}
			else
			{
				DewPlayer.local.hero.ClientHeroEvent_OnKillOrAssist += (Action<EventInfoKill>)delegate(EventInfoKill info)
				{
					if (info.victim is Monster m)
					{
						PlayKillFeedback(m);
					}
				};
			}
		});
	}

	private void PlayKillFeedback(Monster m)
	{
		switch (m.type)
		{
		case Monster.MonsterType.Lesser:
			FxPlayNew(killFeedbackLesser, m);
			break;
		case Monster.MonsterType.Normal:
		case Monster.MonsterType.MiniBoss:
			FxPlayNew(killFeedback, m);
			break;
		case Monster.MonsterType.Boss:
			break;
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		NetworkedManagerBase<ActorManager>.instance.onEntityAdd += new Action<Entity>(OnEntityAdd);
		_lastHitStopTime = Time.unscaledTime + hitTimeStopCooldownTime;
	}

	private void OnEntityAdd(Entity obj)
	{
		Monster m = obj as Monster;
		if ((object)m == null || (disableOnLesser && m.type == Monster.MonsterType.Lesser))
		{
			return;
		}
		m.EntityEvent_OnTakeDamage += (Action<EventInfoDamage>)delegate(EventInfoDamage info)
		{
			if (!m.Status.hasMirageSkin && !(Time.unscaledTime - _lastHitStopTime < hitTimeStopCooldownTime) && !(info.damage.amount + info.damage.discardedAmount < minDamageAmount))
			{
				float num = (info.damage.amount + info.damage.discardedAmount) / m.maxHealth;
				if (m.currentHealth < 0.001f)
				{
					num *= killingBlowDamageMultiplier;
				}
				num = (num - damageRatioRange.x) / (damageRatioRange.y - damageRatioRange.x);
				if (!(num < 0f))
				{
					float num2 = chanceByRatio.Evaluate(num);
					if (!(UnityEngine.Random.value > num2) && !(info.actor.FindFirstOfType<Hero>() == null))
					{
						foreach (DewPlayer current in DewPlayer.humanPlayers)
						{
							if (!current.hero.IsNullInactiveDeadOrKnockedOut() && Vector2.Distance(m.position.ToXY(), current.hero.position.ToXY()) > everyoneVicinityRadius)
							{
								return;
							}
						}
						FxPlayNewNetworked(hitStopDealDamage, m);
						_lastHitStopTime = Time.unscaledTime;
					}
				}
			}
		};
		m.EntityEvent_OnDeath += (Action<EventInfoKill>)delegate
		{
			if (m.type == Monster.MonsterType.MiniBoss)
			{
				FxPlayNewNetworked(killFeedbackMiniBoss, m);
			}
			if (m.type == Monster.MonsterType.Boss)
			{
				FxPlayNewNetworked(killFeedbackBoss, m);
			}
		};
	}

	public void OnSettingsChanged()
	{
		UpdateScreenKillEffectsStrength();
	}

	private void MirrorProcessed()
	{
	}
}
