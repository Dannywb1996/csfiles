using System;
using UnityEngine;

public struct GetFocusTargetSettings
{
	public Vector3 direction;

	public Vector2 angleLimit;

	public Vector2 normalizedPerpendicularDistLimit;

	public Vector2 normalizedDistLimit;

	public Func<IGamepadFocusable, bool> condition;

	public Func<IGamepadFocusable, float> customScoreFunc;

	public bool useCenterOfRect;

	public GetFocusTargetCacheData cacheData;
}
