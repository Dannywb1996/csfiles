using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[ExecuteAlways]
[RequireComponent(typeof(PlayableDirector))]
public class DewCutsceneDirector : DewNetworkBehaviour
{
	public Action onFinish;

	public bool stopAllPlayers = true;

	public bool disableAi = true;

	public float reenableAiDelay;

	public float cutsceneStartDelay;

	public bool enableSkip = true;

	private PlayableDirector _director;

	private float _lastStopTime;

	private bool _isSkipping;

	private bool _didSkip;

	private void Start()
	{
		_director = GetComponent<PlayableDirector>();
		_director.playOnAwake = false;
		TimelineAsset playable = (TimelineAsset)_director.playableAsset;
		if (!Application.IsPlaying(this))
		{
			return;
		}
		foreach (PlayableBinding o in playable.outputs)
		{
			if (o.streamName == "Cinemachine Track")
			{
				_director.SetGenericBinding(o.sourceObject, Dew.mainCamera.GetComponent<CinemachineBrain>());
			}
			if (o.streamName.StartsWith("Activation Track"))
			{
				GameObject gobj = _director.GetGenericBinding(o.sourceObject) as GameObject;
				if (!(gobj == null) && Application.IsPlaying(this))
				{
					gobj.SetActive(value: false);
				}
			}
		}
		_director.stopped += delegate
		{
			OnStopPlaying();
		};
		NetworkedManagerBase<ActorManager>.instance.onEntityAdd += new Action<Entity>(OnEntityAdd);
	}

	public void Play()
	{
		if (!Application.IsPlaying(this))
		{
			return;
		}
		if (base.isServer)
		{
			if (stopAllPlayers)
			{
				StopAllPlayers();
			}
			if (disableAi)
			{
				EntityAI.DisableAI = true;
			}
		}
		StopAllCoroutines();
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			_isSkipping = false;
			ManagerBase<CameraManager>.instance.isPlayingCutscene = true;
			ManagerBase<CameraManager>.instance.currentCutsceneDirector = this;
			ManagerBase<CameraManager>.instance.DoCutsceneFadeOut();
			yield return new WaitForSeconds(ManagerBase<CameraManager>.instance.cutsceneFadeTime + cutsceneStartDelay);
			if (base.isServer && stopAllPlayers)
			{
				StopAllPlayers();
			}
			InGameUIManager.instance.SetState("Cutscene");
			ManagerBase<CameraManager>.instance.SetActiveEntityVCam(value: false);
			ManagerBase<CameraManager>.instance.DoCutsceneFadeIn();
			_director.Play();
		}
	}

	[Command(requiresAuthority = false)]
	public void CmdSkip()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendCommandInternal("System.Void DewCutsceneDirector::CmdSkip()", 895073170, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcSkip()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void DewCutsceneDirector::RpcSkip()", -400979971, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void StopAllPlayers()
	{
		foreach (Hero allHero in NetworkedManagerBase<ActorManager>.instance.allHeroes)
		{
			allHero.Control.Stop();
			allHero.Control.CancelOngoingChannels();
		}
	}

	private void OnStopPlaying()
	{
		StopAllCoroutines();
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (!_isSkipping)
			{
				ManagerBase<CameraManager>.instance.DoCutsceneFadeOut();
				yield return new WaitForSeconds(ManagerBase<CameraManager>.instance.cutsceneFadeTime);
			}
			ManagerBase<CameraManager>.instance.DoCutsceneFadeIn();
			ManagerBase<CameraManager>.instance.SetActiveEntityVCam(value: true);
			InGameUIManager.instance.SetState("Playing");
			ManagerBase<CameraManager>.instance.isPlayingCutscene = false;
			ManagerBase<CameraManager>.instance.currentCutsceneDirector = null;
			yield return new WaitForSeconds(ManagerBase<CameraManager>.instance.cutsceneFadeTime);
			yield return new WaitForSeconds(reenableAiDelay);
			if (base.isServer && disableAi)
			{
				EntityAI.DisableAI = false;
			}
			onFinish?.Invoke();
		}
	}

	[ClientRpc]
	public void PlayNetworked()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void DewCutsceneDirector::PlayNetworked()", 1583074866, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (ManagerBase<CameraManager>.instance != null && ManagerBase<CameraManager>.instance.currentCutsceneDirector == this)
		{
			ManagerBase<CameraManager>.instance.isPlayingCutscene = false;
			ManagerBase<CameraManager>.instance.currentCutsceneDirector = null;
		}
		if (NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.onEntityAdd -= new Action<Entity>(OnEntityAdd);
		}
	}

	private void OnEntityAdd(Entity obj)
	{
		if (base.isServer && obj is BossMonster)
		{
			obj.Visual.SkipSpawning();
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdSkip()
	{
		if (ManagerBase<CameraManager>.instance.isPlayingCutscene && !(ManagerBase<CameraManager>.instance.currentCutsceneDirector != this) && _director.state == PlayState.Playing)
		{
			RpcSkip();
		}
	}

	protected static void InvokeUserCode_CmdSkip(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSkip called on client.");
		}
		else
		{
			((DewCutsceneDirector)obj).UserCode_CmdSkip();
		}
	}

	protected void UserCode_RpcSkip()
	{
		if (ManagerBase<CameraManager>.instance.isPlayingCutscene && !(ManagerBase<CameraManager>.instance.currentCutsceneDirector != this) && _director.state == PlayState.Playing && !(_director.time >= _director.duration - 0.10000000149011612 - (double)ManagerBase<CameraManager>.instance.cutsceneFadeTime))
		{
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			_didSkip = true;
			ManagerBase<CameraManager>.instance.DoCutsceneFadeOut();
			yield return new WaitForSeconds(ManagerBase<CameraManager>.instance.cutsceneFadeTime);
			if (base.isServer)
			{
				foreach (Entity e in NetworkedManagerBase<ActorManager>.instance.allEntities)
				{
					if (e.Visual.isSpawning)
					{
						e.Visual.SkipSpawning();
					}
				}
				foreach (KeyValuePair<SpawnMonsterSettings, Coroutine> ongoingSpawn in SingletonDewNetworkBehaviour<Room>.instance.monsters.ongoingSpawns)
				{
					ongoingSpawn.Key.isCutsceneSkipped = true;
				}
			}
			if (_director.state == PlayState.Playing)
			{
				_director.time = _director.duration - 0.009999999776482582;
			}
		}
	}

	protected static void InvokeUserCode_RpcSkip(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSkip called on server.");
		}
		else
		{
			((DewCutsceneDirector)obj).UserCode_RpcSkip();
		}
	}

	protected void UserCode_PlayNetworked()
	{
		Play();
	}

	protected static void InvokeUserCode_PlayNetworked(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC PlayNetworked called on server.");
		}
		else
		{
			((DewCutsceneDirector)obj).UserCode_PlayNetworked();
		}
	}

	static DewCutsceneDirector()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(DewCutsceneDirector), "System.Void DewCutsceneDirector::CmdSkip()", InvokeUserCode_CmdSkip, requiresAuthority: false);
		RemoteProcedureCalls.RegisterRpc(typeof(DewCutsceneDirector), "System.Void DewCutsceneDirector::RpcSkip()", InvokeUserCode_RpcSkip);
		RemoteProcedureCalls.RegisterRpc(typeof(DewCutsceneDirector), "System.Void DewCutsceneDirector::PlayNetworked()", InvokeUserCode_PlayNetworked);
	}
}
