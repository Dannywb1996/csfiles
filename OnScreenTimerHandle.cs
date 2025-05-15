using System;
using UnityEngine;

public class OnScreenTimerHandle
{
	public string rawText;

	public Func<string> rawTextGetter;

	public Func<float> fillAmountGetter;

	public Color color;
}
