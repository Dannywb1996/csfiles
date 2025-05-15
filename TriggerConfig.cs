using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class TriggerConfig
{
	public enum StatusEffectVictimType
	{
		Caster,
		Target
	}

	public Sprite triggerIcon;

	public bool isActive = true;

	[SerializeField]
	internal float _manaCost;

	[SerializeField]
	internal int _maxCharges = 1;

	[SerializeField]
	public int startCharges = -1;

	[SerializeField]
	internal int _addedCharges = 1;

	[SerializeField]
	internal float _cooldownTime = 4f;

	[SerializeField]
	internal float _minimumDelay;

	public AbilityInstance spawnedInstance;

	public bool lockCooldownUntilKilled;

	public bool lockCastUntilKilled;

	public bool setFillAmount = true;

	public bool destroyExistingEffect;

	public StatusEffect appliedStatusEffect;

	[FormerlySerializedAs("animation")]
	public DewAnimationClip startAnim;

	public DewAnimationClip endAnim;

	public DewAudioClip castVoice;

	public GameObject effectOnCast;

	public StatusEffectVictimType victim;

	public TriggerChannelData channel;

	public float postDelay;

	public AbilitySelfValidator selfValidator;

	public bool ignoreBlock;

	public bool ignoreAbilityLock;

	public bool faceForward = true;

	public bool overrideRotation;

	public float overrideRotationDuration = 0.75f;

	public bool canReceiveCooldownReduction = true;

	public bool postponeBasicCommand;

	public bool moveTowardsCastDirection;

	public bool canConsumeCastBonus = true;

	public bool alwaysCastImmediately;

	public bool castByMoveDirectionByDefault;

	public bool castByMoveDirectionGamepad;

	public bool ignoreAimDirectionGamepad;

	public bool unstoppableWhileCasting;

	public AbilityTargetValidator targetValidator;

	public CastMethodData castMethod;

	public AbilityTrigger.PredictionSettings predictionSettings = AbilityTrigger.PredictionSettings.Default;

	internal AbilityTrigger _parent;

	public float manaCost
	{
		get
		{
			return _manaCost;
		}
		set
		{
			_manaCost = value;
			_parent._isConfigDirty = true;
		}
	}

	public int maxCharges
	{
		get
		{
			return _maxCharges;
		}
		set
		{
			_maxCharges = value;
			_parent._isConfigDirty = true;
		}
	}

	public int addedCharges
	{
		get
		{
			return _addedCharges;
		}
		set
		{
			_addedCharges = value;
			_parent._isConfigDirty = true;
		}
	}

	public float cooldownTime
	{
		get
		{
			return _cooldownTime;
		}
		set
		{
			_cooldownTime = value;
			_parent._isConfigDirty = true;
		}
	}

	public float minimumDelay
	{
		get
		{
			return _minimumDelay;
		}
		set
		{
			_minimumDelay = value;
			_parent._isConfigDirty = true;
		}
	}

	public float effectiveRange => castMethod.GetEffectiveRange();

	private bool IsStatusEffectNonTargeted()
	{
		if (spawnedInstance is StatusEffect)
		{
			return castMethod.type != CastMethodType.Target;
		}
		return false;
	}

	public bool CheckRange(CastInfo info)
	{
		switch (castMethod.type)
		{
		case CastMethodType.None:
		case CastMethodType.Cone:
		case CastMethodType.Arrow:
			return true;
		case CastMethodType.Target:
			if (info.target == null)
			{
				return true;
			}
			return Vector2.Distance(info.caster.agentPosition.ToXY(), info.target.agentPosition.ToXY()) - info.target.Control.outerRadius < castMethod.targetData.range;
		case CastMethodType.Point:
			if (castMethod._isClamping)
			{
				return true;
			}
			return Vector2.Distance(info.caster.agentPosition.ToXY(), info.point.ToXY()) < castMethod.pointData.range;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public bool CheckRange(Entity caster, Entity target)
	{
		return Vector2.Distance(caster.agentPosition.ToXY(), target.agentPosition.ToXY()) - target.Control.outerRadius <= castMethod.GetEffectiveRange();
	}

	public Vector3 GetMoveToCastDestination(CastInfo info)
	{
		return castMethod.type switch
		{
			CastMethodType.Target => info.target.position, 
			CastMethodType.Point => info.point, 
			_ => default(Vector3), 
		};
	}

	public float GetMoveToCastRequiredDistance(CastInfo info)
	{
		switch (castMethod.type)
		{
		case CastMethodType.Target:
			return castMethod.targetData.range + info.target.Control.outerRadius;
		case CastMethodType.Point:
			if (castMethod._isClamping)
			{
				return float.PositiveInfinity;
			}
			return castMethod.pointData.range;
		default:
			return float.PositiveInfinity;
		}
	}
}
