using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cinemachine;
using Mirror;
using UnityEngine;
using UnityEngine.VFX;

public static class DewEffect
{
	public struct PlayEffectMessage : NetworkMessage
	{
		public bool isNew;

		public NetworkIdentity parent;

		public string path;
	}

	public struct PlayPositionedEffectMessage : NetworkMessage
	{
		public bool isNew;

		public NetworkIdentity parent;

		public string path;

		public Vector3 position;

		public Quaternion rotation;
	}

	public struct PlayAttachedEffectMessage : NetworkMessage
	{
		public bool isNew;

		public NetworkIdentity parent;

		public string path;

		public Entity entity;
	}

	public struct PlayCastEffectMessage : NetworkMessage
	{
		public NetworkIdentity parent;

		public string path;

		public CastInfo info;

		public CastMethodType method;

		public float duration;
	}

	public struct StopEffectMessage : NetworkMessage
	{
		public NetworkIdentity parent;

		public string path;
	}

	public static bool disablePlay;

	public static bool disablePlayNew;

	private static Dictionary<int, int> _effectsInThisFrame = new Dictionary<int, int>();

	private static int _effectsInThisFrameCount = 0;

	private static void EnsureSameFrame()
	{
		if (_effectsInThisFrameCount != Time.frameCount)
		{
			_effectsInThisFrame.Clear();
			_effectsInThisFrameCount = Time.frameCount;
		}
	}

	private static bool CheckLimit(GameObject effect, NetworkIdentity parent)
	{
		EnsureSameFrame();
		int code = HashCode.Combine(effect, parent);
		if (_effectsInThisFrame.TryGetValue(code, out var count))
		{
			if (count > 7)
			{
				return false;
			}
			_effectsInThisFrame[code] = count + 1;
		}
		else
		{
			_effectsInThisFrame[code] = 1;
		}
		return true;
	}

