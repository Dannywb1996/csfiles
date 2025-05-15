using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

[LogicUpdatePriority(-200)]
public abstract class AbilityInstance : Actor
{
	public class WatchedCondition
	{
		internal Func<bool> _condition;

		internal Action _action;

		public bool isWatched { get; internal set; }

		public void StopWatching()
		{
			isWatched = false;
		}
	}

	private class OngoingSequence
	{
		public enum StateType
		{
			Active = 0,
			Done = 1,
			WaitForTime = 2,
			WaitForNextUpdate = 2,
			WaitForCondition = 3
		}

		public StateType state;

		public Stack<IEnumerator> enumerators;

		public float remainingWaitTime;

		public Func<bool> condition;
	}

	[SyncVar]
	private CastInfo _info;

	[SyncVar]
	internal int _skillLevel = -1;

	public GameObject startEffectNoStop;

	public GameObject startEffect;

	public GameObject endEffect;

	[NonSerialized]
	public ReactionChain chain;

	[SyncVar]
	private Gem _gem;

	protected AbilityTargetValidatorWrapper tvDefaultHarmfulEffectTargets;

	protected AbilityTargetValidatorWrapper tvDefaultUsefulEffectTargets;

	private List<OngoingSequence> _sequences = new List<OngoingSequence>();

	private List<WatchedCondition> _watchedConditions = new List<WatchedCondition>();

	protected NetworkBehaviourSyncVar ____gemNetId;

	public CastInfo info
	{
		get
		{
			return _info;
		}
		set
		{
			Network_info = value;
		}
	}

	public int skillLevel
	{
		get
		{
			return ScalingValue.levelOverride ?? _skillLevel;
		}
		set
		{
			Network_skillLevel = value;
		}
	}

	public Gem gem
	{
		get
		{
			if (Network_gem == null)
			{
				Network_gem = FindFirstOfType<Gem>();
			}
			return Network_gem;
		}
		set
		{
			Network_gem = value;
		}
	}

	public bool hasOngoingSequences => _sequences.Count > 0;

	public int effectiveLevel => skillLevel;

	public Entity statEntity => info.caster;

