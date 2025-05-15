using System;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class EntitySound : EntityComponent, ICleanup
{
	public SafeAction ClientEvent_OnFootstep;

	public DewAudioClip voiceStart;

	public DewAudioClip voiceIdle;

	public Vector2 voiceIdleInterval = new Vector2(5f, 10f);

	public DewAudioClip voiceDeath;

	public DewAudioClip conversationStart;

	public DewAudioClip conversationClick;

	private DewAudioSource _voiceSource;

	private float _lastIdleTime;

	private float _nextIdleInterval;

	private float _nextRippleTime;

	public bool canDestroy => !_voiceSource.isPlaying;

	public override void OnStart()
	{
		base.OnStart();
		_voiceSource = base.gameObject.AddComponent<DewAudioSource>();
		_voiceSource.space = AudioSpaceType.Normal;
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		_lastIdleTime = Time.time - (voiceIdleInterval.x + voiceIdleInterval.y) * UnityEngine.Random.value * 0.5f;
		_nextIdleInterval = UnityEngine.Random.Range(voiceIdleInterval.x, voiceIdleInterval.y);
		base.entity.EntityEvent_OnDeath += new Action<EventInfoKill>(EntityEventOnDeath);
	}

	private void EntityEventOnDeath(EventInfoKill obj)
	{
		Say(voiceDeath, interruptPrevious: true);
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		if (!base.entity.Visual.skipSpawning)
		{
			Say(voiceStart, interruptPrevious: false);
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!base.entity.isSleeping)
		{
			if (Time.time > _nextRippleTime)
			{
				DoPeriodicSurfaceEffect();
			}
			if (base.isServer && base.entity.isActive && base.entity.Status.isAlive && voiceIdle != null && Time.time - _lastIdleTime > _nextIdleInterval)
			{
				Say(voiceIdle, interruptPrevious: false);
				_lastIdleTime = Time.time;
				_nextIdleInterval = UnityEngine.Random.Range(voiceIdleInterval.x, voiceIdleInterval.y);
			}
		}
	}

	[Server]
	public void Say(DewAudioClip clip, bool interruptPrevious)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntitySound::Say(DewAudioClip,System.Boolean)' called when server was not active");
		}
		else if (!(clip == null) && (!_voiceSource.isPlaying || !(_voiceSource.clip == clip)))
		{
			RpcSay(clip, interruptPrevious);
		}
	}

	[ClientRpc]
	private void RpcSay(DewAudioClip clip, bool interruptPrevious)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteDewAudioClip(clip);
		writer.WriteBool(interruptPrevious);
		SendRPCInternal("System.Void EntitySound::RpcSay(DewAudioClip,System.Boolean)", 448643774, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public void OnCleanup()
	{
	}

	public void DoPeriodicSurfaceEffect()
	{
		if (base.entity.Visual.currentYOffset > 0.1f)
		{
			_nextRippleTime = Time.time + 0.4f;
			return;
		}
		DewSurfaceData data = GetFootstepData();
		if (data == null || data.fxPeriodicEffect == null || (data.excludeLesserMonster && base.entity is Monster { type: Monster.MonsterType.Lesser }))
		{
			_nextRippleTime = Time.time + 0.4f;
			return;
		}
		_nextRippleTime = Time.time + UnityEngine.Random.Range(data.periodicEffectInterval.x, data.periodicEffectInterval.y);
		if (!data.onlyWhenWalking || base.entity.Control.isWalking)
		{
			DewEffect.PlayNew(data.fxPeriodicEffect, Dew.GetPositionOnGround(base.entity.position) + Vector3.up * 0.05f, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f), base.netIdentity);
		}
	}

	public void DoFootstep()
	{
		if (!(base.entity.Visual.currentYOffset > 0.1f))
		{
			if (SingletonDewNetworkBehaviour<Room>.softInstance != null)
			{
				SingletonDewNetworkBehaviour<Room>.softInstance.ClientEvent_OnFootstep?.Invoke(base.entity);
			}
			ClientEvent_OnFootstep?.Invoke();
			DewSurfaceData data = GetFootstepData();
			if (!(data == null))
			{
				Vector3 lfoot = base.entity.Visual.GetBonePosition(HumanBodyBones.LeftFoot);
				Vector3 rfoot = base.entity.Visual.GetBonePosition(HumanBodyBones.RightFoot);
				DewEffect.PlayNew(data.fxHeroFootstep, (lfoot.y < rfoot.y) ? lfoot : rfoot, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f), base.netIdentity);
			}
		}
	}

	private DewSurfaceData GetFootstepData()
	{
		if (Physics.Raycast(base.entity.position + Vector3.up * 1.5f, Vector3.down, out var result, 5f, LayerMasks.Ground) && result.collider.TryGetComponent<Room_SurfaceOverride>(out var surface))
		{
			if (surface.data != null)
			{
				return surface.data;
			}
			return DewSurfaceData.defaultSurface;
		}
		if (SingletonDewNetworkBehaviour<Room>.softInstance != null)
		{
			return SingletonDewNetworkBehaviour<Room>.softInstance.defaultSurface;
		}
		return DewSurfaceData.defaultSurface;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcSay__DewAudioClip__Boolean(DewAudioClip clip, bool interruptPrevious)
	{
		if (!(clip == null) && (!_voiceSource.isPlaying || interruptPrevious))
		{
			AudioType type = (base.entity.isOwned ? AudioType.GameSelf : (base.entity.IsAnyBoss() ? AudioType.GameBoss : ((!(base.entity.owner != null) || !base.entity.owner.isHumanPlayer) ? AudioType.GameOthers : AudioType.GameOtherPlayers)));
			_voiceSource.type = type;
			_voiceSource.clip = clip;
			_voiceSource.Play();
		}
	}

	protected static void InvokeUserCode_RpcSay__DewAudioClip__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSay called on server.");
		}
		else
		{
			((EntitySound)obj).UserCode_RpcSay__DewAudioClip__Boolean(reader.ReadDewAudioClip(), reader.ReadBool());
		}
	}

	static EntitySound()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(EntitySound), "System.Void EntitySound::RpcSay(DewAudioClip,System.Boolean)", InvokeUserCode_RpcSay__DewAudioClip__Boolean);
	}
}
