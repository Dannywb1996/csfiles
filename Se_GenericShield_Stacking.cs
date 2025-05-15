using System;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Se_GenericShield_Stacking : StatusEffect
{
	public GameObject fxEffect;

	[NonSerialized]
	public ShieldEffect shield = new ShieldEffect();

	[NonSerialized]
	public float timeout = 3f;

	[SyncVar(hook = "OnShouldShowEffectChanged")]
	private bool _shouldShowEffect;

	private float _nextExpirationTime;

	public bool Network_shouldShowEffect
	{
		get
		{
			return _shouldShowEffect;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _shouldShowEffect, 512uL, OnShouldShowEffectChanged);
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			ShieldEffect shieldEffect = shield;
			shieldEffect.onAmountModified = (Action<float, float>)Delegate.Combine(shieldEffect.onAmountModified, new Action<float, float>(OnAmountModified));
			DoBasicEffect(shield);
			_nextExpirationTime = Time.time + timeout;
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (base.isServer && Time.time > _nextExpirationTime)
		{
			_nextExpirationTime = float.PositiveInfinity;
			shield.amount = 0f;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer)
		{
			UpdateShowStatus();
		}
	}

	private void OnAmountModified(float arg1, float arg2)
	{
		UpdateShowStatus();
	}

	private void OnShouldShowEffectChanged(bool from, bool to)
	{
		if (to)
		{
			FxPlay(fxEffect, base.victim);
		}
		else
		{
			FxStop(fxEffect);
		}
	}

	[Server]
	private void UpdateShowStatus()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Se_GenericShield_Stacking::UpdateShowStatus()' called when server was not active");
		}
		else
		{
			Network_shouldShowEffect = base.isActive && shield.amount > 0.0001f;
		}
	}

	[Server]
	public void AddAmount(float amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Se_GenericShield_Stacking::AddAmount(System.Single)' called when server was not active");
			return;
		}
		amount = ProcessShieldAmount(amount, base.victim);
		shield.amount += amount;
		_nextExpirationTime = Time.time + timeout;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(_shouldShowEffect);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x200L) != 0L)
		{
			writer.WriteBool(_shouldShowEffect);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref _shouldShowEffect, OnShouldShowEffectChanged, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x200L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _shouldShowEffect, OnShouldShowEffectChanged, reader.ReadBool());
		}
	}
}