	private static bool CheckLimit(GameObject effect, Vector3 position, Quaternion rotation, NetworkIdentity parent)
	{
		EnsureSameFrame();
		int code = HashCode.Combine(effect, parent, Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z), Mathf.RoundToInt(rotation.eulerAngles.y));
		if (_effectsInThisFrame.TryGetValue(code, out var count))
		{
			if (count > 4)
			{
				return false;
			}
			_effectsInThisFrame[code] = count + 1;
		}
		else
		{
			_effectsInThisFrame[code] = 1;
		}
		return true;
	}

	private static bool CheckLimit(GameObject effect, Entity attach, NetworkIdentity parent)
	{
		EnsureSameFrame();
		int code = HashCode.Combine(effect, attach, parent);
		if (_effectsInThisFrame.TryGetValue(code, out var count))
		{
			if (count > 3)
			{
				return false;
			}
			_effectsInThisFrame[code] = count + 1;
		}
		else
		{
			_effectsInThisFrame[code] = 1;
		}
		if (!CheckLimit(effect, parent))
		{
			return false;
		}
		return true;
	}

	public static void PlayNewNetworked(NetworkIdentity parent, GameObject effect, Vector3 position, Quaternion? rotation)
	{
		ThrowIfServerNotActive();
		if (!(effect == null))
		{
			if (!rotation.HasValue)
			{
				rotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
			}
			NetworkServer.SendToAll(new PlayPositionedEffectMessage
			{
				isNew = true,
				parent = parent,
				path = GetPath(parent.transform, effect),
				position = position,
				rotation = rotation.Value
			});
		}
	}

	public static void PlayNewNetworked(NetworkIdentity parent, GameObject effect, Entity entity)
	{
		ThrowIfServerNotActive();
		if (!(effect == null))
		{
			NetworkServer.SendToAll(new PlayAttachedEffectMessage
			{
				isNew = true,
				parent = parent,
				path = GetPath(parent.transform, effect),
				entity = entity
			});
		}
	}

	public static void PlayNewNetworked(NetworkIdentity parent, GameObject effect)
	{
		ThrowIfServerNotActive();
		if (!(effect == null))
		{
			NetworkServer.SendToAll(new PlayEffectMessage
			{
				isNew = true,
				parent = parent,
				path = GetPath(parent.transform, effect)
			});
		}
	}

	public static void PlayNetworked(NetworkIdentity parent, GameObject effect)
	{
		ThrowIfServerNotActive();
		if (!(effect == null))
		{
			NetworkServer.SendToAll(new PlayEffectMessage
			{
				isNew = false,
				parent = parent,
				path = GetPath(parent.transform, effect)
			});
		}
	}

	public static void PlayNetworked(NetworkIdentity parent, GameObject effect, Entity entity)
	{
		ThrowIfServerNotActive();
		if (!(effect == null))
		{
			NetworkServer.SendToAll(new PlayAttachedEffectMessage
			{
				isNew = false,
				parent = parent,
				path = GetPath(parent.transform, effect),
				entity = entity
			});
		}
	}

	public static void PlayNetworked(NetworkIdentity parent, GameObject effect, Vector3 position, Quaternion? rotation)
	{
		ThrowIfServerNotActive();
		if (!(effect == null))
		{
			if (!rotation.HasValue)
			{
				rotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
			}
			NetworkServer.SendToAll(new PlayPositionedEffectMessage
			{
				isNew = false,
				parent = parent,
				path = GetPath(parent.transform, effect),
				position = position,
				rotation = rotation.Value
			});
		}
	}

	public static void PlayCastEffectNetworked(NetworkIdentity parent, GameObject effect, CastInfo info, CastMethodType method, float duration = -1f)
	{
		ThrowIfServerNotActive();
		if (!(effect == null))
		{
			NetworkServer.SendToAll(new PlayCastEffectMessage
			{
				parent = parent,
				path = GetPath(parent.transform, effect),
				info = info,
				method = method,
				duration = duration
			});
		}
	}

	public static void StopNetworked(NetworkIdentity parent, GameObject effect)
	{
		ThrowIfServerNotActive();
		if (!(effect == null))
		{
			NetworkServer.SendToAll(new StopEffectMessage
			{
				parent = parent,
				path = GetPath(parent.transform, effect)
			});
		}
	}

	internal static void HandleEffectMessage(PlayPositionedEffectMessage msg)
	{
		if (msg.parent == null)
		{
			Debug.Log("Effect '" + msg.path + "' parent not found");
			return;
		}
		GameObject effect = ResolvePath(msg.parent.transform, msg.path);
		if (effect == null)
		{
			Debug.Log("Effect '" + msg.parent.name + "/" + msg.path + "' not found");
		}
		else if (msg.isNew)
		{
			PlayNew(effect, msg.position, msg.rotation, msg.parent);
		}
		else
		{
			Play(effect, msg.position, msg.rotation, msg.parent);
		}
	}

	internal static void HandleEffectMessage(PlayAttachedEffectMessage msg)
	{
		if (msg.parent == null)
		{
			Debug.Log("Effect '" + msg.path + "' parent not found");
			return;
		}
		GameObject effect = ResolvePath(msg.parent.transform, msg.path);
		if (effect == null)
		{
			Debug.Log("Effect '" + msg.parent.name + "/" + msg.path + "' not found");
		}
		else if (msg.isNew)
		{
			PlayNew(effect, msg.entity, msg.parent);
		}
		else
		{
			Play(effect, msg.entity, msg.parent);
		}
	}

	internal static void HandleEffectMessage(PlayEffectMessage msg)
	{
		if (msg.parent == null)
		{
			Debug.Log("Effect '" + msg.path + "' parent not found");
			return;
		}
		GameObject effect = ResolvePath(msg.parent.transform, msg.path);
		if (effect == null)
		{
			Debug.Log("Effect '" + msg.parent.name + "/" + msg.path + "' not found");
		}
		else if (msg.isNew)
		{
			PlayNew(effect, msg.parent);
		}
		else
		{
			Play(effect, msg.parent);
		}
	}

	internal static void HandleEffectMessage(PlayCastEffectMessage msg)
	{
		if (msg.parent == null)
		{
			Debug.Log("Effect '" + msg.path + "' parent not found");
			return;
		}
		GameObject effect = ResolvePath(msg.parent.transform, msg.path);
		if (effect == null)
		{
			Debug.Log("Effect '" + msg.parent.name + "/" + msg.path + "' not found");
		}
		else
		{
			PlayCastEffect(effect, msg.info, msg.method, msg.duration, msg.parent);
		}
	}

	internal static void HandleEffectMessage(StopEffectMessage msg)
	{
		if (msg.parent == null)
		{
			Debug.Log("Effect '" + msg.path + "' parent not found");
			return;
		}
		GameObject effect = ResolvePath(msg.parent.transform, msg.path);
		if (effect == null)
		{
			Debug.Log("Effect '" + msg.parent.name + "/" + msg.path + "' not found");
		}
		else
		{
			Stop(effect);
		}
	}

	public static void PlayNew(GameObject effect, NetworkIdentity parent = null)
	{
		if (!disablePlayNew && !(effect == null) && CheckLimit(effect, parent))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(effect, effect.transform.position, effect.transform.rotation);
			Play(gameObject, parent);
			gameObject.AddComponent<EffectAutoDestroy>();
		}
	}

	public static void PlayNew(GameObject effect, Vector3 position, Quaternion? rotation, NetworkIdentity parent = null)
	{
		if (!disablePlayNew && !(effect == null))
		{
			if (!rotation.HasValue)
			{
				rotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
			}
			if (CheckLimit(effect, position, rotation.Value, parent))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(effect, position, rotation.Value);
				Play(gameObject, parent);
				gameObject.AddComponent<EffectAutoDestroy>();
			}
		}
	}

	public static void PlayNew(GameObject effect, Entity attach, NetworkIdentity parent = null)
	{
		if (!disablePlayNew && !(effect == null))
		{
			if (attach == null)
			{
				PlayNew(effect);
			}
			else if (CheckLimit(effect, attach, parent))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(effect, effect.transform.position, effect.transform.rotation);
				Play(gameObject, attach, parent);
				gameObject.AddComponent<EffectAutoDestroy>();
			}
		}
	}

	public static void Play(GameObject effect, NetworkIdentity parent = null)
	{
		if (effect == null || disablePlay)
		{
			return;
		}
		ListReturnHandle<Transform> handle;
		List<Transform> queue = DewPool.GetList(out handle);
		queue.Add(effect.transform);
		while (queue.Count > 0)
		{
			Transform cursor = queue[queue.Count - 1];
			queue.RemoveAt(queue.Count - 1);
			if (cursor.TryGetComponent<FxSelectiveVisibility>(out var vis))
			{
				if (vis.wasDisabled)
				{
					cursor.gameObject.SetActive(value: false);
					continue;
				}
				if (!vis.IsVisibleLocally())
				{
					cursor.gameObject.SetActive(value: false);
					continue;
				}
				cursor.gameObject.SetActive(value: true);
			}
			if (cursor.TryGetComponent<FxGameObject>(out var gobj))
			{
				gobj.Play();
			}
			if (!cursor.gameObject.activeInHierarchy)
			{
				continue;
			}
			ListReturnHandle<IEffectSetupComponent> h;
			foreach (IEffectSetupComponent s in ((Component)cursor).GetComponentsNonAlloc(out h))
			{
				try
				{
					s.OnEffectSetup();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			h.Return();
			foreach (Transform child in cursor)
			{
				queue.Add(child);
			}
		}
		handle.Return();
		PlayAllParticleSystems(effect.transform);
		PlayAllVisualEffects(effect.transform);
		ListReturnHandle<CinemachineImpulseSource> h2;
		List<CinemachineImpulseSource> impulses = effect.GetComponentsInChildrenNonAlloc(includeInactive: true, out h2);
		for (int i = 0; i < impulses.Count; i++)
		{
			if (impulses[i].isActiveAndEnabled)
			{
				impulses[i].GenerateImpulse();
			}
		}
		h2.Return();
		ListReturnHandle<IEffectComponent> h3;
		List<IEffectComponent> comps = effect.GetComponentsInChildrenNonAlloc(includeInactive: true, out h3);
		for (int j = 0; j < comps.Count; j++)
		{
			if (comps[j] is FxGameObject || (comps[j] is MonoBehaviour mb && (!mb.enabled || !mb.gameObject.activeInHierarchy)))
			{
				continue;
			}
			if (comps[j] is IEffectWithOwnerContext eoc)
			{
				Entity e = GetEntity();
				EffectOwnerContext context;
				if (e == null)
				{
					context = EffectOwnerContext.None;
				}
				else if (e.IsAnyBoss())
				{
					context = EffectOwnerContext.Boss;
				}
				else
				{
					DewPlayer currentPlayer = ((ManagerBase<CameraManager>.instance.focusedEntity != null) ? ManagerBase<CameraManager>.instance.focusedEntity.owner : DewPlayer.local);
					context = ((e.owner == currentPlayer) ? EffectOwnerContext.Self : ((!(e.owner != null) || !e.owner.isHumanPlayer) ? EffectOwnerContext.Others : EffectOwnerContext.OtherPlayers));
				}
				if (context == EffectOwnerContext.Self && FxSelectiveVisibility.forceFail)
				{
					context = EffectOwnerContext.OtherPlayers;
				}
				eoc.SetOwnerContext(context);
			}
			comps[j].Play();
		}
		h3.Return();
		Entity GetEntity()
		{
			if (parent == null)
			{
				return null;
			}
			if (!parent.fxDidCacheEntity)
			{
				parent.fxDidCacheEntity = true;
				Actor actor = parent.GetComponent<Actor>();
				if (actor != null)
				{
					if (actor is Entity e2)
					{
						parent.fxEntity = e2;
					}
					else
					{
						parent.fxEntity = actor.firstEntity;
					}
				}
			}
			return parent.fxEntity as Entity;
		}
	}

	public static void Play(GameObject effect, Vector3 position, Quaternion? rotation, NetworkIdentity parent = null)
	{
		if (!(effect == null))
		{
			if (!rotation.HasValue)
			{
				rotation = ManagerBase<CameraManager>.instance.entityCamAngleRotation;
			}
			effect.transform.SetPositionAndRotation(position, rotation.Value);
			Play(effect, parent);
		}
	}

	public static void Play(GameObject effect, Entity attach, NetworkIdentity parent = null)
	{
		if (effect == null)
		{
			return;
		}
		if (attach == null)
		{
			Play(effect);
			return;
		}
		ListReturnHandle<IAttachableToEntity> handle;
		foreach (IAttachableToEntity item in effect.GetComponentsInChildrenNonAlloc(out handle))
		{
			item.OnAttachToEntity(attach);
		}
		handle.Return();
		Play(effect, parent);
	}

	public static void PlayCastEffect(GameObject effect, CastInfo info, CastMethodType method, float duration = -1f, NetworkIdentity parent = null)
	{
		if (!(effect == null))
		{
			IAttachableToEntity[] componentsInChildren = effect.GetComponentsInChildren<IAttachableToEntity>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].OnAttachToEntity(info.caster);
			}
			FxCastTelegraph[] componentsInChildren2 = effect.GetComponentsInChildren<FxCastTelegraph>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].Setup(method, info, duration);
			}
			Play(effect, parent);
		}
	}

	public static void Stop(GameObject effect)
	{
		if (!(effect == null))
		{
			StopAllParticleSystems(effect.transform);
			StopAllVisualEffects(effect.transform);
			IEffectComponent[] comps = effect.GetComponentsInChildren<IEffectComponent>(includeInactive: true);
			for (int i = 0; i < comps.Length; i++)
			{
				comps[i].Stop();
			}
		}
	}

	internal static void RegisterHandlers()
	{
		NetworkClient.RegisterHandler<PlayEffectMessage>(HandleEffectMessage);
		NetworkClient.RegisterHandler<PlayPositionedEffectMessage>(HandleEffectMessage);
		NetworkClient.RegisterHandler<PlayAttachedEffectMessage>(HandleEffectMessage);
		NetworkClient.RegisterHandler<StopEffectMessage>(HandleEffectMessage);
		NetworkClient.RegisterHandler<PlayCastEffectMessage>(HandleEffectMessage);
	}

	private static string GetPath(Transform parent, GameObject target)
	{
		int iter = 0;
		string relName = "";
		Transform cursor = target.transform;
		while (true)
		{
			iter++;
			if (iter > 100)
			{
				Debug.LogError($"Cannot get relative name, parent tree of '{target}' is too deep!");
				return null;
			}
			if (cursor == null)
			{
				Debug.LogError($"Cannot get relative name, maybe '{target}' is not parent of '{parent.name}'?");
				return null;
			}
			if (cursor == parent)
			{
				return "/" + relName;
			}
			if (cursor.name.Contains("/"))
			{
				break;
			}
			relName = cursor.name + "/" + relName;
			cursor = cursor.parent;
		}
		Debug.LogError("Cannot get relative name, name contains '/' character!");
		return null;
	}

	private static GameObject ResolvePath(Transform parent, string path)
	{
		if (path == "/")
		{
			return parent.gameObject;
		}
		string subName = path.Substring(1);
		Transform tr = parent.Find(subName);
		if (tr == null)
		{
			Debug.LogError("Cannot resolve relative name: " + subName);
			return null;
		}
		return tr.gameObject;
	}

	private static void PlayAllVisualEffects(Transform transform)
	{
		ListReturnHandle<VisualEffect> handle;
		foreach (VisualEffect e in ((Component)transform).GetComponentsInChildrenNonAlloc(out handle))
		{
			if (e.isActiveAndEnabled)
			{
				e.Play();
			}
		}
		handle.Return();
	}

	private static void StopAllVisualEffects(Transform transform)
	{
		VisualEffect[] componentsInChildren = transform.GetComponentsInChildren<VisualEffect>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Stop();
		}
	}

	private static void PlayAllParticleSystems(Transform transform)
	{
		if (!transform.gameObject.activeInHierarchy)
		{
			return;
		}
		if (transform.TryGetComponent<ParticleSystem>(out var ps))
		{
			ps.Play();
			return;
		}
		foreach (Transform item in transform)
		{
			PlayAllParticleSystems(item);
		}
	}

	private static void StopAllParticleSystems(Transform transform)
	{
		if (transform.TryGetComponent<ParticleSystem>(out var ps))
		{
			ps.Stop();
			ParticleSystem[] componentsInChildren = transform.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem p in componentsInChildren)
			{
				if (p.TryGetComponent<FxParticleSystem>(out var data))
				{
					if (data.clearParticlesOnStop == FxParticleSystem.ClearParticlesBehavior.ClearSelf)
					{
						p.Clear(withChildren: false);
					}
					else if (data.clearParticlesOnStop == FxParticleSystem.ClearParticlesBehavior.ClearWithChildren)
					{
						p.Clear(withChildren: true);
					}
				}
			}
			return;
		}
		foreach (Transform item in transform)
		{
			StopAllParticleSystems(item);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ThrowIfServerNotActive()
	{
		if (!NetworkServer.active)
		{
			throw new InvalidOperationException("You can only play networked effects on server");
		}
	}

	public static void TintRecursively(GameObject gameObject, Color color)
	{
		ListReturnHandle<Component> handle0;
		foreach (Component item in gameObject.GetComponentsInChildrenNonAlloc(includeInactive: true, out handle0))
		{
			TintObject(item, color, dontLogWarning: true);
		}
		handle0.Return();
	}

	public static void TintObject(object obj, Color color, bool dontLogWarning = false)
	{
		color.a = 1f;
		if (obj is ParticleSystem ps)
		{
			ps.TintMainColor(color);
		}
		else if (obj is Light l)
		{
			l.color *= color;
		}
		else if (obj is FxEntityColor ec)
		{
			ec.baseColor *= color;
			ec.emission *= color;
		}
		else if (!dontLogWarning)
		{
			Debug.LogWarning($"{obj.GetType()} is not tint-able: {obj}");
		}
	}
}
