using System;
using System.Collections.Generic;
using System.Linq;
using DewInternal;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class DewResources
{
	private struct PreloadRuleEntry
	{
		public MonoBehaviour owner;

		public Action<PreloadInterface> onPreload;
	}

	private const string MainDatabase = "MainResources";

	private static DewResourceDatabase _database;

	private static Dictionary<string, AsyncOperationHandle<UnityEngine.Object>> _loadedObjects = new Dictionary<string, AsyncOperationHandle<UnityEngine.Object>>();

	public static bool EnableVerboseLogging = false;

	private static PreloadInterface _preloadInterface = new PreloadInterface();

	private static List<PreloadRuleEntry> _preloadRules = new List<PreloadRuleEntry>();

	public static DewResourceDatabase database
	{
		get
		{
			if (_database == null)
			{
				_database = Resources.Load<DewResourceDatabase>("MainResources");
				if (_database == null)
				{
					throw new Exception("Could not load main DewResourceDatabase!");
				}
				_database.InitForRuntime();
			}
			return _database;
		}
	}

	public static IReadOnlyCollection<string> loadedGuids => _loadedObjects.Keys;

	private static T Convert<T>(UnityEngine.Object obj) where T : UnityEngine.Object
	{
		if (obj is T)
		{
			return (T)obj;
		}
		if (obj is GameObject gobj && gobj.TryGetComponent<T>(out var c))
		{
			return c;
		}
		return null;
	}

	private static UnityEngine.Object Convert(UnityEngine.Object obj, Type type)
	{
		if (type.IsInstanceOfType(obj))
		{
			return obj;
		}
		if (obj is GameObject gobj && gobj.TryGetComponent(type, out var c))
		{
			return c;
		}
		return null;
	}

	private static bool TryConvert<T>(UnityEngine.Object obj, out T result) where T : UnityEngine.Object
	{
		result = Convert<T>(obj);
		return result != null;
	}

	private static bool TryConvert(UnityEngine.Object obj, Type type, out UnityEngine.Object result)
	{
		result = Convert(obj, type);
		return result != null;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInitAddressables()
	{
		UnloadUnused();
	}

	public static UnityEngine.Object Load(string guid)
	{
		if (!_loadedObjects.TryGetValue(guid, out var obj))
		{
			try
			{
				obj = Addressables.LoadAssetAsync<UnityEngine.Object>(guid);
			}
			catch (Exception exception)
			{
				Debug.LogError("Exception occurred while trying to load " + guid);
				Debug.LogException(exception);
				return null;
			}
			_loadedObjects.Add(guid, obj);
		}
		if (obj.Status == AsyncOperationStatus.Succeeded)
		{
			return obj.Result;
		}
		if (obj.Status == AsyncOperationStatus.Failed)
		{
			return null;
		}
		UnityEngine.Object o = obj.WaitForCompletion();
		if (EnableVerboseLogging)
		{
			Debug.Log("+ [DewResources] Load: " + o);
		}
		return o;
	}

	public static void Preload(string guid)
	{
		if (_loadedObjects.TryGetValue(guid, out var obj))
		{
			return;
		}
		obj = Addressables.LoadAssetAsync<UnityEngine.Object>(guid);
		if (EnableVerboseLogging)
		{
			obj.Completed += delegate(AsyncOperationHandle<UnityEngine.Object> o)
			{
				if (o.Result != null)
				{
					Debug.Log("+ [DewResources] Preload " + o.Result);
				}
				else
				{
					Debug.Log("+ [DewResources] Preload " + guid);
				}
			};
		}
		_loadedObjects.Add(guid, obj);
	}

	public static void UnloadUnused()
	{
		try
		{
			string[] currentGuids = _loadedObjects.Keys.ToArray();
			int unloaded = 0;
			int preloaded = 0;
			_preloadInterface._guids.Clear();
			for (int i = _preloadRules.Count - 1; i >= 0; i--)
			{
				PreloadRuleEntry p = _preloadRules[i];
				if (p.owner == null)
				{
					_preloadRules.RemoveAt(i);
				}
				else
				{
					try
					{
						p.onPreload(_preloadInterface);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
			}
			string[] array = currentGuids;
			foreach (string g in array)
			{
				if (_preloadInterface._guids.Contains(g))
				{
					continue;
				}
				unloaded++;
				if (EnableVerboseLogging)
				{
					if (_loadedObjects[g].Result != null)
					{
						Debug.Log("[DewResources] - Unload " + _loadedObjects[g].Result);
					}
					else
					{
						Debug.Log("[DewResources] - Unload " + g);
					}
				}
				Addressables.Release(_loadedObjects[g]);
				_loadedObjects.Remove(g);
			}
			foreach (string g2 in _preloadInterface._guids)
			{
				if (!currentGuids.Contains(g2))
				{
					preloaded++;
					Preload(g2);
				}
			}
			if (unloaded == 0 && preloaded == 0)
			{
				if (currentGuids.Length != 0)
				{
					Debug.Log($"[DewResources] Maintenance Finished: {currentGuids.Length} (Nothing changed)");
				}
			}
			else
			{
				Debug.Log($"[DewResources] Maintenance Finished: {currentGuids.Length} -> {loadedGuids.Count} (-{unloaded}) (+{preloaded})");
			}
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	public static string FindRoomNameBySubstring(string name)
	{
		foreach (string s in database.sceneNames)
		{
			if (s.StartsWith("Room_", StringComparison.InvariantCultureIgnoreCase) && s.ToLower().Contains(name.ToLower()))
			{
				return s;
			}
		}
		return null;
	}

	public static IEnumerable<UnityEngine.Object> FindAllByTypeSubstring(string substring)
	{
		foreach (KeyValuePair<string, string> pair in database.typeNameToGuid)
		{
			if (pair.Key.ToLower().Contains(substring.ToLower()))
			{
				yield return GetByGuid<UnityEngine.Object>(pair.Value);
			}
		}
	}

	public static IEnumerable<TFilter> FindAllByTypeSubstring<TFilter>(string substring) where TFilter : UnityEngine.Object
	{
		foreach (KeyValuePair<string, string> pair in database.typeNameToGuid)
		{
			if (pair.Key.ToLower().Contains(substring.ToLower()) && TryConvert<TFilter>(GetByGuid(pair.Value), out var res))
			{
				yield return res;
			}
		}
	}

	public static UnityEngine.Object FindOneByTypeSubstring(string substring)
	{
		IEnumerator<UnityEngine.Object> enumerator = FindAllByTypeSubstring(substring).GetEnumerator();
		enumerator.MoveNext();
		UnityEngine.Object curr = enumerator.Current;
		enumerator.Dispose();
		return curr;
	}

	public static TFilter FindOneByTypeSubstring<TFilter>(string substring) where TFilter : UnityEngine.Object
	{
		IEnumerator<TFilter> enumerator = FindAllByTypeSubstring<TFilter>(substring).GetEnumerator();
		enumerator.MoveNext();
		TFilter curr = enumerator.Current;
		enumerator.Dispose();
		return curr;
	}

	public static IEnumerable<UnityEngine.Object> FindAllByNameSubstring(string substring)
	{
		foreach (KeyValuePair<string, string> pair in database.nameToGuid)
		{
			if (pair.Key.ToLower().Contains(substring.ToLower()))
			{
				yield return GetByGuid(pair.Value);
			}
		}
	}

	public static IEnumerable<TFilter> FindAllByNameSubstring<TFilter>(string substring) where TFilter : UnityEngine.Object
	{
		foreach (KeyValuePair<string, string> pair in database.nameToGuid)
		{
			if (pair.Key.ToLower().Contains(substring.ToLower()) && TryConvert<TFilter>(GetByGuid(pair.Value), out var res))
			{
				yield return res;
			}
		}
	}

	public static UnityEngine.Object FindOneByIdSubstring(string substring)
	{
		IEnumerator<UnityEngine.Object> enumerator = FindAllByNameSubstring(substring).GetEnumerator();
		enumerator.MoveNext();
		UnityEngine.Object curr = enumerator.Current;
		enumerator.Dispose();
		return curr;
	}

	public static TFilter FindOneByIdSubstring<TFilter>(string substring) where TFilter : UnityEngine.Object
	{
		IEnumerator<TFilter> enumerator = FindAllByNameSubstring<TFilter>(substring).GetEnumerator();
		enumerator.MoveNext();
		TFilter curr = enumerator.Current;
		enumerator.Dispose();
		return curr;
	}

	public static IEnumerable<T> FindAllByType<T>() where T : UnityEngine.Object
	{
		foreach (KeyValuePair<string, Type> pair in database.typeNameToType)
		{
			if (typeof(T).IsAssignableFrom(pair.Value) && database.typeToGuid.ContainsKey(pair.Value))
			{
				yield return GetByType<T>(pair.Value);
			}
		}
	}

	public static IEnumerable<UnityEngine.Object> FindAllByType(Type type)
	{
		foreach (KeyValuePair<string, Type> pair in database.typeNameToType)
		{
			if (type.IsAssignableFrom(pair.Value) && database.typeToGuid.ContainsKey(pair.Value))
			{
				yield return GetByType(pair.Value);
			}
		}
	}

	public static GameObject GetNetworkedPrefab(uint id)
	{
		if (database.netObjectAssetIdToGuid.TryGetValue(id, out var _))
		{
			return (GameObject)Load(database.netObjectAssetIdToGuid[id]);
		}
		Debug.LogError(string.Format("[DewResources] {0} failed with id: {1}", "GetNetworkedPrefab", id));
		return null;
	}

	public static T GetByType<T>() where T : UnityEngine.Object
	{
		return (T)GetByType(typeof(T));
	}

	public static UnityEngine.Object GetByType(Type type)
	{
		if (database.typeToGuid.TryGetValue(type, out var guid))
		{
			return Convert(Load(guid), type);
		}
		Debug.LogError("[DewResources] GetByType failed with type: " + type.Name);
		return null;
	}

	public static T GetByType<T>(Type type) where T : UnityEngine.Object
	{
		if (database.typeToGuid.TryGetValue(type, out var guid))
		{
			return Convert<T>(Load(guid));
		}
		Debug.LogError("[DewResources] GetByType<" + typeof(T).Name + "> failed with type: " + type.Name);
		return null;
	}

	public static UnityEngine.Object GetByShortTypeName(string name)
	{
		if (database.typeNameToType.TryGetValue(name, out var type))
		{
			return GetByType(type);
		}
		Debug.LogError("[DewResources] GetByShortTypeName failed with name: " + name);
		return null;
	}

	public static T GetByShortTypeName<T>(string name) where T : UnityEngine.Object
	{
		return (T)GetByShortTypeName(name);
	}

	public static T GetByGuid<T>(string guid) where T : UnityEngine.Object
	{
		return Convert<T>(Load(guid));
	}

	public static UnityEngine.Object GetByGuid(string guid)
	{
		return Load(guid);
	}

	public static T GetByName<T>(string name) where T : UnityEngine.Object
	{
		if (database.nameToGuid.TryGetValue(name, out var guid))
		{
			return Convert<T>(Load(guid));
		}
		Debug.LogError("[DewResources] GetByName<" + typeof(T).Name + "> failed with name: " + name);
		return null;
	}

	public static UnityEngine.Object GetByName(string name)
	{
		if (database.nameToGuid.TryGetValue(name, out var guid))
		{
			return Load(guid);
		}
		Debug.LogError("[DewResources] GetByName failed with name: " + name);
		return null;
	}

	public static string GetGuidOfAsset(UnityEngine.Object obj)
	{
		if (obj is ILinkedByGuid l)
		{
			return l.resourceId;
		}
		if (database.objectToGuidFallback.TryGetValue(obj, out var id))
		{
			return id;
		}
		if (database.nameToGuid.TryGetValue(obj.name, out var id2))
		{
			return id2;
		}
		Debug.LogError(string.Format("[DewResources] {0} failed with object: {1}", "GetGuidOfAsset", obj));
		return null;
	}

	public static List<string> GetAllDependencies(out ListReturnHandle<string> handle, string objName)
	{
		List<string> list = DewPool.GetList(out handle);
		Add(objName);
		return list;
		void Add(string n)
		{
			if (!database.dependencyConnections.TryGetValue(n, out var deps))
			{
				return;
			}
			foreach (string d in deps)
			{
				if (!(d == objName) && !list.Contains(d))
				{
					list.Add(d);
					Add(d);
				}
			}
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void OnInitPreload()
	{
		_preloadRules.Clear();
	}

	public static void AddPreloadRule(MonoBehaviour owner, Action<PreloadInterface> onPreload)
	{
		if (owner == null)
		{
			return;
		}
		_preloadRules.Add(new PreloadRuleEntry
		{
			owner = owner,
			onPreload = onPreload
		});
		try
		{
			string[] currentGuids = _loadedObjects.Keys.ToArray();
			int preloaded = 0;
			_preloadInterface._guids.Clear();
			onPreload(_preloadInterface);
			foreach (string g in _preloadInterface._guids)
			{
				if (!currentGuids.Contains(g))
				{
					preloaded++;
					Preload(g);
				}
			}
			if (preloaded == 0)
			{
				if (currentGuids.Length != 0)
				{
					Debug.Log($"[DewResources] Preload Rule Added by {owner.name}: {currentGuids.Length} (Nothing changed)");
				}
			}
			else
			{
				Debug.Log($"[DewResources] Preload Rule Added by {owner.name}: {currentGuids.Length} -> {loadedGuids.Count} (+{preloaded})");
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}
}
