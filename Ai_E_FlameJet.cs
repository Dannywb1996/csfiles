using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_E_FlameJet : AbilityInstance
{
	public int spawns = 8;

	public float spawnInterval = 0.375f;

	public float rotateSpeed = 40f;

	public float selfSlowAmount;

	public bool cancelable;

	public float uncancellableTime = 0.5f;

	[SyncVar]
	private Quaternion _syncedRotation;

	private StatusEffect _eff;

	public Quaternion Network_syncedRotation
	{
		get
		{
			return _syncedRotation;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _syncedRotation, 32uL, null);
		}
	}

	protected override void OnCreate()
	{
		Network_syncedRotation = base.info.rotation;
		base.position = base.info.caster.position;
		base.rotation = _syncedRotation;
		base.OnCreate();
	}

	protected override IEnumerator OnCreateSequenced()
	{
		if (!base.isServer)
		{
			yield break;
		}
		_eff = CreateBasicEffect(base.info.caster, new SlowEffect
		{
			strength = selfSlowAmount
		}, (float)spawns * spawnInterval, "flamejet_selfslow");
		base.info.caster.Control.LockGamepadRotation();
		Channel channel = new Channel
		{
			duration = (float)spawns * spawnInterval,
			blockedActions = (Channel.BlockedAction)(6 | (cancelable ? 128 : 0)),
			uncancellableTime = uncancellableTime,
			onCancel = base.Destroy,
			onComplete = base.Destroy
		}.AddValidation(AbilitySelfValidator.Default);
		base.info.caster.Control.StartChannel(channel);
		for (int i = 0; i < spawns; i++)
		{
			if (base.firstTrigger != null)
			{
				base.firstTrigger.fillAmount = (float)(spawns - i) / (float)spawns;
			}
			CreateAbilityInstance<Ai_E_FlameJet_Projectile>(base.info.caster.position, Quaternion.identity, new CastInfo(base.info.caster, CastInfo.GetAngle(_syncedRotation)));
			yield return new SI.WaitForSeconds(spawnInterval);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer)
		{
			if (_eff != null && _eff.isActive)
			{
				_eff.Destroy();
			}
			if (base.firstTrigger != null)
			{
				base.firstTrigger.fillAmount = 0f;
			}
			if (base.info.caster != null)
			{
				base.info.caster.Animation.StopAbilityAnimation();
				base.info.caster.Control.UnlockGamepadRotation();
			}
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!base.isServer)
		{
			return;
		}
		Vector3 vector = default(Vector3);
		bool flag = false;
		DewPlayer owner = base.info.caster.owner;
		if (owner.inputMode == InputMode.Gamepad && !owner.isGamepadExplicitAim)
		{
			if (owner.gamepadTargetEnemy != null)
			{
				flag = true;
				vector = owner.gamepadTargetEnemy.agentPosition;
			}
		}
		else
		{
			flag = true;
			vector = owner.cursorWorldPos;
		}
		if (flag)
		{
			Network_syncedRotation = Quaternion.RotateTowards(_syncedRotation, Quaternion.LookRotation(vector - base.info.caster.position), rotateSpeed * dt);
		}
		base.info.caster.Control.Rotate(_syncedRotation, immediately: false, 0.25f);
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		base.position = base.info.caster.position;
		base.rotation = _syncedRotation;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteQuaternion(_syncedRotation);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteQuaternion(_syncedRotation);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref _syncedRotation, null, reader.ReadQuaternion());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x20L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _syncedRotation, null, reader.ReadQuaternion());
		}
	}
}
