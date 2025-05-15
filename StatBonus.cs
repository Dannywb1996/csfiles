using System;
using UnityEngine;

[Serializable]
public class StatBonus
{
	[SerializeField]
	internal float _attackDamageFlat;

	[SerializeField]
	internal float _attackDamagePercentage;

	[SerializeField]
	internal float _abilityPowerFlat;

	[SerializeField]
	internal float _abilityPowerPercentage;

	[SerializeField]
	internal float _maxHealthFlat;

	[SerializeField]
	internal float _maxHealthPercentage;

	[SerializeField]
	internal float _maxManaFlat;

	[SerializeField]
	internal float _maxManaPercentage;

	[SerializeField]
	internal float _healthRegenFlat;

	[SerializeField]
	internal float _healthRegenPercentage;

	[SerializeField]
	internal float _manaRegenFlat;

	[SerializeField]
	internal float _manaRegenPercentage;

	[SerializeField]
	internal float _attackSpeedPercentage;

	[SerializeField]
	internal float _critAmpFlat;

	[SerializeField]
	internal float _critAmpPercentage;

	[SerializeField]
	internal float _critChanceFlat;

	[SerializeField]
	internal float _critChancePercentage;

	[SerializeField]
	internal float _abilityHasteFlat;

	[SerializeField]
	internal float _abilityHastePercentage;

	[SerializeField]
	internal float _tenacityFlat;

	[SerializeField]
	internal float _tenacityPercentage;

	[SerializeField]
	internal float _movementSpeedPercentage;

	[SerializeField]
	internal float _fireEffectAmpFlat;

	[SerializeField]
	internal float _coldEffectAmpFlat;

	[SerializeField]
	internal float _lightEffectAmpFlat;

	[SerializeField]
	internal float _darkEffectAmpFlat;

	[SerializeField]
	internal float _armorFlat;

	[SerializeField]
	internal float _armorPercentage;

	internal bool _isDirty = true;

	public float attackDamageFlat
	{
		get
		{
			return _attackDamageFlat;
		}
		set
		{
			if (_attackDamageFlat != value)
			{
				_attackDamageFlat = value;
				_isDirty = true;
			}
		}
	}

	public float attackDamagePercentage
	{
		get
		{
			return _attackDamagePercentage;
		}
		set
		{
			if (_attackDamagePercentage != value)
			{
				_attackDamagePercentage = value;
				_isDirty = true;
			}
		}
	}

	public float abilityPowerFlat
	{
		get
		{
			return _abilityPowerFlat;
		}
		set
		{
			if (_abilityPowerFlat != value)
			{
				_abilityPowerFlat = value;
				_isDirty = true;
			}
		}
	}

	public float abilityPowerPercentage
	{
		get
		{
			return _abilityPowerPercentage;
		}
		set
		{
			if (_abilityPowerPercentage != value)
			{
				_abilityPowerPercentage = value;
				_isDirty = true;
			}
		}
	}

	public float maxHealthFlat
	{
		get
		{
			return _maxHealthFlat;
		}
		set
		{
			if (_maxHealthFlat != value)
			{
				_maxHealthFlat = value;
				_isDirty = true;
			}
		}
	}

	public float maxHealthPercentage
	{
		get
		{
			return _maxHealthPercentage;
		}
		set
		{
			if (_maxHealthPercentage != value)
			{
				_maxHealthPercentage = value;
				_isDirty = true;
			}
		}
	}

	public float maxManaFlat
	{
		get
		{
			return _maxManaFlat;
		}
		set
		{
			if (_maxManaFlat != value)
			{
				_maxManaFlat = value;
				_isDirty = true;
			}
		}
	}

	public float maxManaPercentage
	{
		get
		{
			return _maxManaPercentage;
		}
		set
		{
			if (_maxManaPercentage != value)
			{
				_maxManaPercentage = value;
				_isDirty = true;
			}
		}
	}

	public float healthRegenFlat
	{
		get
		{
			return _healthRegenFlat;
		}
		set
		{
			if (_healthRegenFlat != value)
			{
				_healthRegenFlat = value;
				_isDirty = true;
			}
		}
	}

	public float healthRegenPercentage
	{
		get
		{
			return _healthRegenPercentage;
		}
		set
		{
			if (_healthRegenPercentage != value)
			{
				_healthRegenPercentage = value;
				_isDirty = true;
			}
		}
	}

