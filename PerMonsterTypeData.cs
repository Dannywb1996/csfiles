using System;

[Serializable]
public struct PerMonsterTypeData<T>
{
	public T lesser;

	public T normal;

	public T miniBoss;

	public T boss;

	public T Get(Monster.MonsterType type)
	{
		return type switch
		{
			Monster.MonsterType.Lesser => lesser, 
			Monster.MonsterType.Normal => normal, 
			Monster.MonsterType.MiniBoss => miniBoss, 
			Monster.MonsterType.Boss => boss, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public void Set(Monster.MonsterType type, T value)
	{
		switch (type)
		{
		case Monster.MonsterType.Lesser:
			lesser = value;
			break;
		case Monster.MonsterType.Normal:
			normal = value;
			break;
		case Monster.MonsterType.MiniBoss:
			miniBoss = value;
			break;
		case Monster.MonsterType.Boss:
			boss = value;
			break;
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
	}
}
