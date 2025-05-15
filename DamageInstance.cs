using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public abstract class DamageInstance : AbilityInstance
{
	private class Ad_DuplicateCheck
	{
		public Dictionary<Type, float> hitTimes = new Dictionary<Type, float>();
	}

	public enum OriginType
	{
		None,
		Instance,
		Caster
	}

	public enum DuplicateCheckType
	{
		HitOnce = 0,
		CooldownPerInstance = 1,
		CooldownSharedByInstanceType = 2,
		DontCheck = -1
	}

	public GameObject hitEffect;

	public GameObject mainEffect;

	public DewCollider range;

	public AbilityTargetValidator hittable;

	public DuplicateCheckType duplicateCheck;

	public float cooldownTime = 0.25f;

	public bool cancelIfCasterDead = true;

	public bool destroyWhenDone = true;

	public DamageData.SourceType type;

	public ScalingValue dmgFactor = new ScalingValue(0f, 1f, 0f, 0f);

	public bool multiplyDamageByMaxHp;

	public float procCoefficient = 1f;

	public OriginType origin;

	public bool isDamageOverTime;

	public float attackEffect;

	public AttackEffectType attackEffectType = AttackEffectType.Others;

	public float knockupAmount;

	public bool applyElemental;

	public float elementalChance = 1f;

	public ElementalType elemental;

	public bool doKnockback;

	public bool useDirectionOfCast;

	public float closeEnemyMultiplier = 1f;

	public float closeEnemyDistanceThreshold = 4f;

	public Knockback knockbackSettings;

	public float heroDamageMultiplier = 1f;

	public float monsterDamageMultiplier = 1f;

	[SyncVar]
	public float strengthMultiplier = 1f;

	private Dictionary<Entity, float> _hitEntities;

	private bool _isCooldownCheck
	{
		get
		{
			DuplicateCheckType duplicateCheckType = duplicateCheck;
			return duplicateCheckType == DuplicateCheckType.CooldownSharedByInstanceType || duplicateCheckType == DuplicateCheckType.CooldownPerInstance;
		}
	}

	public float NetworkstrengthMultiplier
	{
		get
		{
			return strengthMultiplier;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref strengthMultiplier, 32uL, null);
		}
	}

	protected override void OnCreate()
	{
		if (base.isServer)
		{
			_hitEntities = new Dictionary<Entity, float>();
		}
		base.OnCreate();
		if (base.isServer && mainEffect != null)
		{
			FxPlayNetworked(mainEffect);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer && _hitEntities != null)
		{
			_hitEntities = null;
		}
	}

	protected bool CheckShouldBeDestroyed()
	{
		if (cancelIfCasterDead && (object)base.info.caster != null && base.info.caster.IsNullInactiveDeadOrKnockedOut())
		{
			return true;
		}
		return false;
	}

	[Server]
	protected void DoCollisionChecks()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DamageInstance::DoCollisionChecks()' called when server was not active");
		}
		else
		{
			OnCollisionCheck();
		}
	}

	protected virtual void OnCollisionCheck()
	{
		if (!base.isActive || _hitEntities == null)
		{
			return;
		}
		if (!ShouldUseDefaultAbilityTargetValidator())
		{
			ArrayReturnHandle<Entity> handle;
			ReadOnlySpan<Entity> ents = range.GetEntities(out handle);
			for (int i = 0; i < ents.Length; i++)
			{
				Entity ent = ents[i];
				if (!IsDuplicate(ent) && OnValidateTarget(ent))
				{
					AddToDuplicateTracker(ent);
					OnHit(ent);
				}
			}
			handle.Return();
			return;
		}
		ArrayReturnHandle<Entity> handle2;
		ReadOnlySpan<Entity> ents2 = range.GetEntities(out handle2, hittable, base.info.caster);
		for (int j = 0; j < ents2.Length; j++)
		{
			Entity ent2 = ents2[j];
			if (!IsDuplicate(ent2) && OnValidateTarget(ent2))
			{
				AddToDuplicateTracker(ent2);
				OnHit(ent2);
			}
		}
		handle2.Return();
	}

	protected virtual bool ShouldUseDefaultAbilityTargetValidator()
	{
		return (object)base.info.caster != null;
	}

	protected virtual bool OnValidateTarget(Entity entity)
	{
		return true;
	}

	protected virtual void OnHit(Entity entity)
	{
		if (hitEffect != null)
		{
			FxPlayNewNetworked(hitEffect, entity);
		}
		float dmgAmount = GetValue(dmgFactor);
		if (multiplyDamageByMaxHp)
		{
			dmgAmount *= entity.maxHealth;
		}
		if (entity is Hero)
		{
			dmgAmount *= heroDamageMultiplier;
		}
		if (entity is Monster)
		{
			dmgAmount *= monsterDamageMultiplier;
		}
		DamageData dmg = CreateDamage(type, dmgAmount, Mathf.Clamp01(procCoefficient * strengthMultiplier));
		if (applyElemental && UnityEngine.Random.value < elementalChance)
		{
			dmg.SetElemental(elemental);
		}
		switch (origin)
		{
		case OriginType.Instance:
			dmg.SetOriginPosition(base.position);
			break;
		case OriginType.Caster:
			dmg.SetOriginPosition(base.info.caster.position);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case OriginType.None:
			break;
		}
		if (doKnockback)
		{
			float originalDistance = knockbackSettings.distance;
			if (dmg.originPosition.HasValue && Vector2.Distance(dmg.originPosition.Value, entity.position) < closeEnemyDistanceThreshold)
			{
				knockbackSettings.distance *= closeEnemyMultiplier;
			}
			if (useDirectionOfCast)
			{
				knockbackSettings.ApplyWithDirection(base.info.forward, entity);
			}
			else if (dmg.originPosition.HasValue)
			{
				knockbackSettings.ApplyWithOrigin(dmg.originPosition.Value, entity);
			}
			knockbackSettings.distance = originalDistance;
		}
		if (knockupAmount > 0.001f && !entity.Status.hasCrowdControlImmunity)
		{
			entity.Visual.KnockUp(knockupAmount * strengthMultiplier, isFriendly: false);
		}
		dmg.ApplyRawMultiplier(strengthMultiplier);
		if (isDamageOverTime)
		{
			dmg.SetAttr(DamageAttribute.DamageOverTime);
		}
		if (attackEffect > 0.001f)
		{
			dmg.DoAttackEffect(attackEffectType, attackEffect);
		}
		OnBeforeDispatchDamage(ref dmg, entity);
		dmg.Dispatch(entity, chain);
	}

	protected virtual void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
	}

	protected bool IsDuplicate(Entity ent)
	{
		switch (duplicateCheck)
		{
		case DuplicateCheckType.HitOnce:
			return _hitEntities.ContainsKey(ent);
		case DuplicateCheckType.CooldownPerInstance:
		{
			if (!_hitEntities.TryGetValue(ent, out var lastHit))
			{
				return false;
			}
			return Time.time - lastHit < cooldownTime;
		}
		case DuplicateCheckType.CooldownSharedByInstanceType:
		{
			if (!ent.TryGetData<Ad_DuplicateCheck>(out var check))
			{
				return false;
			}
			if (!check.hitTimes.TryGetValue(GetType(), out var lastHitTime))
			{
				return false;
			}
			return Time.time - lastHitTime < cooldownTime;
		}
		case DuplicateCheckType.DontCheck:
			return false;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	protected void AddToDuplicateTracker(Entity ent)
	{
		switch (duplicateCheck)
		{
		case DuplicateCheckType.HitOnce:
		case DuplicateCheckType.CooldownPerInstance:
			_hitEntities[ent] = Time.time;
			break;
		case DuplicateCheckType.CooldownSharedByInstanceType:
		{
			if (!ent.TryGetData<Ad_DuplicateCheck>(out var data))
			{
				data = new Ad_DuplicateCheck();
				ent.AddData(data);
			}
			data.hitTimes[GetType()] = Time.time;
			break;
		}
		case DuplicateCheckType.DontCheck:
			break;
		default:
			throw new ArgumentOutOfRangeException();
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
			writer.WriteFloat(strengthMultiplier);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteFloat(strengthMultiplier);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref strengthMultiplier, null, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x20L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref strengthMultiplier, null, reader.ReadFloat());
		}
	}
}
