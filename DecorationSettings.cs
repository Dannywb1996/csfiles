using System;
using UnityEngine;

[Serializable]
public class DecorationSettings
{
	public float decoDensity = 0.25f;

	public GameObject[] decorations;

	public float decoPositionRandomMag = 1.5f;

	public bool uniformScale;

	public Vector3 decoScaleMin = Vector3.one * 0.8f;

	public Vector3 decoScaleMax = Vector3.one * 1.2f;

	public Vector3 decoRotationMin = Vector3.zero;

	public Vector3 decoRotationMax = new Vector3(0f, 360f, 0f);
}
