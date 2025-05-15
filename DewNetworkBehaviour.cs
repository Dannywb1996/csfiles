using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class DewNetworkBehaviour : NetworkBehaviour, ILogicUpdate
{
	private bool _didRegisterLogic;

	protected virtual void Awake()
	{
		syncInterval = 0.01f;
		syncMode = SyncMode.Observers;
	}

	private IEnumerator RoutineDelayedExecution(Action method)
	{
		yield return new WaitForEndOfFrame();
		method();
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		try
		{
			OnStart();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
		StartCoroutine(RoutineDelayedExecution(OnLateStartServer));
		StartCoroutine(RoutineDelayedExecution(OnLateStart));
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		if (base.isServer)
		{
			return;
		}
		try
		{
			OnStart();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
		StartCoroutine(RoutineDelayedExecution(OnLateStart));
		try
		{
			DewNetworkManager.instance.onDewNetworkBehaviourStart?.Invoke(this);
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		try
		{
			OnStop();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		if (base.isServer)
		{
			return;
		}
		try
		{
			OnStop();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
		try
		{
			DewNetworkManager.instance.onDewNetworkBehaviourStop?.Invoke(this);
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	public virtual void OnStart()
	{
		LogicUpdateManager.Register(this);
		_didRegisterLogic = true;
	}

	public virtual void OnLateStart()
	{
	}

	public virtual void OnLateStartServer()
	{
	}

	public virtual void OnStop()
	{
		if (_didRegisterLogic)
		{
			LogicUpdateManager.Unregister(this);
			_didRegisterLogic = false;
		}
	}

	protected virtual void OnDestroy()
	{
		if (_didRegisterLogic)
		{
			LogicUpdateManager.Unregister(this);
			_didRegisterLogic = false;
		}
	}

	public virtual void LogicUpdate(float dt)
	{
	}

	public virtual void FrameUpdate()
	{
	}

	protected void GetComponent<T>(out T comp)
	{
		comp = GetComponent<T>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayNewNetworked(GameObject effect, Vector3 position, Quaternion? rotation)
	{
		DewEffect.PlayNewNetworked(base.netIdentity, effect, position, rotation);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayNewNetworked(GameObject effect, Entity entity)
	{
		DewEffect.PlayNewNetworked(base.netIdentity, effect, entity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayNewNetworked(GameObject effect)
	{
		DewEffect.PlayNewNetworked(base.netIdentity, effect);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayNetworked(GameObject effect)
	{
		DewEffect.PlayNetworked(base.netIdentity, effect);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayNetworked(GameObject effect, Vector3 position, Quaternion? rotation)
	{
		DewEffect.PlayNetworked(base.netIdentity, effect, position, rotation);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayNetworked(GameObject effect, Entity entity)
	{
		DewEffect.PlayNetworked(base.netIdentity, effect, entity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxStopNetworked(GameObject effect)
	{
		DewEffect.StopNetworked(base.netIdentity, effect);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayNew(GameObject effect)
	{
		DewEffect.PlayNew(effect, base.netIdentity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayNew(GameObject effect, Vector3 position, Quaternion? rotation)
	{
		DewEffect.PlayNew(effect, position, rotation, base.netIdentity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlayNew(GameObject effect, Entity attach)
	{
		DewEffect.PlayNew(effect, attach, base.netIdentity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlay(GameObject effect)
	{
		DewEffect.Play(effect, base.netIdentity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlay(GameObject effect, Vector3 position, Quaternion? rotation)
	{
		DewEffect.Play(effect, position, rotation, base.netIdentity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxPlay(GameObject effect, Entity attach)
	{
		DewEffect.Play(effect, attach, base.netIdentity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FxStop(GameObject effect)
	{
		DewEffect.Stop(effect);
	}

	private void MirrorProcessed()
	{
	}
}
