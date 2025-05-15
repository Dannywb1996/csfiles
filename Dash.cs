using System;
using UnityEngine;

[Serializable]
public class Dash
{
	public float distance = 2.25f;

	public float duration = 0.25f;

	public DewEase ease;

	public bool rotateForward = true;

	public bool affectedByMovementSpeed;

	public bool useLinearSweep = true;

	public bool canGoOverTerrain;

	public void ApplyByDestination(Entity ent, Vector3 dest, Action<DispByDestination> beforeApply = null)
	{
		ApplyByDirection(ent, dest - ent.agentPosition, beforeApply);
	}

	public void ApplyByDirection(Entity ent, Vector3 dir, Action<DispByDestination> beforeApply = null)
	{
		Vector3 dest = (useLinearSweep ? Dew.GetValidAgentDestination_LinearSweep(ent.agentPosition, ent.agentPosition + dir.normalized * distance) : Dew.GetValidAgentDestination_Closest(ent.agentPosition, ent.agentPosition + dir.normalized * distance));
		DispByDestination disp = new DispByDestination
		{
			canGoOverTerrain = canGoOverTerrain,
			destination = dest,
			duration = duration,
			ease = ease,
			isCanceledByCC = true,
			isFriendly = true,
			rotateForward = rotateForward,
			affectedByMovementSpeed = affectedByMovementSpeed
		};
		beforeApply?.Invoke(disp);
		ent.Control.StartDisplacement(disp);
	}
}
