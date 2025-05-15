using System;
using System.Globalization;
using UnityEngine;

[Serializable]
public struct ScalingValue : ISerializationCallbackReceiver
{
	public const float SkillDefaultPerLevelMultiplier = 1.25f;

	public const float GemDefaultPerLevelMultiplier = 1.01f;

	public static int? levelOverride;

	public string valueString;

	public LevelScaling leveling;

	public float scalingMultiplier;

	private bool _isParsed;

	private float _baseValue;

	private float _adFactor;

	private float _apFactor;

	private float _lvlFactor;

	public string scaleHelpText => $"{GetAddedScalingMultiplierPerLevel():0.#%}/level";

	public float baseValue
	{
		get
		{
			if (!_isParsed)
			{
				ParseValueString();
			}
			return _baseValue;
		}
	}

	public float adFactor
	{
		get
		{
			if (!_isParsed)
			{
				ParseValueString();
			}
			return _adFactor;
		}
	}

	public float apFactor
	{
		get
		{
			if (!_isParsed)
			{
				ParseValueString();
			}
			return _apFactor;
		}
	}

	public float lvlFactor
	{
		get
		{
			if (!_isParsed)
			{
				ParseValueString();
			}
			return _lvlFactor;
		}
	}

	public float perLevelMultiplier
	{
		get
		{
			if (leveling == LevelScaling.GemDefault)
			{
				return 1.01f;
			}
			if (leveling == LevelScaling.SkillDefault)
			{
				return 1.25f;
			}
			return 1f;
		}
	}

	public static implicit operator ScalingValue(string str)
	{
		return new ScalingValue
		{
			valueString = str
		};
	}

	public static explicit operator ScalingValue(float val)
	{
		return new ScalingValue
		{
			valueString = val.ToString(CultureInfo.InvariantCulture)
		};
	}

	public static ScalingValue Lerp(ScalingValue a, ScalingValue b, float t)
	{
		LevelScaling leveling = a.leveling;
		float scalingMultiplier = Mathf.Lerp(a.scalingMultiplier, b.scalingMultiplier, t);
		float baseValue = Mathf.Lerp(a.baseValue, b.baseValue, t);
		float apFactor = Mathf.Lerp(a.apFactor, b.apFactor, t);
		float adFactor = Mathf.Lerp(a.adFactor, b.adFactor, t);
		float lvlFactor = Mathf.Lerp(a.lvlFactor, b.lvlFactor, t);
		ScalingValue newVal = new ScalingValue(baseValue, adFactor, apFactor, lvlFactor, leveling);
		newVal.scalingMultiplier = scalingMultiplier;
		return newVal;
	}

	public ScalingValue(float baseValue, float adFactor, float apFactor, float lvlFactor)
	{
		this = default(ScalingValue);
		valueString = GetValueString(baseValue, adFactor, apFactor, lvlFactor);
		scalingMultiplier = 1f;
	}

	public ScalingValue(float baseValue, float adFactor, float apFactor, float lvlFactor, LevelScaling scaling)
	{
		this = default(ScalingValue);
		valueString = GetValueString(baseValue, adFactor, apFactor, lvlFactor);
		leveling = scaling;
		scalingMultiplier = 1f;
	}

	private void ParseValueString()
	{
		_baseValue = float.NaN;
		_adFactor = float.NaN;
		_apFactor = float.NaN;
		_lvlFactor = float.NaN;
		string[] array = valueString.Split(' ');
		foreach (string val in array)
		{
			if (val.EndsWith("ad"))
			{
				SetIfNaNOrThrow(ref _adFactor, float.Parse(val.Substring(0, val.Length - 2), CultureInfo.InvariantCulture));
			}
			else if (val.EndsWith("ap"))
			{
				SetIfNaNOrThrow(ref _apFactor, float.Parse(val.Substring(0, val.Length - 2), CultureInfo.InvariantCulture));
			}
			else if (val.EndsWith("x"))
			{
				SetIfNaNOrThrow(ref _lvlFactor, float.Parse(val.Substring(0, val.Length - 1), CultureInfo.InvariantCulture));
			}
			else
			{
				SetIfNaNOrThrow(ref _baseValue, float.Parse(val, CultureInfo.InvariantCulture));
			}
		}
		if (float.IsNaN(_baseValue))
		{
			_baseValue = 0f;
		}
		if (float.IsNaN(_adFactor))
		{
			_adFactor = 0f;
		}
		if (float.IsNaN(_apFactor))
		{
			_apFactor = 0f;
		}
		if (float.IsNaN(_lvlFactor))
		{
			_lvlFactor = 0f;
		}
		_isParsed = true;
		static void SetIfNaNOrThrow(ref float destination, float setValue)
		{
			if (!float.IsNaN(destination))
			{
				throw new Exception("Duplicate Term");
			}
			destination = setValue;
		}
	}

	private static string GetValueString(float baseValue, float adFactor, float apFactor, float lvlFactor)
	{
		string valStr = "";
		if (baseValue != 0f)
		{
			valStr = valStr + baseValue.ToString(CultureInfo.InvariantCulture) + " ";
		}
		if (adFactor != 0f)
		{
			valStr = valStr + adFactor.ToString(CultureInfo.InvariantCulture) + "ad ";
		}
		if (apFactor != 0f)
		{
			valStr = valStr + apFactor.ToString(CultureInfo.InvariantCulture) + "ap ";
		}
		if (lvlFactor != 0f)
		{
			valStr = valStr + lvlFactor.ToString(CultureInfo.InvariantCulture) + "x";
		}
		if (valStr.Length == 0)
		{
			return "0";
		}
		return valStr.Trim();
	}

	public override string ToString()
	{
		return valueString ?? "";
	}

	public float GetValue(int level, Entity self)
	{
		if (self == null)
		{
			return GetValue(level);
		}
		return GetValue(level, self.Status.attackDamage, self.Status.abilityPower);
	}

	public float GetAddedScalingMultiplierPerLevel()
	{
		if (leveling == LevelScaling.GemDefault)
		{
			return 0.00999999f * scalingMultiplier;
		}
		if (leveling == LevelScaling.SkillDefault)
		{
			return 0.25f * scalingMultiplier;
		}
		return 0f;
	}

	public float GetScalingMultiplier(int level)
	{
		return 1f + GetAddedScalingMultiplierPerLevel() * (float)Mathf.Max(0, level - 1);
	}

	public float GetValue(int level, float attackDamage = 0f, float abilityPower = 0f)
	{
		if (levelOverride.HasValue)
		{
			level = levelOverride.Value;
		}
		if (!_isParsed)
		{
			ParseValueString();
		}
		float multiplier = 1f;
		level = Mathf.Max(level, 1);
		multiplier = GetScalingMultiplier(level);
		return (_baseValue + _lvlFactor * (float)level + _adFactor * attackDamage + _apFactor * abilityPower) * multiplier;
	}

	public void OnBeforeSerialize()
	{
		if (scalingMultiplier <= 0.0001f)
		{
			scalingMultiplier = 1f;
		}
	}

	public void OnAfterDeserialize()
	{
	}
}
