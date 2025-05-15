using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DewInternal;

[CreateAssetMenu(fileName = "New Dew Resource Database", menuName = "Dew Resource Database", order = 1)]
public class DewResourceDatabase : SerializedScriptableObject
{
	private static readonly string[] Paths = new string[3] { "Assets/Dew", "Assets/DewCore", "Assets/Res/Animations" };

	public Dictionary<string, string> typeAssemblyQualifiedNameToGuid = new Dictionary<string, string>();

	public Dictionary<string, string> nameToGuid = new Dictionary<string, string>();

	public Dictionary<uint, string> netObjectAssetIdToGuid = new Dictionary<uint, string>();

	public List<string> sceneNames = new List<string>();

	public Dictionary<UnityEngine.Object, string> objectToGuidFallback = new Dictionary<UnityEngine.Object, string>();

	public List<string> excludedFromPoolObjects = new List<string>();

	public Dictionary<string, List<string>> dependencyConnections = new Dictionary<string, List<string>>();

	public Dictionary<string, List<string>> deepDependencyConnections = new Dictionary<string, List<string>>();

	public List<string> strippedTypes = new List<string>();

	public List<string> allGuids = new List<string>();

	[NonSerialized]
	public Dictionary<Type, string> typeToGuid = new Dictionary<Type, string>();

	[NonSerialized]
	public Dictionary<string, Type> guidToType = new Dictionary<string, Type>();

	[NonSerialized]
	public Dictionary<string, string> typeNameToGuid = new Dictionary<string, string>();

	[NonSerialized]
	public Dictionary<string, Type> typeNameToType = new Dictionary<string, Type>();

	public bool InitForRuntime()
	{
		typeToGuid.Clear();
		guidToType.Clear();
		typeNameToGuid.Clear();
		typeNameToType.Clear();
		bool hasError = false;
		foreach (KeyValuePair<string, string> pair in typeAssemblyQualifiedNameToGuid)
		{
			try
			{
				Type type = Type.GetType(pair.Key);
				if (type == null)
				{
					Debug.LogWarning("Type '" + pair.Key + "' not found in current domain, possibly stale database?");
					continue;
				}
				typeToGuid.Add(type, pair.Value);
				guidToType.Add(pair.Value, type);
				typeNameToGuid.Add(type.Name, pair.Value);
				typeNameToType.Add(type.Name, type);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception, this);
				hasError = true;
			}
		}
		return !hasError;
	}
}
