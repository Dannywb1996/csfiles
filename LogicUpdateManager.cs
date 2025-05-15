using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Profiling;
using UnityEngine;

public class LogicUpdateManager : ManagerBase<LogicUpdateManager>
{
	private static readonly ProfilerMarker FrameUpdateMarker = new ProfilerMarker("LogicUpdate.FrameUpdate");

	private static readonly ProfilerMarker LogicUpdateMarker = new ProfilerMarker("LogicUpdate.LogicUpdate");

	public const float LogicUpdateInterval = 1f / 30f;

	private const float MaxLogicUpdateBehindSeconds = 5f;

	private const int MaxLogicUpdatePerFrame = 10;

	private const bool WarnOnMultipleLogicUpdatesPerFrame = false;

	public const int DefaultExecutionPriority = 0;

	internal readonly List<ILogicUpdate> _removedObjects = new List<ILogicUpdate>();

	internal readonly List<ILogicUpdate> _addedObjects = new List<ILogicUpdate>();

	private float _lastLogicUpdateTime;

	internal List<ILogicUpdate>[] _logicObjectLists;

	internal static Dictionary<int, int> _priorityToArrayIndex;

	internal static Dictionary<int, int> _arrayIndexToPriority;

	private static Dictionary<Type, int> _typeToPriorityCache;

	private static List<int> _allPriorities;

	public float RemainingTimeUntilNextLogicUpdate => Mathf.Max(1f / 30f - (Time.time - _lastLogicUpdateTime), Time.deltaTime);

	public override bool shouldRegisterUpdates => false;

	public float deltaTime => 1f / 30f;

	protected override void Awake()
	{
		base.Awake();
		InitIfNecessary();
	}

