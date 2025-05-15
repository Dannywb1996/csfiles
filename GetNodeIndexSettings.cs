using UnityEngine;

public class GetNodeIndexSettings
{
	public WorldNodeType[] allowedTypes = new WorldNodeType[1] { WorldNodeType.Combat };

	public Vector2Int desiredDistance = new Vector2Int(2, 3);

	public bool preferCloserToExit = true;

	public bool preferNoMainModifier;
}