	public CastInfo Network_info
	{
		get
		{
			return _info;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _info, 4uL, null);
		}
	}

	public int Network_skillLevel
	{
		get
		{
			return _skillLevel;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _skillLevel, 8uL, null);
		}
	}

	public Gem Network_gem
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____gemNetId, ref _gem);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref _gem, 16uL, null, ref ____gemNetId);
		}
	}

	protected override void OnPrepare()
	{
		tvDefaultHarmfulEffectTargets = new AbilityTargetValidatorWrapper(info.caster, EntityRelation.Neutral | EntityRelation.Enemy);
		tvDefaultUsefulEffectTargets = new AbilityTargetValidatorWrapper(info.caster, EntityRelation.Self | EntityRelation.Ally);
		base.OnPrepare();
		if (skillLevel == -1)
		{
			if (base.parentActor is AbilityInstance { skillLevel: not -1 } ai)
			{
				Network_skillLevel = ai.skillLevel;
			}
			else if (base.parentActor is SkillTrigger { level: not -1 } st)
			{
				Network_skillLevel = st.level;
			}
			else if (base.parentActor is Gem { effectiveLevel: not -1 } gm)
			{
				Network_skillLevel = gm.effectiveLevel;
			}
			else if (base.parentActor is Summon { skillLevel: not -1 } s)
			{
				Network_skillLevel = s.skillLevel;
			}
			else if (base.parentActor is AbilityTrigger { owner: Summon { skillLevel: not -1 } s2 })
			{
				Network_skillLevel = s2.skillLevel;
			}
		}
	}

	public override void OnStart()
	{
		if (!base.isServer)
		{
			tvDefaultHarmfulEffectTargets = new AbilityTargetValidatorWrapper(info.caster, EntityRelation.Neutral | EntityRelation.Enemy);
			tvDefaultUsefulEffectTargets = new AbilityTargetValidatorWrapper(info.caster, EntityRelation.Self | EntityRelation.Ally);
		}
		base.OnStart();
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		FxPlay(startEffect, info.caster);
		FxPlay(startEffectNoStop, info.caster);
		try
		{
			StartSequence(OnCreateSequenced());
		}
		catch (Exception exception)
		{
			Debug.LogError("Exception occured on " + base.transform.GetScenePath() + "::OnCreateSequenced");
			Debug.LogException(exception, this);
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!base.isActive)
		{
			return;
		}
		for (int i = _watchedConditions.Count - 1; i >= 0; i--)
		{
			WatchedCondition cond = _watchedConditions[i];
			if (!cond.isWatched)
			{
				_watchedConditions.RemoveAt(i);
			}
			else if (cond._condition())
			{
				cond._action?.Invoke();
				_watchedConditions.RemoveAt(i);
			}
		}
		if (!base.isActive)
		{
			return;
		}
		for (int i2 = _sequences.Count - 1; i2 >= 0; i2--)
		{
			OngoingSequence seq = _sequences[i2];
			HandleSequence(seq);
			if (seq.state == OngoingSequence.StateType.Done)
			{
				_sequences.RemoveAt(i2);
			}
		}
	}

	private void HandleSequence(OngoingSequence seq)
	{
		IEnumerator currentEnumerator;
		while (true)
		{
			if (seq.state == OngoingSequence.StateType.Done)
			{
				return;
			}
			if (seq.enumerators.Count <= 0)
			{
				seq.state = OngoingSequence.StateType.Done;
				return;
			}
			if (seq.state == OngoingSequence.StateType.WaitForCondition)
			{
				if (!((SI.WaitForCondition)seq.enumerators.Peek().Current)._condition())
				{
					return;
				}
				seq.state = OngoingSequence.StateType.Active;
			}
			if (seq.state == OngoingSequence.StateType.WaitForTime)
			{
				if (seq.remainingWaitTime > 0f)
				{
					seq.remainingWaitTime -= 1f / 30f;
					if (seq.remainingWaitTime > 0f)
					{
						return;
					}
				}
				seq.state = OngoingSequence.StateType.Active;
			}
			if (seq.state == OngoingSequence.StateType.WaitForTime)
			{
				seq.state = OngoingSequence.StateType.Active;
			}
			currentEnumerator = seq.enumerators.Peek();
			try
			{
				if (!currentEnumerator.MoveNext())
				{
					seq.enumerators.Pop();
					continue;
				}
			}
			catch (Exception exception)
			{
				Debug.LogError("Exception occured while in sequence of " + GetActorReadableName());
				Debug.LogException(exception, this);
				seq.state = OngoingSequence.StateType.Done;
				return;
			}
			if (!(currentEnumerator.Current is SI.DoImmediately))
			{
				if (currentEnumerator.Current is SI.WaitForSeconds wfs)
				{
					seq.state = OngoingSequence.StateType.WaitForTime;
					seq.remainingWaitTime = wfs._seconds;
					return;
				}
				object current = currentEnumerator.Current;
				if (current is SI.WaitForCondition)
				{
					_ = (SI.WaitForCondition)current;
					seq.state = OngoingSequence.StateType.WaitForCondition;
					return;
				}
				if (currentEnumerator.Current == null)
				{
					seq.state = OngoingSequence.StateType.WaitForTime;
					return;
				}
				if (!(currentEnumerator.Current is IEnumerator newEnumerator))
				{
					break;
				}
				seq.enumerators.Push(newEnumerator);
			}
		}
		Debug.LogError($"Sequence returned value out of range: {currentEnumerator} returned {currentEnumerator.Current}", this);
		seq.state = OngoingSequence.StateType.Done;
	}

	[Server]
	public WatchedCondition DestroyOnCondition(Func<bool> condition)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'AbilityInstance/WatchedCondition AbilityInstance::DestroyOnCondition(System.Func`1<System.Boolean>)' called when server was not active");
			return null;
		}
		WatchedCondition cond = new WatchedCondition
		{
			_condition = condition.Invoke,
			_action = delegate
			{
				if (base.isActive)
				{
					Destroy();
				}
			}
		};
		cond.isWatched = true;
		_watchedConditions.Add(cond);
		return cond;
	}

	[Server]
	public void DestroyOnDeath(Entity entity, bool includeKnockOuts = false)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityInstance::DestroyOnDeath(Entity,System.Boolean)' called when server was not active");
			return;
		}
		if (entity == null || entity.isDead || !entity.isActive || (includeKnockOuts && entity is Hero { isKnockedOut: not false }))
		{
			DestroyIfActive();
			return;
		}
		entity.EntityEvent_OnDeath += new Action<EventInfoKill>(DestroyWatchCallback);
		entity.ClientActorEvent_OnDestroyed += new Action<Actor>(DestroyWatchCallback);
		if (includeKnockOuts && entity is Hero h)
		{
			h.ClientHeroEvent_OnKnockedOut += new Action<EventInfoKill>(DestroyWatchCallback);
		}
		ClientActorEvent_OnDestroyed += (Action<Actor>)delegate
		{
			if (!(entity == null))
			{
				entity.EntityEvent_OnDeath -= new Action<EventInfoKill>(DestroyWatchCallback);
				entity.ClientActorEvent_OnDestroyed -= new Action<Actor>(DestroyWatchCallback);
				if (includeKnockOuts && entity is Hero hero2)
				{
					hero2.ClientHeroEvent_OnKnockedOut -= new Action<EventInfoKill>(DestroyWatchCallback);
				}
			}
		};
	}

	[Server]
	public void DestroyOnDestroy(Actor actor)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void AbilityInstance::DestroyOnDestroy(Actor)' called when server was not active");
			return;
		}
		if (actor == null || !actor.isActive)
		{
			DestroyIfActive();
			return;
		}
		actor.ClientActorEvent_OnDestroyed += new Action<Actor>(DestroyWatchCallback);
		ClientActorEvent_OnDestroyed += (Action<Actor>)delegate
		{
			if (!(actor == null))
			{
				actor.ClientActorEvent_OnDestroyed -= new Action<Actor>(DestroyWatchCallback);
			}
		};
	}

	private void DestroyWatchCallback(Actor obj)
	{
		if (base.isActive)
		{
			Destroy();
		}
	}

	private void DestroyWatchCallback(EventInfoKill obj)
	{
		if (base.isActive)
		{
			Destroy();
		}
	}

	protected void StartSequence(IEnumerator sequence)
	{
		if (!base.isActive)
		{
			sequence.MoveNext();
			return;
		}
		OngoingSequence newSeq = new OngoingSequence
		{
			state = OngoingSequence.StateType.Active,
			enumerators = new Stack<IEnumerator>()
		};
		newSeq.enumerators.Push(sequence);
		_sequences.Add(newSeq);
		HandleSequence(newSeq);
	}

	protected virtual IEnumerator OnCreateSequenced()
	{
		yield return default(SI.DoImmediately);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			if (endEffect != null)
			{
				FxPlay(endEffect, info.caster);
			}
			for (int i = 0; i < _watchedConditions.Count; i++)
			{
				_watchedConditions[i].isWatched = false;
			}
			yield return null;
			yield return null;
			if (startEffect != null)
			{
				FxStop(startEffect);
			}
		}
	}

	[Server]
	public T CreateStatusEffect<T>(T prefab, Entity victim, Action<T> beforePrepare = null) where T : StatusEffect
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'T AbilityInstance::CreateStatusEffect(T,Entity,System.Action`1<T>)' called when server was not active");
			return null;
		}
		return CreateStatusEffect(prefab, victim, new CastInfo(info.caster), beforePrepare);
	}

	[Server]
	public T CreateStatusEffect<T>(Entity victim, Action<T> beforePrepare = null) where T : StatusEffect
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'T AbilityInstance::CreateStatusEffect(Entity,System.Action`1<T>)' called when server was not active");
			return null;
		}
		return CreateStatusEffect(victim, new CastInfo(info.caster), beforePrepare);
	}

	public override string GetActorReadableName()
	{
		return base.GetActorReadableName() + " " + ((skillLevel > 0) ? $" ({skillLevel})" : "");
	}

	public void StartChargingChannel(ChargingChannel channel)
	{
		channel.Dispatch(info.caster, base.firstTrigger);
	}

	public DamageData DefaultDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Default, value, procCoefficient);
	}

	public DamageData PhysicalDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Physical, value, procCoefficient);
	}

	public DamageData MagicDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Magic, value, procCoefficient);
	}

	public DamageData PhysicalMagicDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Physical | DamageData.SourceType.Magic, value, procCoefficient);
	}

	public DamageData PureDamage(ScalingValue value, float procCoefficient = 1f)
	{
		return CreateDamage(DamageData.SourceType.Pure, value, procCoefficient);
	}

	public DamageData Damage(ScalingValue value, float procCoefficient = 1f)
	{
		DamageData.SourceType type = DamageData.SourceType.Default;
		if (value.adFactor > 0f)
		{
			type |= DamageData.SourceType.Physical;
		}
		else if (value.apFactor > 0f)
		{
			type |= DamageData.SourceType.Magic;
		}
		return CreateDamage(type, value, procCoefficient);
	}

	public DamageData CreateDamage(DamageData.SourceType type, ScalingValue value, float procCoefficient = 1f)
	{
		return new DamageData(type, value, statEntity, effectiveLevel, procCoefficient).SetActor(this);
	}

	public HealData Heal(ScalingValue amount)
	{
		return new HealData(GetValue(amount)).SetActor(this);
	}

	public float GetValue(ScalingValue val)
	{
		return val.GetValue(effectiveLevel, statEntity);
	}

	public T GetValue<T>(T[] val)
	{
		return val.GetClamped(effectiveLevel);
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_CastInfo(writer, _info);
			writer.WriteInt(_skillLevel);
			writer.WriteNetworkBehaviour(Network_gem);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			GeneratedNetworkCode._Write_CastInfo(writer, _info);
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteInt(_skillLevel);
		}
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_gem);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref _info, null, GeneratedNetworkCode._Read_CastInfo(reader));
			GeneratedSyncVarDeserialize(ref _skillLevel, null, reader.ReadInt());
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _gem, null, reader, ref ____gemNetId);
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 4L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _info, null, GeneratedNetworkCode._Read_CastInfo(reader));
		}
		if ((num & 8L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _skillLevel, null, reader.ReadInt());
		}
		if ((num & 0x10L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _gem, null, reader, ref ____gemNetId);
		}
	}
}
