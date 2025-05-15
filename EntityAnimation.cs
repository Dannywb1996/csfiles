using System;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class EntityAnimation : EntityComponent
{
	[Serializable]
	public struct AnimationClipWithSpeed
	{
		public static AnimationClipWithSpeed Default = new AnimationClipWithSpeed
		{
			speed = 1f
		};

		public AnimationClip clip;

		public float speed;
	}

	public struct AbilityAnimationStatus
	{
		public bool isPlaying;

		public EaseFunction easeFunction;

		public float rawClipDuration;

		public float duration;

		public Vector2 trimRange;

		public float elapsedTime;

		public DewAnimationClip currentClip;

		public float overrideWalkNormalizedDuration;

		public float normalizedTime => elapsedTime / duration;

		public float normalizedTimeParameter => (trimRange.x + (trimRange.y - trimRange.x) * easeFunction(0f, 1f, normalizedTime)) / rawClipDuration;
	}

	public enum ReplaceableAnimationType
	{
		Idle,
		RunForward,
		RunForwardRight,
		RunRight,
		RunBackwardRight,
		RunBackward,
		RunBackwardLeft,
		RunLeft,
		RunForwardLeft,
		Ability,
		Stagger,
		Death
	}

	public enum LocomotionType
	{
		Simple,
		FourDirections,
		EightDirections
	}

	private const float AbilityAnimationTransitionTime = 0.1f;

	private const float AbilityAnimationCancelByWalkGraceTime = 0.075f;

	public AnimationClipWithSpeed idle = AnimationClipWithSpeed.Default;

	public AnimationClipWithSpeed lobby = AnimationClipWithSpeed.Default;

	public LocomotionType locomotion;

	public float walkAnimationSpeed = 1f;

	[FormerlySerializedAs("walkClip")]
	public AnimationClip runFowardClip;

	public AnimationClip runForwardRightClip;

	public AnimationClip runRightClip;

	public AnimationClip runBackwardRightClip;

	public AnimationClip runBackwardClip;

	public AnimationClip runBackwardLeftClip;

	public AnimationClip runLeftClip;

	public AnimationClip runForwardLeftClip;

	public AnimationClipWithSpeed stagger = AnimationClipWithSpeed.Default;

	public AnimationClipWithSpeed death = AnimationClipWithSpeed.Default;

	[NonSerialized]
	public Animator animator;

	private AnimatorOverrideController _animatorOverrides;

	internal Animator _abilitySampler;

	private AnimatorOverrideController _abilitySamplerOverrides;

	private Transform _rotationFixTransformTarget;

	private Transform _rotationFixTransformSource;

	private AnimationClip[] _baseClips = new AnimationClip[12];

	public AbilityAnimationStatus abilityAnimStatus;

	private bool _didInit;

	public bool support4Directions => locomotion != LocomotionType.Simple;

	public bool support8Directions => locomotion == LocomotionType.EightDirections;

	public override void OnStart()
	{
		base.OnStart();
		if (_didInit)
		{
			return;
		}
		_didInit = true;
		InitAnimator();
		if (animator == null)
		{
			return;
		}
		if (idle.clip != null)
		{
			ReplaceAnimationLocal(ReplaceableAnimationType.Idle, idle);
		}
		switch (locomotion)
		{
		case LocomotionType.Simple:
			if (runFowardClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunForward, runFowardClip);
			}
			break;
		case LocomotionType.FourDirections:
			if (runFowardClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunForward, runFowardClip);
			}
			if (runRightClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunRight, runRightClip);
			}
			if (runBackwardClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunBackward, runBackwardClip);
			}
			if (runLeftClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunLeft, runLeftClip);
			}
			break;
		case LocomotionType.EightDirections:
			if (runFowardClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunForward, runFowardClip);
			}
			if (runForwardRightClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunForwardRight, runForwardRightClip);
			}
			if (runRightClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunRight, runRightClip);
			}
			if (runBackwardRightClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunBackwardRight, runBackwardRightClip);
			}
			if (runBackwardClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunBackward, runBackwardClip);
			}
			if (runBackwardLeftClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunBackwardLeft, runBackwardLeftClip);
			}
			if (runLeftClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunLeft, runLeftClip);
			}
			if (runForwardLeftClip != null)
			{
				ReplaceAnimationLocal(ReplaceableAnimationType.RunForwardLeft, runForwardLeftClip);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		animator.SetInteger("locomotionType", (int)locomotion);
		if (stagger.clip != null)
		{
			ReplaceAnimationLocal(ReplaceableAnimationType.Stagger, stagger);
		}
		if (death.clip != null)
		{
			ReplaceAnimationLocal(ReplaceableAnimationType.Death, death);
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!base.entity.isSleeping && base.entity.isActive && !(animator == null))
		{
			animator.SetFloat("walkSpeedMultiplier", base.entity.Control.walkStrength * walkAnimationSpeed * base.entity.Status.movementSpeedMultiplier / Mathf.Clamp(base.entity.Visual.etScaleMultiplier.x, 0.01f, 100f));
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!base.entity.isSleeping && animator != null)
		{
			FrameUpdateAnimations();
		}
	}

	public void PlayStaggerAnimation()
	{
		RpcPlayStaggerAnimation();
	}

	[ClientRpc]
	private void RpcPlayStaggerAnimation()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void EntityAnimation::RpcPlayStaggerAnimation()", -1820776812, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void AssignBaseClipReferences(AnimationClip[] clips)
	{
		string[] names = Enum.GetNames(typeof(ReplaceableAnimationType));
		int[] values = (int[])Enum.GetValues(typeof(ReplaceableAnimationType));
		for (int i = 0; i < clips.Length; i++)
		{
			for (int j = 0; j < names.Length; j++)
			{
				if (names[j] == clips[i].name)
				{
					_baseClips[values[j]] = clips[i];
					break;
				}
			}
		}
	}

	[Server]
	public void ReplaceAnimation(ReplaceableAnimationType type, AnimationClip clip)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAnimation::ReplaceAnimation(EntityAnimation/ReplaceableAnimationType,UnityEngine.AnimationClip)' called when server was not active");
		}
		else
		{
			RpcReplaceAnimation(type, clip);
		}
	}

	public void ReplaceAnimationLocal(ReplaceableAnimationType type, AnimationClip newClip)
	{
		List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
		overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(_baseClips[(int)type], newClip));
		_animatorOverrides.ApplyOverrides(overrides);
		if (_abilitySamplerOverrides != null)
		{
			_abilitySamplerOverrides.ApplyOverrides(overrides);
		}
	}

	public void ReplaceAnimationLocal(ReplaceableAnimationType type, AnimationClipWithSpeed newData)
	{
		ReplaceAnimationLocal(type, newData.clip);
		animator.SetFloat("speed" + type, newData.speed);
		if (_abilitySampler != null)
		{
			_abilitySampler.SetFloat("speed" + type, newData.speed);
		}
	}

	[ClientRpc]
	private void RpcReplaceAnimation(ReplaceableAnimationType type, AnimationClip clip)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_EntityAnimation_002FReplaceableAnimationType(writer, type);
		writer.WriteAnimationClip(clip);
		SendRPCInternal("System.Void EntityAnimation::RpcReplaceAnimation(EntityAnimation/ReplaceableAnimationType,UnityEngine.AnimationClip)", 1139056298, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void PlayAbilityAnimation(DewAnimationClip clip, float speed = 1f)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAnimation::PlayAbilityAnimation(DewAnimationClip,System.Single)' called when server was not active");
		}
		else if (!(clip == null))
		{
			RpcPlayAbilityAnimation(clip, clip.GetEntryIndex(), speed);
		}
	}

	[Server]
	public void PlayAbilityAnimation(DewAnimationClip clip, float speed, float clipSelectValue)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAnimation::PlayAbilityAnimation(DewAnimationClip,System.Single,System.Single)' called when server was not active");
		}
		else if (!(clip == null))
		{
			RpcPlayAbilityAnimation(clip, clip.GetEntryIndex(clipSelectValue), speed);
		}
	}

	[ClientRpc]
	private void RpcPlayAbilityAnimation(DewAnimationClip clip, int index, float speed)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteDewAnimationClip(clip);
		writer.WriteInt(index);
		writer.WriteFloat(speed);
		SendRPCInternal("System.Void EntityAnimation::RpcPlayAbilityAnimation(DewAnimationClip,System.Int32,System.Single)", -998352957, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void StopAbilityAnimation()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAnimation::StopAbilityAnimation()' called when server was not active");
		}
		else
		{
			RpcStopAbilityAnimation();
		}
	}

	[Server]
	public void StopAbilityAnimation(DewAnimationClip clip)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityAnimation::StopAbilityAnimation(DewAnimationClip)' called when server was not active");
		}
		else
		{
			RpcStopAbilityAnimation(clip);
		}
	}

	[ClientRpc]
	private void RpcStopAbilityAnimation(DewAnimationClip clip)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteDewAnimationClip(clip);
		SendRPCInternal("System.Void EntityAnimation::RpcStopAbilityAnimation(DewAnimationClip)", 318803303, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcStopAbilityAnimation()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void EntityAnimation::RpcStopAbilityAnimation()", 1050101317, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void InitAnimator()
	{
		animator = GetComponentInChildren<Animator>();
		if (animator == null)
		{
			return;
		}
		_animatorOverrides = new AnimatorOverrideController(animator.runtimeAnimatorController);
		animator.runtimeAnimatorController = _animatorOverrides;
		AssignBaseClipReferences(animator.runtimeAnimatorController.animationClips);
		if (!animator.isHuman)
		{
			return;
		}
		_abilitySampler = UnityEngine.Object.Instantiate(animator, animator.transform);
		_abilitySampler.name = "Model (AbilitySampler)";
		_abilitySampler.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		Component[] componentsInChildren = _abilitySampler.GetComponentsInChildren<Component>(includeInactive: true);
		foreach (Component c in componentsInChildren)
		{
			if (!(c == null) && !(c is Transform) && !(c == _abilitySampler))
			{
				if (c is Light && c.TryGetComponent<UniversalAdditionalLightData>(out var cc))
				{
					UnityEngine.Object.DestroyImmediate(cc);
				}
				if (c is Light && c.TryGetComponent<FxPointLight>(out var pl))
				{
					UnityEngine.Object.DestroyImmediate(pl);
				}
				if (c is SkinnedMeshRenderer && c.TryGetComponent<Cloth>(out var cloth))
				{
					UnityEngine.Object.DestroyImmediate(cloth);
				}
				UnityEngine.Object.DestroyImmediate(c);
			}
		}
		_abilitySamplerOverrides = new AnimatorOverrideController(_abilitySampler.runtimeAnimatorController);
		_abilitySampler.runtimeAnimatorController = _abilitySamplerOverrides;
		_rotationFixTransformTarget = animator.GetBoneTransform(HumanBodyBones.Spine);
		_rotationFixTransformSource = _abilitySampler.GetBoneTransform(HumanBodyBones.Spine);
	}

	private void FrameUpdateAnimations()
	{
		animator.SetBool("isWalking", base.entity.Control.isWalking && (!abilityAnimStatus.isPlaying || abilityAnimStatus.currentClip.overrideWalk != DewAnimationClip.OverrideWalkBehavior.Full));
		if (base.entity is Hero hero)
		{
			animator.SetBool("isDead", !base.entity.isActive || hero.isKnockedOut);
		}
		else
		{
			animator.SetBool("isDead", !base.entity.isActive);
		}
		if (abilityAnimStatus.isPlaying)
		{
			if (!Dew.IsOkay(abilityAnimStatus.elapsedTime) || !Dew.IsOkay(abilityAnimStatus.normalizedTime))
			{
				abilityAnimStatus.elapsedTime = 0f;
				abilityAnimStatus.isPlaying = false;
			}
			abilityAnimStatus.elapsedTime = Mathf.MoveTowards(abilityAnimStatus.elapsedTime, abilityAnimStatus.duration, Time.deltaTime);
			if (abilityAnimStatus.elapsedTime >= abilityAnimStatus.duration || !base.entity.isActive)
			{
				abilityAnimStatus.isPlaying = false;
			}
			else if (base.entity.Control.isWalking && abilityAnimStatus.elapsedTime > 0.075f && (abilityAnimStatus.currentClip.overrideWalk == DewAnimationClip.OverrideWalkBehavior.None || abilityAnimStatus.normalizedTime > abilityAnimStatus.overrideWalkNormalizedDuration))
			{
				abilityAnimStatus.isPlaying = false;
			}
			animator.SetFloat("abilityAnimationNormalizedTime", abilityAnimStatus.normalizedTimeParameter);
			if (_abilitySampler != null)
			{
				_abilitySampler.SetFloat("abilityAnimationNormalizedTime", abilityAnimStatus.normalizedTimeParameter);
			}
		}
		float layerWeight = animator.GetLayerWeight(1);
		int desiredUpperWeight = (abilityAnimStatus.isPlaying ? 1 : 0);
		float nextUpperWeight = Mathf.MoveTowards(layerWeight, desiredUpperWeight, Time.deltaTime / 0.1f);
		animator.SetLayerWeight(1, nextUpperWeight);
	}

	private void LateUpdate()
	{
		if (animator == null)
		{
			return;
		}
		if (_rotationFixTransformSource != null)
		{
			float weight = animator.GetLayerWeight(1);
			_rotationFixTransformTarget.rotation = Quaternion.Slerp(_rotationFixTransformTarget.rotation, _rotationFixTransformSource.rotation, weight);
		}
		if (locomotion != LocomotionType.Simple)
		{
			Vector3 walkDir = base.entity.Control.agentVelocity;
			if (walkDir.sqrMagnitude > 0.001f)
			{
				Vector3 walkLocalForward = (Quaternion.Inverse(base.transform.rotation) * Quaternion.LookRotation(walkDir) * Vector3.forward).normalized;
				animator.SetFloat("walkDirX", walkLocalForward.x, 0.1f, Time.deltaTime);
				animator.SetFloat("walkDirY", walkLocalForward.z, 0.1f, Time.deltaTime);
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPlayStaggerAnimation()
	{
		if (animator != null)
		{
			animator.SetTrigger("Stagger");
		}
	}

	protected static void InvokeUserCode_RpcPlayStaggerAnimation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayStaggerAnimation called on server.");
		}
		else
		{
			((EntityAnimation)obj).UserCode_RpcPlayStaggerAnimation();
		}
	}

	protected void UserCode_RpcReplaceAnimation__ReplaceableAnimationType__AnimationClip(ReplaceableAnimationType type, AnimationClip clip)
	{
		ReplaceAnimationLocal(type, clip);
	}

	protected static void InvokeUserCode_RpcReplaceAnimation__ReplaceableAnimationType__AnimationClip(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcReplaceAnimation called on server.");
		}
		else
		{
			((EntityAnimation)obj).UserCode_RpcReplaceAnimation__ReplaceableAnimationType__AnimationClip(GeneratedNetworkCode._Read_EntityAnimation_002FReplaceableAnimationType(reader), reader.ReadAnimationClip());
		}
	}

	protected void UserCode_RpcPlayAbilityAnimation__DewAnimationClip__Int32__Single(DewAnimationClip clip, int index, float speed)
	{
		DewAnimationEntry entry = clip.GetEntry(index);
		if (entry == null)
		{
			return;
		}
		ReplaceAnimationLocal(ReplaceableAnimationType.Ability, entry.rawClip);
		if (speed < 0.01f)
		{
			speed = 0.01f;
		}
		if (Dew.IsOkay(speed) && Dew.IsOkay(entry.duration) && !(entry.duration <= 0f))
		{
			abilityAnimStatus.isPlaying = true;
			abilityAnimStatus.duration = Mathf.Max(entry.duration / speed, 0.0001f);
			abilityAnimStatus.easeFunction = EasingFunction.GetEasingFunction(entry.timeCurve);
			abilityAnimStatus.elapsedTime = 0f;
			abilityAnimStatus.rawClipDuration = entry.rawClip.length;
			abilityAnimStatus.trimRange = entry.trimRange;
			abilityAnimStatus.currentClip = clip;
			abilityAnimStatus.overrideWalkNormalizedDuration = clip.overrideWalkNormalizedDuration;
			if (animator.isHuman && (clip.overrideWalk == DewAnimationClip.OverrideWalkBehavior.UpperBody || clip.onlyUpperBody))
			{
				animator.SetLayerWeight(1, 1f);
			}
			animator.SetFloat("abilityAnimationSpeed", entry.rawClip.length / entry.duration * speed);
			animator.SetFloat("abilityAnimationNormalizedTime", abilityAnimStatus.normalizedTimeParameter);
			if (!clip.onlyUpperBody)
			{
				animator.SetTrigger("StartAbilityAnimation");
			}
			if (_abilitySampler != null)
			{
				_abilitySampler.SetFloat("abilityAnimationSpeed", entry.rawClip.length / entry.duration * speed);
				_abilitySampler.SetFloat("abilityAnimationNormalizedTime", abilityAnimStatus.normalizedTimeParameter);
				_abilitySampler.SetTrigger("StartAbilityAnimation");
			}
		}
	}

	protected static void InvokeUserCode_RpcPlayAbilityAnimation__DewAnimationClip__Int32__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayAbilityAnimation called on server.");
		}
		else
		{
			((EntityAnimation)obj).UserCode_RpcPlayAbilityAnimation__DewAnimationClip__Int32__Single(reader.ReadDewAnimationClip(), reader.ReadInt(), reader.ReadFloat());
		}
	}

	protected void UserCode_RpcStopAbilityAnimation__DewAnimationClip(DewAnimationClip clip)
	{
		if (!(abilityAnimStatus.currentClip != clip))
		{
			abilityAnimStatus.elapsedTime = abilityAnimStatus.duration;
			animator.SetTrigger("StopAbilityAnimation");
		}
	}

	protected static void InvokeUserCode_RpcStopAbilityAnimation__DewAnimationClip(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStopAbilityAnimation called on server.");
		}
		else
		{
			((EntityAnimation)obj).UserCode_RpcStopAbilityAnimation__DewAnimationClip(reader.ReadDewAnimationClip());
		}
	}

	protected void UserCode_RpcStopAbilityAnimation()
	{
		abilityAnimStatus.elapsedTime = abilityAnimStatus.duration;
		animator.SetTrigger("StopAbilityAnimation");
	}

	protected static void InvokeUserCode_RpcStopAbilityAnimation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStopAbilityAnimation called on server.");
		}
		else
		{
			((EntityAnimation)obj).UserCode_RpcStopAbilityAnimation();
		}
	}

	static EntityAnimation()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(EntityAnimation), "System.Void EntityAnimation::RpcPlayStaggerAnimation()", InvokeUserCode_RpcPlayStaggerAnimation);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityAnimation), "System.Void EntityAnimation::RpcReplaceAnimation(EntityAnimation/ReplaceableAnimationType,UnityEngine.AnimationClip)", InvokeUserCode_RpcReplaceAnimation__ReplaceableAnimationType__AnimationClip);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityAnimation), "System.Void EntityAnimation::RpcPlayAbilityAnimation(DewAnimationClip,System.Int32,System.Single)", InvokeUserCode_RpcPlayAbilityAnimation__DewAnimationClip__Int32__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityAnimation), "System.Void EntityAnimation::RpcStopAbilityAnimation(DewAnimationClip)", InvokeUserCode_RpcStopAbilityAnimation__DewAnimationClip);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityAnimation), "System.Void EntityAnimation::RpcStopAbilityAnimation()", InvokeUserCode_RpcStopAbilityAnimation);
	}
}
