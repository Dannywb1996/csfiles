using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class RoomModifierBase : Actor, IExcludeFromPool
{
	[FormerlySerializedAs("showOnTopRight")]
	public bool isMain;

	public Color mainColor = Color.white;

	public Sprite mapSprite;

	public float mapSpriteScale = 1f;

	public NodeModifierVisibility visibilityOnWorld = NodeModifierVisibility.OnRevealedFull;

	public bool hiddenOnVisitedNode;

	public bool excludeFromPool;

	public ModifierSpawnType spawnType;

	public float chance = 0.01f;

	public ScaleWithDifficultyMode difficultyScaling;

	public string[] allowedZones;

	public Vector2Int zoneIndexRange = new Vector2Int(0, int.MaxValue);

	public bool spawnOncePerLoop;

	public bool disallowOtherModifiers;

	public bool modifiesRewards;

	public GameObject fxLoop;

	public bool disableOtherEnvParticles;

	public bool enableInnerDecorations;

	public DecorationSettings innerDecorations;

	public bool enableOuterDecorations;

	public DecorationSettings outerDecorations;

	[CompilerGenerated]
	[SyncVar]
	private bool _003CisNewInstance_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int _003Cid_003Ek__BackingField;

	private readonly List<GameObject> _spawnedProps = new List<GameObject>();

	private Action<Entity> _onEntityAdd;

	private readonly List<Action> _onDestroyActor = new List<Action>();

	bool IExcludeFromPool.excludeFromPool => excludeFromPool;

	public bool isNewInstance
	{
		[CompilerGenerated]
		get
		{
			return _003CisNewInstance_003Ek__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CisNewInstance_003Ek__BackingField = value;
		}
	}

	public int id
	{
		[CompilerGenerated]
		get
		{
			return _003Cid_003Ek__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003Cid_003Ek__BackingField = value;
		}
	}

	public Dictionary<string, object> serverData => NetworkedManagerBase<ZoneManager>.instance.modifierServerData[id];

	public ModifierData modData => NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers.Find((ModifierData m) => m.id == id);

	public override bool isDestroyedOnRoomChange => false;

	public bool Network_003CisNewInstance_003Ek__BackingField
	{
		get
		{
			return isNewInstance;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isNewInstance, 4uL, null);
		}
	}

	public int Network_003Cid_003Ek__BackingField
	{
		get
		{
			return id;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref id, 8uL, null);
		}
	}

	private string GetZoneValidationMessage(string[] obj)
	{
		if (obj == null)
		{
			return null;
		}
		foreach (string o in obj)
		{
			if (!DewResources.database.nameToGuid.ContainsKey(o))
			{
				return "Zone of name '" + o + "' not found";
			}
		}
		return null;
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		FxPlay(fxLoop);
		if (!base.isServer)
		{
			return;
		}
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted += new Action<EventInfoLoadRoom>(ClientEventOnRoomLoadStarted);
		NetworkedManagerBase<ActorManager>.instance.onEntityAdd += new Action<Entity>(OnEntityAdd);
		if (spawnOncePerLoop)
		{
			string typeName = GetType().Name;
			if (!NetworkedManagerBase<ZoneManager>.instance._bannedRoomModifiersForCurrentLoop.Contains(typeName))
			{
				NetworkedManagerBase<ZoneManager>.instance._bannedRoomModifiersForCurrentLoop.Add(typeName);
			}
		}
	}

	private void OnEntityAdd(Entity obj)
	{
		try
		{
			_onEntityAdd?.Invoke(obj);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(fxLoop);
		foreach (GameObject p in _spawnedProps)
		{
			if (p != null)
			{
				UnityEngine.Object.Destroy(p);
			}
		}
		_spawnedProps.Clear();
		if (disableOtherEnvParticles)
		{
			foreach (EnvParticle e in EnvParticle.instances)
			{
				if (!(e == null))
				{
					e.gameObject.SetActive(value: true);
				}
			}
		}
		if (base.isServer)
		{
			if (NetworkedManagerBase<ZoneManager>.instance != null)
			{
				NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoadStarted -= new Action<EventInfoLoadRoom>(ClientEventOnRoomLoadStarted);
			}
			if (NetworkedManagerBase<ActorManager>.instance != null)
			{
				NetworkedManagerBase<ActorManager>.instance.onEntityAdd -= new Action<Entity>(OnEntityAdd);
			}
		}
		foreach (Action c in _onDestroyActor)
		{
			try
			{
				c();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		_onDestroyActor.Clear();
	}

	[Server]
	public void ModifyEntities(Action<Entity> onStart, Action<Entity> onStop)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomModifierBase::ModifyEntities(System.Action`1<Entity>,System.Action`1<Entity>)' called when server was not active");
			return;
		}
		foreach (Entity e in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			try
			{
				onStart?.Invoke(e);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		_onEntityAdd = (Action<Entity>)Delegate.Combine(_onEntityAdd, onStart);
		_onDestroyActor.Add(delegate
		{
			foreach (Entity current in NetworkedManagerBase<ActorManager>.instance.allEntities)
			{
				try
				{
					onStop?.Invoke(current);
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
			}
		});
	}

	public override void OnStart()
	{
		base.OnStart();
		_spawnedProps.Clear();
		if (enableInnerDecorations)
		{
			_spawnedProps.AddRange(PlaceDecorations(innerDecorations, SingletonDewNetworkBehaviour<Room>.instance.map.mapData.innerPropNodeIndices));
		}
		if (enableOuterDecorations)
		{
			_spawnedProps.AddRange(PlaceDecorations(outerDecorations, SingletonDewNetworkBehaviour<Room>.instance.map.mapData.outerPropNodeIndices));
		}
		if (enableInnerDecorations)
		{
			GameObject[] decorations = innerDecorations.decorations;
			for (int i = 0; i < decorations.Length; i++)
			{
				decorations[i].SetActive(value: false);
			}
		}
		if (enableOuterDecorations)
		{
			GameObject[] decorations = outerDecorations.decorations;
			for (int i = 0; i < decorations.Length; i++)
			{
				decorations[i].SetActive(value: false);
			}
		}
		if (!disableOtherEnvParticles)
		{
			return;
		}
		foreach (EnvParticle e in EnvParticle.instances)
		{
			if (!(e == null))
			{
				e.gameObject.SetActive(value: false);
			}
		}
	}

	public static List<GameObject> PlaceDecorations(DecorationSettings s, IReadOnlyList<(int, int)> indices)
	{
		List<GameObject> spawned = new List<GameObject>();
		float cellSize = SingletonDewNetworkBehaviour<Room>.instance.map.mapData.cells.cellSize;
		int numOfProps = Mathf.RoundToInt(cellSize * cellSize * (float)indices.Count * s.decoDensity);
		for (int i = 0; i < numOfProps; i++)
		{
			GameObject prefab = s.decorations[UnityEngine.Random.Range(0, s.decorations.Length)];
			Vector3 pos = SingletonDewNetworkBehaviour<Room>.instance.map.mapData.cells.GetWorldPos(indices[UnityEngine.Random.Range(0, indices.Count)]).ToXZ();
			pos += UnityEngine.Random.insideUnitSphere * s.decoPositionRandomMag;
			pos = Dew.GetPositionOnGround(pos) + prefab.transform.localPosition;
			Quaternion rot = Quaternion.Euler(UnityEngine.Random.Range(s.decoRotationMin.x, s.decoRotationMax.x), UnityEngine.Random.Range(s.decoRotationMin.y, s.decoRotationMax.y), UnityEngine.Random.Range(s.decoRotationMin.z, s.decoRotationMax.z)) * prefab.transform.localRotation;
			Vector3 scale = Vector3.zero;
			if (s.uniformScale)
			{
				float num = UnityEngine.Random.Range(s.decoScaleMin.x, s.decoScaleMax.x);
				scale = Vector3.Scale(prefab.transform.localScale, new Vector3(num, num, num));
			}
			else
			{
				scale = Vector3.Scale(prefab.transform.localScale, new Vector3(UnityEngine.Random.Range(s.decoScaleMin.x, s.decoScaleMax.x), UnityEngine.Random.Range(s.decoScaleMin.y, s.decoScaleMax.y), UnityEngine.Random.Range(s.decoScaleMin.z, s.decoScaleMax.z)));
			}
			GameObject instance = UnityEngine.Object.Instantiate(prefab, pos, rot);
			instance.transform.localScale = scale;
			spawned.Add(instance);
		}
		return spawned;
	}

	private void ClientEventOnRoomLoadStarted(EventInfoLoadRoom _)
	{
		DestroyIfActive();
	}

	public float GetScaledChance()
	{
		return difficultyScaling switch
		{
			ScaleWithDifficultyMode.None => chance, 
			ScaleWithDifficultyMode.Beneficial => chance * NetworkedManagerBase<GameManager>.instance.difficulty.beneficialNodeMultiplier, 
			ScaleWithDifficultyMode.Harmful => chance * NetworkedManagerBase<GameManager>.instance.difficulty.harmfulNodeMultiplier, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public virtual bool IsAvailableInGame()
	{
		return true;
	}

	public override bool ShouldBeSaved()
	{
		return true;
	}

	[Server]
	public void RemoveModifier()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomModifierBase::RemoveModifier()' called when server was not active");
		}
		else
		{
			NetworkedManagerBase<ZoneManager>.instance.RemoveModifier(NetworkedManagerBase<ZoneManager>.instance.currentNodeIndex, id);
		}
	}

	[Server]
	public void PlaceShrine<T>(PlaceShrineSettings settings) where T : Shrine
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void RoomModifierBase::PlaceShrine(PlaceShrineSettings)' called when server was not active");
			return;
		}
		Shrine shrine;
		if (serverData.TryGetValue("shrineData_" + typeof(T).Name, out var val))
		{
			Dictionary<string, object> data = (Dictionary<string, object>)val;
			T prefab = DewResources.GetByType<T>();
			shrine = (Shrine)prefab.OnLoadCreateActor(data, this);
			shrine.OnLoadActor(data);
		}
		else
		{
			Vector3 pos;
			if (settings.customPosition.HasValue)
			{
				pos = settings.customPosition.Value;
			}
			else if (settings.spawnOnLastSection)
			{
				SingletonDewNetworkBehaviour<Room>.instance.GetFinalSection().TryGetGoodNodePosition(out pos);
			}
			else
			{
				SingletonDewNetworkBehaviour<Room>.instance.props.TryGetGoodNodePosition(out pos);
			}
			shrine = Dew.CreateActor<T>(pos, null, this);
			if (settings.lockedUntilCleared)
			{
				shrine.isLocked = true;
				if (SingletonDewNetworkBehaviour<Room>.instance.didClearRoom)
				{
					Listener();
				}
				else
				{
					SingletonDewNetworkBehaviour<Room>.instance.onRoomClear.AddListener(Listener);
				}
			}
		}
		shrine.ExcludeFromRoomSave();
		shrine.ClientEvent_OnSuccessfulUse += (Action<Entity>)delegate
		{
			if (shrine.currentUseCount >= shrine.maxUseCount)
			{
				shrine.ClearExcludeFromRoomSave();
				if (settings.removeModifierOnUse)
				{
					RemoveModifier();
				}
			}
		};
		_onDestroyActor.Add(delegate
		{
			if (!shrine.IsNullOrInactive())
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				shrine.OnSaveActor(dictionary);
				serverData["shrineData_" + typeof(T).Name] = dictionary;
			}
		});
		void Listener()
		{
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			while (SingletonDewNetworkBehaviour<Room>.instance != null && SingletonDewNetworkBehaviour<Room>.instance.monsters.isDoingHunterWelcomingSpawn)
			{
				yield return new WaitForSeconds(0.5f);
			}
			if (shrine != null)
			{
				shrine.isLocked = false;
			}
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
			writer.WriteBool(isNewInstance);
			writer.WriteInt(id);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteBool(isNewInstance);
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteInt(id);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref isNewInstance, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref id, null, reader.ReadInt());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 4L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isNewInstance, null, reader.ReadBool());
		}
		if ((num & 8L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref id, null, reader.ReadInt());
		}
	}
}
