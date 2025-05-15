using System;
using UnityEngine;

public class Se_WaypointTeleport : StatusEffect
{
	public DewAnimationClip channelAnim;

	public DewAnimationClip completeAnim;

	public float channelDuration = 1f;

	public float postTeleportDaze = 0.75f;

	public GameObject teleportEffect;

	private bool _didFail;

	private Channel _daze;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			base.victim.Control.CancelOngoingChannels();
			base.victim.Control.Stop();
			base.victim.Control.Rotate(Vector3.back, immediately: false);
			if (channelAnim != null)
			{
				base.victim.Animation.PlayAbilityAnimation(channelAnim);
			}
			base.victim.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(EntityEventOnTakeDamage);
			_daze = base.info.caster.Control.StartDaze(channelDuration);
			Channel daze = _daze;
			daze.onCancel = (Action)Delegate.Combine(daze.onCancel, (Action)delegate
			{
				_didFail = true;
				Destroy();
			});
			SetTimer(channelDuration);
			ShowOnScreenTimer();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (!base.isServer)
		{
			return;
		}
		if (_daze != null && _daze.isAlive)
		{
			_daze.Cancel();
		}
		if (!(base.victim != null) || !base.victim.isActive)
		{
			return;
		}
		base.victim.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(EntityEventOnTakeDamage);
		if (channelAnim != null)
		{
			base.victim.Animation.StopAbilityAnimation(channelAnim);
		}
		if (!_didFail)
		{
			if (completeAnim != null)
			{
				base.victim.Animation.PlayAbilityAnimation(completeAnim);
			}
			Teleport(base.victim, base.info.point);
			FxPlayNetworked(teleportEffect, base.victim);
			base.victim.Control.Stop();
			base.victim.Control.StartDaze(postTeleportDaze);
		}
		else if (base.victim.owner != null && base.victim.owner.isHumanPlayer)
		{
			base.victim.owner.TpcShowCenterMessage(CenterMessageType.Error, "InGame_Message_TeleportUnavailableInCombat");
		}
	}

	private void EntityEventOnTakeDamage(EventInfoDamage obj)
	{
		if (base.isActive && !(obj.actor is ElementalStatusEffect) && obj.actor.FindFirstOfType<Entity>() is Monster)
		{
			_didFail = true;
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
