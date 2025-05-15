using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public abstract class StackedStatusEffect : StatusEffect
{
	public int maxStack = 5;

	public bool killOnZeroStack = true;

	public bool autoDecay = true;

	public float decayTime = 5f;

	public bool decayAllAtOnce = true;

	public int decayCount = 1;

	public bool resetTimerOnStackChange = true;

	[SyncVar(hook = "OnStackChange")]
	[SerializeField]
	private int _stack = 1;

	[SyncVar]
	private float _lastStackDecayTime;

	public int stack => _stack;

	public float remainingDecayTime
	{
		get
		{
			if (!autoDecay)
			{
				return 0f;
			}
			return decayTime - (float)NetworkTime.time + _lastStackDecayTime;
		}
	}

	public int Network_stack
	{
		get
		{
			return _stack;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _stack, 512uL, OnStackChange);
		}
	}

	public float Network_lastStackDecayTime
	{
		get
		{
			return _lastStackDecayTime;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _lastStackDecayTime, 1024uL, null);
		}
	}

	public void SetStack(int stack)
	{
		if (autoDecay && resetTimerOnStackChange)
		{
			ResetDecayTimer();
		}
		Network_stack = Mathf.Clamp(stack, 0, maxStack);
	}

	public void AddStack(int value = 1)
	{
		if (autoDecay && resetTimerOnStackChange)
		{
			ResetDecayTimer();
		}
		if (value < 0)
		{
			Debug.LogWarning(string.Format("{0} parameter out of range: {1}", "AddStack", value));
		}
		else
		{
			Network_stack = Mathf.Clamp(_stack + value, 0, maxStack);
		}
	}

	public void RemoveStack(int value = 1)
	{
		if (autoDecay && resetTimerOnStackChange)
		{
			ResetDecayTimer();
		}
		if (value < 0)
		{
			Debug.LogWarning(string.Format("{0} parameter out of range: {1}", "RemoveStack", value));
		}
		else
		{
			Network_stack = Mathf.Clamp(_stack - value, 0, maxStack);
		}
	}

	protected virtual void OnStackChange(int oldStack, int newStack)
	{
		if (base.isServer && killOnZeroStack && newStack == 0 && base.isActive)
		{
			Destroy();
		}
	}

	public void ResetDecayTimer()
	{
		if (!autoDecay)
		{
			Debug.LogWarning($"Tried to reset decay timer of non-decaying StatusEffect: {this}");
		}
		else
		{
			Network_lastStackDecayTime = (float)NetworkTime.time;
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		Network_lastStackDecayTime = (float)NetworkTime.time;
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_stack != 0)
		{
			Network_stack = 0;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (!base.isServer || !autoDecay)
		{
			return;
		}
		if ((float)NetworkTime.time - _lastStackDecayTime >= decayTime)
		{
			if (decayAllAtOnce)
			{
				SetStack(0);
			}
			else
			{
				RemoveStack(decayCount);
			}
			ResetDecayTimer();
		}
		if (stack == 0)
		{
			ResetDecayTimer();
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
			writer.WriteInt(_stack);
			writer.WriteFloat(_lastStackDecayTime);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x200L) != 0L)
		{
			writer.WriteInt(_stack);
		}
		if ((base.syncVarDirtyBits & 0x400L) != 0L)
		{
			writer.WriteFloat(_lastStackDecayTime);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref _stack, OnStackChange, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref _lastStackDecayTime, null, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x200L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _stack, OnStackChange, reader.ReadInt());
		}
		if ((num & 0x400L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _lastStackDecayTime, null, reader.ReadFloat());
		}
	}
}
