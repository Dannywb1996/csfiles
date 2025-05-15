using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class TimescaleManager : NetworkedManagerBase<TimescaleManager>
{
	private class TimescaleChange
	{
		public float target;

		public float initDuration;

		public float remainingDuration;
	}

	private float _desiredTimescale = 1f;

	[SyncVar(hook = "TimescaleChanged")]
	private float _finalTimescale = 1f;

	private List<AudioSource> _pausedSources = new List<AudioSource>();

	[CompilerGenerated]
	[SyncVar]
	private float _003CeffectTimescale_003Ek__BackingField;

	private List<TimescaleChange> _ongoingTimescaleChanges = new List<TimescaleChange>();

	private TimescaleChange _menuSlowTime;

	public float desiredTimescale
	{
		get
		{
			return _desiredTimescale;
		}
		set
		{
			_desiredTimescale = value;
		}
	}

	public bool shouldTimeBeSlowedBySpecialMenu { get; internal set; }

	public float effectTimescale
	{
		[CompilerGenerated]
		get
		{
			return _003CeffectTimescale_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CeffectTimescale_003Ek__BackingField = value;
		}
	} = 1f;

	public float Network_finalTimescale
	{
		get
		{
			return _finalTimescale;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _finalTimescale, 1uL, TimescaleChanged);
		}
	}

	public float Network_003CeffectTimescale_003Ek__BackingField
	{
		get
		{
			return effectTimescale;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref effectTimescale, 2uL, null);
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		_menuSlowTime = new TimescaleChange
		{
			target = 1f,
			remainingDuration = float.PositiveInfinity,
			initDuration = float.PositiveInfinity
		};
		_ongoingTimescaleChanges.Add(_menuSlowTime);
	}

	public void TimescaleChanged(float oldVal, float newVal)
	{
		Time.timeScale = newVal;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (!NetworkServer.active)
		{
			return;
		}
		if (shouldTimeBeSlowedBySpecialMenu)
		{
			_menuSlowTime.target = 0.025f;
		}
		else
		{
			_menuSlowTime.target = 1f;
		}
		Network_003CeffectTimescale_003Ek__BackingField = 1f;
		for (int i = _ongoingTimescaleChanges.Count - 1; i >= 0; i--)
		{
			TimescaleChange c = _ongoingTimescaleChanges[i];
			c.remainingDuration -= Time.unscaledDeltaTime;
			if (c.remainingDuration < 0f)
			{
				c.remainingDuration = 0f;
				_ongoingTimescaleChanges.RemoveAt(i);
			}
			else
			{
				Network_003CeffectTimescale_003Ek__BackingField = Mathf.Min(effectTimescale, c.target);
			}
		}
		Network_finalTimescale = _desiredTimescale * effectTimescale;
		if (InGameUIManager.softInstance == null)
		{
			Time.timeScale = _finalTimescale;
		}
		else if (NetworkServer.connections.Count <= 1 && InGameUIManager.instance.IsState("Menu"))
		{
			if (!(Time.timeScale > 0f))
			{
				return;
			}
			Time.timeScale = 0f;
			AudioSource[] array = UnityEngine.Object.FindObjectsOfType<AudioSource>();
			foreach (AudioSource a in array)
			{
				if (a.isPlaying)
				{
					_pausedSources.Add(a);
					a.Pause();
				}
			}
		}
		else
		{
			if (!(Math.Abs(Time.timeScale - _finalTimescale) > 0.0001f))
			{
				return;
			}
			Time.timeScale = _finalTimescale;
			foreach (AudioSource a2 in _pausedSources)
			{
				if (a2 != null)
				{
					a2.UnPause();
				}
			}
			_pausedSources.Clear();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Time.timeScale = 1f;
	}

	[Server]
	public void ChangeTimescale(float timescale, float duration)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void TimescaleManager::ChangeTimescale(System.Single,System.Single)' called when server was not active");
			return;
		}
		_ongoingTimescaleChanges.Add(new TimescaleChange
		{
			target = timescale,
			remainingDuration = duration,
			initDuration = duration
		});
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(_finalTimescale);
			writer.WriteFloat(effectTimescale);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteFloat(_finalTimescale);
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteFloat(effectTimescale);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref _finalTimescale, TimescaleChanged, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref effectTimescale, null, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _finalTimescale, TimescaleChanged, reader.ReadFloat());
		}
		if ((num & 2L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref effectTimescale, null, reader.ReadFloat());
		}
	}
}