	public float manaRegenFlat
	{
		get
		{
			return _manaRegenFlat;
		}
		set
		{
			if (_manaRegenFlat != value)
			{
				_manaRegenFlat = value;
				_isDirty = true;
			}
		}
	}

	public float manaRegenPercentage
	{
		get
		{
			return _manaRegenPercentage;
		}
		set
		{
			if (_manaRegenPercentage != value)
			{
				_manaRegenPercentage = value;
				_isDirty = true;
			}
		}
	}

	public float attackSpeedPercentage
	{
		get
		{
			return _attackSpeedPercentage;
		}
		set
		{
			if (_attackSpeedPercentage != value)
			{
				_attackSpeedPercentage = value;
				_isDirty = true;
			}
		}
	}

	public float critAmpFlat
	{
		get
		{
			return _critAmpFlat;
		}
		set
		{
			if (_critAmpFlat != value)
			{
				_critAmpFlat = value;
				_isDirty = true;
			}
		}
	}

	public float critAmpPercentage
	{
		get
		{
			return _critAmpPercentage;
		}
		set
		{
			if (_critAmpPercentage != value)
			{
				_critAmpPercentage = value;
				_isDirty = true;
			}
		}
	}

	public float critChanceFlat
	{
		get
		{
			return _critChanceFlat;
		}
		set
		{
			if (_critChanceFlat != value)
			{
				_critChanceFlat = value;
				_isDirty = true;
			}
		}
	}

	public float critChancePercentage
	{
		get
		{
			return _critChancePercentage;
		}
		set
		{
			if (_critChancePercentage != value)
			{
				_critChancePercentage = value;
				_isDirty = true;
			}
		}
	}

	public float abilityHasteFlat
	{
		get
		{
			return _abilityHasteFlat;
		}
		set
		{
			if (_abilityHasteFlat != value)
			{
				_abilityHasteFlat = value;
				_isDirty = true;
			}
		}
	}

	public float abilityHastePercentage
	{
		get
		{
			return _abilityHastePercentage;
		}
		set
		{
			if (_abilityHastePercentage != value)
			{
				_abilityHastePercentage = value;
				_isDirty = true;
			}
		}
	}

	public float tenacityFlat
	{
		get
		{
			return _tenacityFlat;
		}
		set
		{
			if (_tenacityFlat != value)
			{
				_tenacityFlat = value;
				_isDirty = true;
			}
		}
	}

	public float tenacityPercentage
	{
		get
		{
			return _tenacityPercentage;
		}
		set
		{
			if (_tenacityPercentage != value)
			{
				_tenacityPercentage = value;
				_isDirty = true;
			}
		}
	}

	public float movementSpeedPercentage
	{
		get
		{
			return _movementSpeedPercentage;
		}
		set
		{
			if (_movementSpeedPercentage != value)
			{
				_movementSpeedPercentage = value;
				_isDirty = true;
			}
		}
	}

	public float fireEffectAmpFlat
	{
		get
		{
			return _fireEffectAmpFlat;
		}
		set
		{
			if (_fireEffectAmpFlat != value)
			{
				_fireEffectAmpFlat = value;
				_isDirty = true;
			}
		}
	}

	public float coldEffectAmpFlat
	{
		get
		{
			return _coldEffectAmpFlat;
		}
		set
		{
			if (_coldEffectAmpFlat != value)
			{
				_coldEffectAmpFlat = value;
				_isDirty = true;
			}
		}
	}

	public float lightEffectAmpFlat
	{
		get
		{
			return _lightEffectAmpFlat;
		}
		set
		{
			if (_lightEffectAmpFlat != value)
			{
				_lightEffectAmpFlat = value;
				_isDirty = true;
			}
		}
	}

	public float darkEffectAmpFlat
	{
		get
		{
			return _darkEffectAmpFlat;
		}
		set
		{
			if (_darkEffectAmpFlat != value)
			{
				_darkEffectAmpFlat = value;
				_isDirty = true;
			}
		}
	}

	public float armorFlat
	{
		get
		{
			return _armorFlat;
		}
		set
		{
			if (_armorFlat != value)
			{
				_armorFlat = value;
				_isDirty = true;
			}
		}
	}

	public float armorPercentage
	{
		get
		{
			return _armorPercentage;
		}
		set
		{
			if (_armorPercentage != value)
			{
				_armorPercentage = value;
				_isDirty = true;
			}
		}
	}
}
