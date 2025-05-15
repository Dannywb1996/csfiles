using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class DewHeroesTeleporter : MonoBehaviour, IPlayerPathableArea
{
	public Transform destination;

	public float spacing = 2f;

	public Vector3 pathablePosition => Dew.GetPositionOnGround(destination.position);

	public void TeleportAllHeroes()
	{
		if (!NetworkServer.active)
		{
			throw new InvalidOperationException();
		}
		Vector3 dest = Dew.GetPositionOnGround(destination.position);
		Vector3 right = destination.right.Flattened().normalized;
		IReadOnlyList<Hero> heroes = NetworkedManagerBase<ActorManager>.instance.allHeroes;
		Vector3 currentPos = dest - right * (spacing * (float)(heroes.Count - 1)) / 2f;
		foreach (Hero h in heroes)
		{
			if (!h.IsNullInactiveDeadOrKnockedOut())
			{
				h.Control.CancelOngoingChannels();
				h.Control.CancelOngoingDisplacement();
				h.Control.Teleport(Dew.GetValidAgentDestination_LinearSweep(dest, currentPos));
				currentPos += spacing * right;
			}
		}
	}
}
