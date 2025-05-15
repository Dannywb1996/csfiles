using System;
using UnityEngine;

public class WorldMessageSetting
{
	public string rawText;

	public Vector3 worldPos;

	[NonSerialized]
	public Func<Vector3> worldPosGetter;

	public Color color = Color.white;

	public Vector2? popOffset;
}