	private void InitIfNecessary()
	{
		if (_priorityToArrayIndex == null)
		{
			_priorityToArrayIndex = new Dictionary<int, int>();
			_arrayIndexToPriority = new Dictionary<int, int>();
			_typeToPriorityCache = new Dictionary<Type, int>();
			_allPriorities = new List<int>();
			_allPriorities.Add(0);
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				Type[] types = assemblies[i].GetTypes();
				foreach (Type t in types)
				{
					LogicUpdatePriorityAttribute attr = (LogicUpdatePriorityAttribute)Attribute.GetCustomAttribute(t, typeof(LogicUpdatePriorityAttribute), inherit: true);
					if (attr != null)
					{
						if (!_allPriorities.Contains(attr.priority))
						{
							_allPriorities.Add(attr.priority);
						}
						_typeToPriorityCache.Add(t, attr.priority);
					}
				}
			}
			_allPriorities.Sort();
			for (int k = 0; k < _allPriorities.Count; k++)
			{
				_priorityToArrayIndex.Add(_allPriorities[k], k);
				_arrayIndexToPriority.Add(k, _allPriorities[k]);
			}
		}
		if (_logicObjectLists == null)
		{
			_logicObjectLists = new List<ILogicUpdate>[_allPriorities.Count];
			for (int l = 0; l < _allPriorities.Count; l++)
			{
				_logicObjectLists[l] = new List<ILogicUpdate>();
			}
		}
	}

	private void PrintErrorOfObjectName(object obj)
	{
		try
		{
			if (obj is Actor a)
			{
				Debug.LogError(a.GetActorReadableName());
				return;
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		try
		{
			if (obj is MonoBehaviour a2)
			{
				Debug.LogError(a2);
				return;
			}
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
		try
		{
			if (obj is UnityEngine.Object a3)
			{
				Debug.LogError(a3);
				return;
			}
		}
		catch (Exception exception3)
		{
			Debug.LogException(exception3);
		}
		try
		{
			Debug.LogError($"{obj} ({obj.GetType().Name})");
		}
		catch (Exception exception4)
		{
			Debug.LogException(exception4);
		}
	}

	private void Update()
	{
		DoNodeRemoval();
		DoLogicUpdates();
		for (int i = 0; i < _logicObjectLists.Length; i++)
		{
			List<ILogicUpdate> arr = _logicObjectLists[i];
			for (int j = 0; j < arr.Count; j++)
			{
				try
				{
					arr[j].FrameUpdate();
				}
				catch (Exception exception)
				{
					PrintErrorOfObjectName(arr[j]);
					Debug.LogException(exception, arr[j] as UnityEngine.Object);
				}
			}
		}
		DoNodeRemoval();
		DoNodeAddition();
	}

	private void DoNodeRemoval()
	{
		if (_removedObjects.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < _removedObjects.Count; i++)
		{
			ILogicUpdate obj = _removedObjects[i];
			int priority = GetPriority(obj);
			int arrayIndex = _priorityToArrayIndex[priority];
			List<ILogicUpdate> arr = _logicObjectLists[arrayIndex];
			int objIndex = arr.IndexOf(obj);
			if (objIndex < 0)
			{
				if (obj as UnityEngine.Object != null)
				{
					Debug.LogWarning($"Object to remove not found in logic objects: {obj as UnityEngine.Object}");
				}
				else
				{
					Debug.LogWarning($"Object to remove not found in logic objects: {obj}");
				}
			}
			else
			{
				arr[objIndex] = arr[arr.Count - 1];
				arr.RemoveAt(arr.Count - 1);
			}
		}
		_removedObjects.Clear();
	}

	private void DoLogicUpdates()
	{
		int numOfLogicUpdates = (int)((Time.time - _lastLogicUpdateTime) / (1f / 30f));
		int maxPerFrame = 10;
		if (Time.timeScale > 1f)
		{
			maxPerFrame = Mathf.CeilToInt((float)maxPerFrame * Time.timeScale);
		}
		if (numOfLogicUpdates > maxPerFrame)
		{
			Debug.LogWarning($"Reached maximum logic update per frame: {numOfLogicUpdates}/{maxPerFrame}, running {Time.time - _lastLogicUpdateTime} seconds behind. Possibly performance issue?");
			if (Time.time - _lastLogicUpdateTime > 5f)
			{
				_lastLogicUpdateTime = Time.time - 5f;
			}
			numOfLogicUpdates = maxPerFrame;
		}
		if (numOfLogicUpdates <= 0)
		{
			return;
		}
		for (int _ = 0; _ < numOfLogicUpdates; _++)
		{
			_lastLogicUpdateTime += 1f / 30f;
			for (int i = 0; i < _logicObjectLists.Length; i++)
			{
				List<ILogicUpdate> arr = _logicObjectLists[i];
				for (int j = 0; j < arr.Count; j++)
				{
					try
					{
						arr[j].LogicUpdate(1f / 30f);
					}
					catch (Exception exception)
					{
						PrintErrorOfObjectName(arr[j]);
						if (arr[j] is UnityEngine.Object obj)
						{
							Debug.LogException(exception, obj);
						}
						else
						{
							Debug.LogException(exception);
						}
					}
				}
			}
		}
		DoNodeRemoval();
	}

	private int GetPriority(ILogicUpdate lobj)
	{
		Type type = lobj.GetType();
		if (_typeToPriorityCache.TryGetValue(type, out var priority))
		{
			return priority;
		}
		return 0;
	}

	private void DoNodeAddition()
	{
		for (int i = 0; i < _addedObjects.Count; i++)
		{
			ILogicUpdate lobj = _addedObjects[i];
			int priority = GetPriority(lobj);
			_logicObjectLists[_priorityToArrayIndex[priority]].Add(lobj);
		}
		_addedObjects.Clear();
	}

	public static void Register(ILogicUpdate lobj)
	{
		if (ManagerBase<LogicUpdateManager>.instance == null)
		{
			Debug.LogError("LogicUpdateManager.Register failed, LogicUpdateManager instance not found.", lobj as UnityEngine.Object);
			return;
		}
		ManagerBase<LogicUpdateManager>.instance.InitIfNecessary();
		ManagerBase<LogicUpdateManager>.instance._addedObjects.Add(lobj);
	}

	public static void Unregister(ILogicUpdate lobj)
	{
		if (!(ManagerBase<LogicUpdateManager>.instance == null))
		{
			int index = ManagerBase<LogicUpdateManager>.instance._addedObjects.IndexOf(lobj);
			if (index != -1)
			{
				ManagerBase<LogicUpdateManager>.instance._addedObjects[index] = ManagerBase<LogicUpdateManager>.instance._addedObjects[ManagerBase<LogicUpdateManager>.instance._addedObjects.Count - 1];
				ManagerBase<LogicUpdateManager>.instance._addedObjects.RemoveAt(ManagerBase<LogicUpdateManager>.instance._addedObjects.Count - 1);
			}
			else
			{
				ManagerBase<LogicUpdateManager>.instance.InitIfNecessary();
				ManagerBase<LogicUpdateManager>.instance._removedObjects.Add(lobj);
			}
		}
	}
}
