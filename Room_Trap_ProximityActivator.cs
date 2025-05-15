using System.Collections.Generic;
using UnityEngine;

public class Room_Trap_ProximityActivator : Room_Trap_ActivatorBase
{
	public DewCollider range;

	public bool isActivatedByHeroes = true;

	public bool isActivatedByMonster;

	public float activationCooldown = 2.5f;

	private List<Entity> _ents = new List<Entity>();

	private float _lastActivationTime = float.NegativeInfinity;

	protected override void Awake()
	{
		base.Awake();
		range.receiveEntityCallbacks = true;
		range.invokeEventsOnClients = false;
		range.onEntityEnter.AddListener(delegate(Entity e)
		{
			if (base.isServer)
			{
				_ents.Add(e);
			}
		});
		range.onEntityExit.AddListener(delegate(Entity e)
		{
			if (base.isServer)
			{
				_ents.Remove(e);
			}
		});
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!base.isServer || Time.time - _lastActivationTime < activationCooldown)
		{
			return;
		}
		foreach (Entity e in _ents)
		{
			if ((isActivatedByHeroes && e is Hero) || (isActivatedByMonster && e is Monster))
			{
				_lastActivationTime = Time.time;
				Activate();
				break;
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
