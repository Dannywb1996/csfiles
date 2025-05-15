using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using TMPro;
using UnityEngine;

public class Mon_RedGiant : Monster
{
	public GameObject deathEffects;

	public TextMeshPro dpsText;

	[SyncVar]
	private float _totalDamage;

	[SyncVar]
	private float _dps;

	[SyncVar]
	private float _maxSingleDmg;

	private float _lastDamageTime;

	private Queue<(float, float)> _damages = new Queue<(float, float)>();

	public float Network_totalDamage
	{
		get
		{
			return _totalDamage;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _totalDamage, 128uL, null);
		}
	}

	public float Network_dps
	{
		get
		{
			return _dps;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _dps, 256uL, null);
		}
	}

	public float Network_maxSingleDmg
	{
		get
		{
			return _maxSingleDmg;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _maxSingleDmg, 512uL, null);
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		EntityEvent_OnTakeDamage += (Action<EventInfoDamage>)delegate(EventInfoDamage info)
		{
			float num = info.damage.amount + info.damage.discardedAmount;
			_lastDamageTime = Time.time;
			Network_maxSingleDmg = Mathf.Max(_maxSingleDmg, num);
			Network_totalDamage = _totalDamage + num;
			_damages.Enqueue((Time.time, num));
		};
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (base.isServer)
		{
			if (Time.time - _lastDamageTime > 5f)
			{
				Network_totalDamage = 0f;
				Network_maxSingleDmg = 0f;
			}
			(float, float) res;
			while (_damages.TryPeek(out res) && Time.time - res.Item1 > 3f)
			{
				_damages.Dequeue();
			}
			float sum = 0f;
			foreach (var damage in _damages)
			{
				sum += damage.Item2;
			}
			Network_dps = sum / 3f;
		}
		dpsText.text = $"Total: {_totalDamage:#,##0}\r\nDPS: {_dps:#,##0}\r\nMax Single Dmg: {_maxSingleDmg:#,##0}";
	}

	protected override void AIUpdate(ref EntityAIContext context)
	{
		if (context.targetEnemy != null)
		{
			base.Control.RotateTowards(context.targetEnemy.transform.position, immediately: false);
		}
	}

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		base.Visual.DisableRenderersLocal();
		Quaternion rotation = base.transform.rotation;
		if (info.actor != null && info.actor.firstEntity != null)
		{
			rotation = Quaternion.LookRotation(base.transform.position - info.actor.firstEntity.transform.position).Flattened();
		}
		deathEffects.transform.rotation = rotation;
		deathEffects.transform.parent = null;
		deathEffects.SetActive(value: true);
		Rigidbody[] componentsInChildren = deathEffects.GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody obj in componentsInChildren)
		{
			obj.AddForce(UnityEngine.Random.Range(0f, 1f) * deathEffects.transform.forward, ForceMode.VelocityChange);
			obj.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.1f, 0.2f);
			obj.transform.rotation = UnityEngine.Random.rotation;
		}
		UnityEngine.Object.Destroy(deathEffects, 3f);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(_totalDamage);
			writer.WriteFloat(_dps);
			writer.WriteFloat(_maxSingleDmg);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x80L) != 0L)
		{
			writer.WriteFloat(_totalDamage);
		}
		if ((base.syncVarDirtyBits & 0x100L) != 0L)
		{
			writer.WriteFloat(_dps);
		}
		if ((base.syncVarDirtyBits & 0x200L) != 0L)
		{
			writer.WriteFloat(_maxSingleDmg);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref _totalDamage, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref _dps, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref _maxSingleDmg, null, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x80L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _totalDamage, null, reader.ReadFloat());
		}
		if ((num & 0x100L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _dps, null, reader.ReadFloat());
		}
		if ((num & 0x200L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _maxSingleDmg, null, reader.ReadFloat());
		}
	}
}
