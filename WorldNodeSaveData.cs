using System.Collections.Generic;
using UnityEngine;

public class WorldNodeSaveData : VariantDataContainer
{
	public Dictionary<Hero, (Vector3, Quaternion)> heroPositions = new Dictionary<Hero, (Vector3, Quaternion)>();
}
