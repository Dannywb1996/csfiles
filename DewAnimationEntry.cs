using System;
using UnityEngine;

[Serializable]
public class DewAnimationEntry
{
	public AnimationClip rawClip;

	public Vector2 trimRange = new Vector2(0f, 0f);

	public float duration = 2f;

	public DewEase timeCurve;

	private float _rawClipLength
	{
		get
		{
			if (!rawClip)
			{
				return float.PositiveInfinity;
			}
			return rawClip.length;
		}
	}

	public void Validate()
	{
		if (!(rawClip == null))
		{
			if (trimRange.y > rawClip.length)
			{
				trimRange.y = rawClip.length;
			}
			if (trimRange.x == trimRange.y)
			{
				trimRange.x = 0f;
				trimRange.y = rawClip.length;
			}
		}
	}
}
