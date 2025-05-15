using System;
using UnityEngine;

public class EffectAutoDestroy : MonoBehaviour
{
	private const float AutoDestroyGracePeriod = 0.1f;

	private const float Timeout = 90f;

	private ParticleSystem _particleSystem;

	private IEffectComponent[] _fxComponents;

	private float _startTime;

	protected virtual void Awake()
	{
		_startTime = Time.time;
		_particleSystem = GetComponent<ParticleSystem>();
		if (_particleSystem == null)
		{
			_particleSystem = base.gameObject.AddComponent<ParticleSystem>();
			ParticleSystem.EmissionModule em = _particleSystem.emission;
			em.enabled = false;
			ParticleSystem.MainModule main = _particleSystem.main;
			main.loop = false;
		}
		_fxComponents = GetComponentsInChildren<IEffectComponent>();
		if (NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += new Action<EventInfoLoadRoom>(OnRoomChange);
		}
	}

	private void OnDestroy()
	{
		if (NetworkedManagerBase<ZoneManager>.instance != null)
		{
			NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded -= new Action<EventInfoLoadRoom>(OnRoomChange);
		}
	}

	protected virtual void Update()
	{
		if (Time.time - _startTime > 90f)
		{
			string culprits = "";
			IEffectComponent[] fxComponents = _fxComponents;
			foreach (IEffectComponent c in fxComponents)
			{
				if (c.isPlaying)
				{
					culprits = ((!(c is Component comp)) ? (culprits + "Unknown (???) ") : (culprits + comp.name + " (" + comp.GetType().Name + ") "));
				}
			}
			if (_particleSystem != null && _particleSystem.IsAlive())
			{
				culprits = culprits + _particleSystem.name + " (ParticleSystem)";
			}
			Debug.LogWarning($"Effect '{base.transform.GetScenePath()}' was timed out ({90f} seconds) waiting for: {culprits}");
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			if (Time.time - _startTime < 0.1f)
			{
				return;
			}
			if (NetworkedManagerBase<ZoneManager>.softInstance != null && NetworkedManagerBase<ZoneManager>.softInstance.isInRoomTransition)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				if (_particleSystem != null && _particleSystem.IsAlive())
				{
					return;
				}
				IEffectComponent[] fxComponents = _fxComponents;
				for (int i = 0; i < fxComponents.Length; i++)
				{
					if (fxComponents[i].isPlaying)
					{
						return;
					}
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	private void OnRoomChange(EventInfoLoadRoom _)
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
