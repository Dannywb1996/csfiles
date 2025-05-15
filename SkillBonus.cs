using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class SkillBonus
{
	public bool ignoreReceiveCooldownReductionFlag;

	[SerializeField]
	internal float _cooldownMultiplier = 1f;

	[SerializeField]
	internal int _addedCharge;

	[FormerlySerializedAs("_addedCooldown")]
	[SerializeField]
	internal float _cooldownOffset;

	internal SkillTrigger _parent;

	public float cooldownMultiplier
	{
		get
		{
			return _cooldownMultiplier;
		}
		set
		{
			_cooldownMultiplier = value;
			if (_parent != null)
			{
				_parent._isSkillBonusDirty = true;
			}
		}
	}

	public int addedCharge
	{
		get
		{
			return _addedCharge;
		}
		set
		{
			_addedCharge = value;
			if (_parent != null)
			{
				_parent._isSkillBonusDirty = true;
			}
		}
	}

	public float cooldownOffset
	{
		get
		{
			return _cooldownOffset;
		}
		set
		{
			_cooldownOffset = value;
			if (_parent != null)
			{
				_parent._isSkillBonusDirty = true;
			}
		}
	}

	public void Stop()
	{
		if (!(_parent == null))
		{
			_parent.RemoveSkillBonus(this);
		}
	}
}
