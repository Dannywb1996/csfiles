using System;
using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_UsingShortcut : StatusEffect
{
	public float speedMultiplier;

	public float maxFadeDuration;

	public float delay;

	public GameObject dissolveEffect;

	[NonSerialized]
	public Room_Shortcut startShortcut;

	[NonSerialized]
	public Room_Shortcut targetShortcut;

	protected override IEnumerator OnCreateSequenced()
	{
		if (base.isServer)
		{
			DoInvulnerable();
			DoUncollidable();
			DoInvisible();
			base.victim.Control.forceWalking = true;
			float speed = base.victim.Control.baseAgentSpeed * speedMultiplier;
			float firstDuration = Vector2.Distance(base.victim.position.ToXY(), startShortcut.walkStartPos.position.ToXY()) / speed;
			base.victim.Control.StartDaze(firstDuration);
			base.victim.Control.StartDisplacement(new DispByDestination
			{
				destination = startShortcut.walkStartPos.transform.position,
				canGoOverTerrain = true,
				duration = firstDuration,
				ease = DewEase.Linear,
				isFriendly = true,
				rotateForward = true,
				isCanceledByCC = false,
				rotateSmoothly = true
			});
			yield return new SI.WaitForSeconds(firstDuration);
			float secondDuration = Vector2.Distance(base.victim.position.ToXY(), startShortcut.walkEndPos.position.ToXY()) / speed;
			base.victim.Control.StartDaze(secondDuration);
			base.victim.Control.StartDisplacement(new DispByDestination
			{
				destination = startShortcut.walkEndPos.transform.position,
				canGoOverTerrain = true,
				duration = secondDuration,
				ease = DewEase.Linear,
				isFriendly = true,
				rotateForward = true,
				isCanceledByCC = false,
				rotateSmoothly = true
			});
			FxPlayNetworked(dissolveEffect, base.victim);
			TpcSetFadeStatus(base.victim.owner, value: true);
			TpcSetTransitionStatus(base.victim.owner, value: true);
			yield return new SI.WaitForSeconds(Mathf.Min(maxFadeDuration, secondDuration));
			base.victim.Control.StartDaze(delay);
			yield return new SI.WaitForSeconds(delay * 0.5f);
			Teleport(base.victim, targetShortcut.walkEndPos.position);
			TpcSetCameraPos(base.victim.owner, targetShortcut.walkStartPos.position);
			targetShortcut.Open();
			yield return new SI.WaitForSeconds(delay * 0.5f);
			FxStopNetworked(dissolveEffect);
			TpcSetFadeStatus(base.victim.owner, value: false);
			float thirdDuration = Vector2.Distance(base.victim.position.ToXY(), targetShortcut.walkStartPos.position.ToXY()) / speed;
			base.victim.Control.StartDaze(thirdDuration);
			base.victim.Control.StartDisplacement(new DispByDestination
			{
				destination = targetShortcut.walkStartPos.position,
				canGoOverTerrain = true,
				duration = thirdDuration,
				ease = DewEase.Linear,
				isFriendly = true,
				rotateForward = true,
				isCanceledByCC = false,
				rotateSmoothly = true
			});
			yield return new SI.WaitForSeconds(thirdDuration);
			base.victim.Control.forceWalking = false;
			TpcSetTransitionStatus(base.victim.owner, value: false);
			Destroy();
		}
	}

	[TargetRpc]
	private void TpcSetCameraPos(NetworkConnectionToClient target, Vector3 pos)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(pos);
		SendTargetRPCInternal(target, "System.Void Se_UsingShortcut::TpcSetCameraPos(Mirror.NetworkConnectionToClient,UnityEngine.Vector3)", -507826236, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[TargetRpc]
	private void TpcSetFadeStatus(NetworkConnectionToClient target, bool value)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(value);
		SendTargetRPCInternal(target, "System.Void Se_UsingShortcut::TpcSetFadeStatus(Mirror.NetworkConnectionToClient,System.Boolean)", -1238627915, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[TargetRpc]
	private void TpcSetTransitionStatus(NetworkConnectionToClient target, bool value)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(value);
		SendTargetRPCInternal(target, "System.Void Se_UsingShortcut::TpcSetTransitionStatus(Mirror.NetworkConnectionToClient,System.Boolean)", 1374971836, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer)
		{
			TpcSetTransitionStatus(base.victim.owner, value: false);
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcSetCameraPos__NetworkConnectionToClient__Vector3(NetworkConnectionToClient target, Vector3 pos)
	{
		ManagerBase<CameraManager>.instance.SetCameraPosition(pos);
	}

	protected static void InvokeUserCode_TpcSetCameraPos__NetworkConnectionToClient__Vector3(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcSetCameraPos called on server.");
		}
		else
		{
			((Se_UsingShortcut)obj).UserCode_TpcSetCameraPos__NetworkConnectionToClient__Vector3((NetworkConnectionToClient)NetworkClient.connection, reader.ReadVector3());
		}
	}

	protected void UserCode_TpcSetFadeStatus__NetworkConnectionToClient__Boolean(NetworkConnectionToClient target, bool value)
	{
		if (value)
		{
			ManagerBase<TransitionManager>.instance.FadeOut(showTips: false);
		}
		else
		{
			ManagerBase<TransitionManager>.instance.FadeIn();
		}
	}

	protected static void InvokeUserCode_TpcSetFadeStatus__NetworkConnectionToClient__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcSetFadeStatus called on server.");
		}
		else
		{
			((Se_UsingShortcut)obj).UserCode_TpcSetFadeStatus__NetworkConnectionToClient__Boolean((NetworkConnectionToClient)NetworkClient.connection, reader.ReadBool());
		}
	}

	protected void UserCode_TpcSetTransitionStatus__NetworkConnectionToClient__Boolean(NetworkConnectionToClient target, bool value)
	{
		NetworkedManagerBase<ZoneManager>.instance.isInLocalTransition = value;
	}

	protected static void InvokeUserCode_TpcSetTransitionStatus__NetworkConnectionToClient__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcSetTransitionStatus called on server.");
		}
		else
		{
			((Se_UsingShortcut)obj).UserCode_TpcSetTransitionStatus__NetworkConnectionToClient__Boolean((NetworkConnectionToClient)NetworkClient.connection, reader.ReadBool());
		}
	}

	static Se_UsingShortcut()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Se_UsingShortcut), "System.Void Se_UsingShortcut::TpcSetCameraPos(Mirror.NetworkConnectionToClient,UnityEngine.Vector3)", InvokeUserCode_TpcSetCameraPos__NetworkConnectionToClient__Vector3);
		RemoteProcedureCalls.RegisterRpc(typeof(Se_UsingShortcut), "System.Void Se_UsingShortcut::TpcSetFadeStatus(Mirror.NetworkConnectionToClient,System.Boolean)", InvokeUserCode_TpcSetFadeStatus__NetworkConnectionToClient__Boolean);
		RemoteProcedureCalls.RegisterRpc(typeof(Se_UsingShortcut), "System.Void Se_UsingShortcut::TpcSetTransitionStatus(Mirror.NetworkConnectionToClient,System.Boolean)", InvokeUserCode_TpcSetTransitionStatus__NetworkConnectionToClient__Boolean);
	}
}
