using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_EmbraceNewIdentity : AbilityInstance
{
	[NonSerialized]
	[SyncVar]
	public SkillTrigger targetSkill;

	public DewAnimationClip animChannel;

	public float channelTime;

	public DewAnimationClip animChannelEnd;

	public float postDaze;

	public Transform[] onSkillTransform;

	public DewEase skillEase;

	private Vector3 _targetSkillInitPos;

	private EaseFunction _easeFunc;

	protected NetworkBehaviourSyncVar ___targetSkillNetId;

	public SkillTrigger NetworktargetSkill
	{
		get
		{
			return GetSyncVarNetworkBehaviour(___targetSkillNetId, ref targetSkill);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref targetSkill, 32uL, null, ref ___targetSkillNetId);
		}
	}

	protected override void OnCreate()
	{
		_targetSkillInitPos = ((NetworktargetSkill != null) ? NetworktargetSkill.position : default(Vector3));
		_easeFunc = EasingFunction.GetEasingFunction(skillEase);
		UpdatePositions();
		base.OnCreate();
		if (!base.isServer)
		{
			return;
		}
		if (NetworktargetSkill.IsNullOrInactive() || NetworktargetSkill.owner != null || NetworktargetSkill.handOwner != null)
		{
			Destroy();
			return;
		}
		base.info.caster.Animation.PlayAbilityAnimation(animChannel);
		base.info.caster.Control.RotateTowards(NetworktargetSkill.position, immediately: true);
		base.info.caster.Control.CancelOngoingChannels();
		base.info.caster.Control.CancelOngoingDisplacement();
		CreateBasicEffect(base.info.caster, new InvulnerableEffect(), channelTime + postDaze, "embraceidentity_invul");
		CreateBasicEffect(base.info.caster, new InvisibleEffect(), channelTime + postDaze, "embraceidentity_invis");
		base.info.caster.Control.StartChannel(new Channel
		{
			blockedActions = Channel.BlockedAction.Everything,
			duration = channelTime,
			onCancel = base.DestroyIfActive,
			onComplete = delegate
			{
				Hero hero = (Hero)base.info.caster;
				if (hero.Skill.Identity != null)
				{
					SkillTrigger skillTrigger = hero.Skill.UnequipSkill(HeroSkillLocation.Identity, hero.position, ignoreCanReplace: true);
					skillTrigger._lastDismantler = hero;
					skillTrigger.DismantleSkill();
				}
				hero.Skill.EquipSkill(HeroSkillLocation.Identity, NetworktargetSkill, ignoreCanReplace: true);
				base.info.caster.Animation.PlayAbilityAnimation(animChannelEnd);
				base.info.caster.Control.StartDaze(postDaze);
				DestroyIfActive();
			}
		});
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		UpdatePositions();
	}

	private void UpdatePositions()
	{
		if (NetworktargetSkill != null)
		{
			float v = Mathf.Clamp01((Time.time - base.creationTime) / channelTime);
			NetworktargetSkill.transform.position = Vector3.Lerp(_targetSkillInitPos, base.info.caster.position, _easeFunc(0f, 1f, v));
			Transform[] array = onSkillTransform;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].position = NetworktargetSkill.position;
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteNetworkBehaviour(NetworktargetSkill);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteNetworkBehaviour(NetworktargetSkill);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref targetSkill, null, reader, ref ___targetSkillNetId);
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x20L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref targetSkill, null, reader, ref ___targetSkillNetId);
		}
	}
}
