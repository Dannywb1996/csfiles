using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FeedbackManager : ManagerBase<FeedbackManager>
{
	[Serializable]
	public class EffectPool
	{
		public GameObject effect;

		public int numOfSources = 2;

		private GameObject[] _effects;

		private int _cursor;

		public void Init(Transform parent)
		{
			_effects = new GameObject[numOfSources];
			for (int i = 0; i < numOfSources; i++)
			{
				_effects[i] = UnityEngine.Object.Instantiate(effect, parent);
				_effects[i].name = effect.name + " " + i;
			}
		}

		public void Play()
		{
			DewEffect.Play(_effects[_cursor]);
			_cursor++;
			if (_cursor == numOfSources)
			{
				_cursor = 0;
			}
		}
	}

	public Transform genericFeedbacksParent;

	public Volume slowTimeBySpecialMenuVolume;

	public float slowTimeBySpecialMenuWeightSpeed = 10f;

	public GameObject fxGainStardustOnHero;

	public EffectPool dealDmg;

	public EffectPool dealDmgCrit;

	public EffectPool dealDmgDot;

	public EffectPool dealDmgDotCrit;

	public EffectPool takeDmg;

	public EffectPool takeDmgDot;

	public EffectPool healNormal;

	public EffectPool healCrit;

	private Dictionary<string, GameObject> _feedbackEffects = new Dictionary<string, GameObject>();

	protected override void Awake()
	{
		base.Awake();
		dealDmg.Init(base.transform);
		dealDmgCrit.Init(base.transform);
		dealDmgDot.Init(base.transform);
		dealDmgDotCrit.Init(base.transform);
		takeDmg.Init(base.transform);
		takeDmgDot.Init(base.transform);
		healNormal.Init(base.transform);
		healCrit.Init(base.transform);
		for (int i = 0; i < genericFeedbacksParent.childCount; i++)
		{
			Transform c = genericFeedbacksParent.GetChild(i);
			_feedbackEffects.Add(c.name, c.gameObject);
		}
	}

	private void Start()
	{
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeDamage += (Action<EventInfoDamage>)delegate(EventInfoDamage info)
		{
			if (!(ManagerBase<CameraManager>.instance.focusedEntity == null))
			{
				if (info.victim == ManagerBase<CameraManager>.instance.focusedEntity)
				{
					if (info.damage.HasAttr(DamageAttribute.DamageOverTime))
					{
						takeDmgDot.Play();
					}
					else
					{
						takeDmg.Play();
					}
				}
				else if (info.actor != null && (info.actor == ManagerBase<CameraManager>.instance.focusedEntity || info.actor.IsDescendantOf(ManagerBase<CameraManager>.instance.focusedEntity)))
				{
					if (info.damage.HasAttr(DamageAttribute.DamageOverTime))
					{
						if (info.damage.HasAttr(DamageAttribute.IsCrit))
						{
							dealDmgDotCrit.Play();
						}
						else
						{
							dealDmgDot.Play();
						}
					}
					else if (info.damage.HasAttr(DamageAttribute.IsCrit))
					{
						dealDmgCrit.Play();
					}
					else
					{
						dealDmg.Play();
					}
				}
			}
		};
		NetworkedManagerBase<ClientEventManager>.instance.OnTakeHeal += (Action<EventInfoHeal>)delegate(EventInfoHeal info)
		{
			if (!(ManagerBase<CameraManager>.instance.focusedEntity == null) && !(ManagerBase<CameraManager>.instance.focusedEntity.owner == null) && info.actor != null && (info.actor == ManagerBase<CameraManager>.instance.focusedEntity || info.actor.IsDescendantOf(ManagerBase<CameraManager>.instance.focusedEntity)))
			{
				if (info.isCrit)
				{
					healCrit.Play();
				}
				else
				{
					healNormal.Play();
				}
			}
		};
		GameManager.CallOnReady(delegate
		{
			DewPlayer.local.hero.ClientHeroEvent_OnKnockedOut += (Action<EventInfoKill>)delegate
			{
				PlayFeedbackEffect("UI_Game_LocalHeroKnockedOut");
			};
			foreach (DewPlayer h in DewPlayer.humanPlayers)
			{
				h.ClientEvent_OnEarnStardust += (Action<int>)delegate
				{
					DewEffect.PlayNew(fxGainStardustOnHero, h.hero);
					if (h.isLocalPlayer)
					{
						PlayFeedbackEffect("UI_Game_LocalPlayerGainStardust");
					}
				};
			}
		});
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		slowTimeBySpecialMenuVolume.weight = Mathf.MoveTowards(slowTimeBySpecialMenuVolume.weight, NetworkedManagerBase<TimescaleManager>.instance.shouldTimeBeSlowedBySpecialMenu ? 1 : 0, Time.unscaledDeltaTime * slowTimeBySpecialMenuWeightSpeed);
	}

	public void PlayFeedbackEffect(string feedbackName)
	{
		if (_feedbackEffects.TryGetValue(feedbackName, out var eff))
		{
			DewEffect.Play(eff);
		}
		else
		{
			Debug.LogWarning("Feedback of name '" + feedbackName + "' not found");
		}
	}

	public void StopFeedbackEffect(string feedbackName)
	{
		if (_feedbackEffects.TryGetValue(feedbackName, out var eff))
		{
			DewEffect.Stop(eff);
		}
		else
		{
			Debug.LogWarning("Feedback of name '" + feedbackName + "' not found");
		}
	}
}
