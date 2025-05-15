using System;
using UnityEngine;

[Serializable]
public struct ChaosReward
{
	public ChaosRewardType type;

	public float quantity;

	[HideInInspector]
	public Rarity rarity;
}
