using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class ActorManager : NetworkedManagerBase<ActorManager>
{
	public static bool enableUsefulActorName;

	public SafeAction<Actor> onActorAdd;

	public SafeAction<Actor> onActorRemove;

	public SafeAction<Entity> onEntityAdd;

	public SafeAction<Entity> onEntityRemove;

	public SafeAction<Entity> onAwakeEntityAdd;

	public SafeAction<Entity> onAwakeEntityRemove;

	public SafeAction<Hero> onHeroAdd;

	public SafeAction<Hero> onHeroRemove;

	public SafeAction<Hero> onLocalHeroAdd;

	public SafeAction<Hero> onLocalHeroRemove;

	[SyncVar]
	private Actor _serverActor;

	private List<Actor> _allActors = new List<Actor>();

	private List<Actor> _allActorsBeingDestroyed = new List<Actor>();

	private List<Entity> _allEntities = new List<Entity>();

	private List<Hero> _allHeroes = new List<Hero>();

	private float _lastNullEntryCheckTime;

	private const float SleepTickInterval = 1f;

	private const float HeroWakeEntitiesSqrDistance = 1600f;

	private const float TimeTakenToStartSleeping = 4f;

	private const float HeroTeleportMinDistanceForWakeRecalculation = 5f;

	private float _nextSleepCheck = float.PositiveInfinity;

	private const float StuckCheckInterval = 0.1f;

	private const int StuckCheckStrikes = 3;

	private int _currentIndex;

	private int _stuckCheckVersion;

	private float _nextStuckCheckTime;

	protected NetworkBehaviourSyncVar ____serverActorNetId;

	public Actor serverActor => Network_serverActor;

	public IReadOnlyList<Actor> allActors => _allActors;

	public IReadOnlyList<Actor> allActorsBeingDestroyed => _allActorsBeingDestroyed;

	public int numOfActors => _allActors.Count;

	public IReadOnlyList<Entity> allEntities => _allEntities;

	public int numOfEntities => _allEntities.Count;

	public IReadOnlyList<Hero> allHeroes => _allHeroes;

	public int numOfHeroes => _allHeroes.Count;

	public Actor Network_serverActor
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____serverActorNetId, ref _serverActor);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref _serverActor, 1uL, null, ref ____serverActorNetId);
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		enableUsefulActorName = false;
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		Network_serverActor = Dew.CreateActor<ServerActor>(Vector3.zero, Quaternion.identity);
		DoSleepOnStartServer();
	}

	public override void OnStart()
	{
		base.OnStart();
		DewResources.AddPreloadRule(this, delegate(PreloadInterface preload)
		{
			foreach (Actor current in allActors.Concat(allActorsBeingDestroyed))
			{
				preload.AddType(current.GetType().Name);
			}
		});
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		DoSleepLogicUpdate(dt);
		DoStuckCheckLogicUpdate(dt);
		DoCollectionsLogicUpdate(dt);
	}

	internal void AddActor(Actor actor)
	{
		_allActors.Add(actor);
		onActorAdd?.Invoke(actor);
		if (actor is Entity ent)
		{
			_allEntities.Add(ent);
			if (base.isServer)
			{
				RegisterSleepEvents(ent);
			}
			onEntityAdd?.Invoke(ent);
			onAwakeEntityAdd?.Invoke(ent);
		}
		if (actor is Hero hero)
		{
			_allHeroes.Add(hero);
			onHeroAdd?.Invoke(hero);
		}
	}

	internal void RemoveActor(Actor actor)
	{
		if (_allActors.Remove(actor))
		{
			onActorRemove?.Invoke(actor);
		}
		if (actor is Entity ent && _allEntities.Remove(ent))
		{
			onEntityRemove?.Invoke(ent);
		}
		if (actor is Hero hero && _allHeroes.Remove(hero))
		{
			onHeroRemove?.Invoke(hero);
		}
	}

	internal void AddActorBeingDestroyed(Actor actor)
	{
		if ((object)actor != null)
		{
			_allActorsBeingDestroyed.Add(actor);
		}
	}

	internal void RemoveActorBeingDestroyed(Actor actor)
	{
		if ((object)actor != null)
		{
			_allActorsBeingDestroyed.Remove(actor);
		}
	}

	private void DoCollectionsLogicUpdate(float dt)
	{
		if (Time.time - _lastNullEntryCheckTime < 3f)
		{
			return;
		}
		_lastNullEntryCheckTime = Time.time;
		if (DewPlayer.local != null && DewPlayer.local.hero != null && DewPlayer.local.hero.isInCombat)
		{
			return;
		}
		for (int i = _allActors.Count - 1; i >= 0; i--)
		{
			if (_allActors[i] == null)
			{
				if ((object)_allActors[i] != null)
				{
					Debug.LogException(new NullReferenceException("Null actor found in allActors: " + _allActors[i].GetType().Name));
				}
				else
				{
					Debug.LogException(new NullReferenceException("Null actor found in allActors: null"));
				}
				_allActors.RemoveAt(i);
			}
			else if (!_allActors[i].isActive)
			{
				Debug.LogException(new NullReferenceException("Inactive actor found in allActors: " + _allActors[i].GetActorReadableName()));
				_allActors.RemoveAt(i);
			}
		}
		for (int i2 = _allEntities.Count - 1; i2 >= 0; i2--)
		{
			if (_allEntities[i2] == null)
			{
				if ((object)_allEntities[i2] != null)
				{
					Debug.LogException(new NullReferenceException("Null entity found in allEntities: " + _allEntities[i2].GetType().Name));
				}
				else
				{
					Debug.LogException(new NullReferenceException("Null entity found in allEntities: null"));
				}
				_allEntities.RemoveAt(i2);
			}
			else if (!_allEntities[i2].isActive)
			{
				Debug.LogException(new NullReferenceException("Inactive entity found in allEntities: " + _allEntities[i2].GetActorReadableName()));
				_allEntities.RemoveAt(i2);
			}
		}
		for (int i3 = _allHeroes.Count - 1; i3 >= 0; i3--)
		{
			if (_allHeroes[i3] == null)
			{
				if ((object)_allHeroes[i3] != null)
				{
					Debug.LogException(new NullReferenceException("Null hero found in allHeroes: " + _allHeroes[i3].GetType().Name));
				}
				else
				{
					Debug.LogException(new NullReferenceException("Null hero found in allHeroes: null"));
				}
				_allHeroes.RemoveAt(i3);
			}
			else if (!_allHeroes[i3].isActive)
			{
				Debug.LogException(new NullReferenceException("Inactive hero found in allHeroes: " + _allHeroes[i3].GetType().Name));
				_allHeroes.RemoveAt(i3);
			}
		}
	}

	private void DoSleepOnStartServer()
	{
		GameManager.CallOnReady(delegate
		{
			_nextSleepCheck = Time.time + 1f;
		});
	}

	private void RegisterSleepEvents(Entity obj)
	{
		if (obj is Hero h)
		{
			h.Control.ClientEvent_OnTeleport += (Action<Vector3, Vector3>)delegate(Vector3 from, Vector3 to)
			{
				if (Vector2.Distance(from.ToXY(), to.ToXY()) > 5f)
				{
					CalculateWakeImmediately();
				}
			};
		}
		obj.ActorEvent_OnDealDamage += (Action<EventInfoDamage>)delegate
		{
			obj.WakeUp();
		};
		obj.EntityEvent_OnTakeDamage += (Action<EventInfoDamage>)delegate
		{
			obj.WakeUp();
		};
		obj.EntityEvent_OnTakeHeal += (Action<EventInfoHeal>)delegate
		{
			obj.WakeUp();
		};
	}

	public void DoSleepLogicUpdate(float dt)
	{
		if (base.isServer && Time.time > _nextSleepCheck)
		{
			_nextSleepCheck = Time.time + 1f;
			DoSleepTick();
		}
	}

	private void DoSleepTick()
	{
		foreach (Entity e in _allEntities)
		{
			if (e == null || !e.isActive)
			{
				continue;
			}
			bool canSleep = CanEntitySleep(e);
			if (!canSleep)
			{
				e._accumulatedSleepTime = 0f;
				e.WakeUp();
			}
			else if (!e.isSleeping && canSleep)
			{
				e._accumulatedSleepTime += 1f;
				if (e._accumulatedSleepTime > 4f)
				{
					e.Sleep();
				}
			}
		}
	}

	private void CalculateWakeImmediately()
	{
		foreach (Entity e in _allEntities)
		{
			if (!(e == null) && e.isActive && !CanEntitySleep(e))
			{
				e._accumulatedSleepTime = 0f;
				e.WakeUp();
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool CanEntitySleep(Entity e)
	{
		try
		{
			if (e is Hero)
			{
				return false;
			}
			if (e.IsAnyBoss())
			{
				return false;
			}
			foreach (Hero allHero in _allHeroes)
			{
				if (Vector2.SqrMagnitude(allHero.position.ToXY() - e.position.ToXY()) < 1600f)
				{
					return false;
				}
			}
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return false;
		}
	}

	private void DoStuckCheckLogicUpdate(float dt)
	{
		if (!NetworkServer.active || NetworkedManagerBase<GameManager>.instance.disableStuckCheck || Time.time < _nextStuckCheckTime)
		{
			return;
		}
		if (NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition)
		{
			_stuckCheckVersion++;
			return;
		}
		_currentIndex++;
		if (_currentIndex >= allEntities.Count)
		{
			_currentIndex = 0;
		}
		if (allEntities.Count == 0)
		{
			return;
		}
		Entity current = allEntities[_currentIndex];
		if (current.IsNullInactiveDeadOrKnockedOut() || current.Control.isDisplacing || !current.Control._agent.enabled || current.Visual.isSpawning)
		{
			_nextStuckCheckTime = 0f;
			return;
		}
		_nextStuckCheckTime = Time.time + 0.1f;
		if (current._stuckCheckVersion != _stuckCheckVersion)
		{
			current._stuckCheckVersion = _stuckCheckVersion;
			current._wasStuckCounter = 0;
		}
		if (!Dew.IsOkay(current.agentPosition) || CheckIfStuck(current))
		{
			current._wasStuckCounter++;
			if (current._wasStuckCounter < 3)
			{
				return;
			}
			if (current._wasStuckCounter == 3)
			{
				Debug.Log("Entity seems to be stuck: " + current.GetActorReadableName());
			}
			if (current.isSleeping)
			{
				current.WakeUp();
			}
			if (current is Monster m && !m.IsAnyBoss())
			{
				m.PureDamage(m.maxHealth * 0.1f * (float)(current._wasStuckCounter - 3 + 1), 0f).SetAttr(DamageAttribute.DamageOverTime).SetAttr(DamageAttribute.IgnoreDamageImmunity)
					.Dispatch(m);
				return;
			}
			Vector3 curr = current.agentPosition;
			Dew.FilterNonOkayValues(ref curr);
			Vector3 nearest = Dew.SelectBestWithScore(SingletonDewNetworkBehaviour<Room>.instance.playerPathablePoints, (Vector3 point, int _) => 0f - Vector3.Distance(point, curr));
			nearest = Dew.GetValidAgentDestination_Closest(nearest, curr);
			current.Control.StartDisplacement(new DispByDestination
			{
				destination = nearest,
				duration = 0.2f,
				ease = DewEase.EaseOutQuad,
				canGoOverTerrain = true,
				isFriendly = true,
				rotateForward = false,
				rotateSmoothly = false,
				affectedByMovementSpeed = false,
				isCanceledByCC = false
			});
		}
		else
		{
			current._wasStuckCounter = 0;
		}
	}

	private bool CheckIfStuck(Entity entity)
	{
		if (SingletonDewNetworkBehaviour<Room>.softInstance == null)
		{
			return false;
		}
		Vector3 pos = entity.agentPosition;
		if (NetworkedManagerBase<ZoneManager>.instance.currentNode.type == WorldNodeType.ExitBoss && entity is Hero)
		{
			BossMonster boss = Dew.FindActorOfType<BossMonster>();
			if (!boss.IsNullInactiveDeadOrKnockedOut() && !boss.Control.isDisplacing && boss.Control._agent.enabled && !boss.Visual.isSpawning && !CheckIfStuck(boss) && Dew.GetNavMeshPathStatus(pos, boss.agentPosition) != NavMeshPathStatus.PathComplete)
			{
				return true;
			}
		}
		foreach (Vector3 playerPathablePoint in SingletonDewNetworkBehaviour<Room>.softInstance.playerPathablePoints)
		{
			if (Dew.GetNavMeshPathStatus(playerPathablePoint, pos) == NavMeshPathStatus.PathComplete)
			{
				return false;
			}
		}
		return true;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteNetworkBehaviour(Network_serverActor);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_serverActor);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _serverActor, null, reader, ref ____serverActorNetId);
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _serverActor, null, reader, ref ____serverActorNetId);
		}
	}
}
