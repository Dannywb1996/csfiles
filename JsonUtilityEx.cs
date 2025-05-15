using System;
using System.Collections.Generic;
using UnityEngine;

internal static class JsonUtilityEx
{
	private abstract class ValueWrapper
	{
		private static readonly HashSet<Type> SupportedTypes = new HashSet<Type>
		{
			typeof(string),
			typeof(Vector2Int),
			typeof(Vector3Int),
			typeof(Rect),
			typeof(RectOffset),
			typeof(Bounds),
			typeof(BoundsInt)
		};

		public abstract object obj { get; set; }

		public static Type GetWrapperType(Type type)
		{
			return typeof(ValueWrapper<>).MakeGenericType(type);
		}

		public static bool IsSupport(Type type)
		{
			if (!type.IsPrimitive && !SupportedTypes.Contains(type) && !type.IsArray)
			{
				if (type.IsGenericType)
				{
					return type.GetGenericTypeDefinition() == typeof(List<>);
				}
				return false;
			}
			return true;
		}
	}

	private class ValueWrapper<T> : ValueWrapper
	{
		public T value;

		public override object obj
		{
			get
			{
				return value;
			}
			set
			{
				this.value = (T)value;
			}
		}
	}

	private static Dictionary<Type, ValueWrapper> _wrapperTable;

	public static T FromJson<T>(string json)
	{
		return (T)FromJson(json, typeof(T));
	}

	public static object FromJson(string json, Type type)
	{
		if (ValueWrapper.IsSupport(type))
		{
			Type wrapperType = ValueWrapper.GetWrapperType(type);
			return (JsonUtility.FromJson(json, wrapperType) as ValueWrapper)?.obj;
		}
		return JsonUtility.FromJson(json, type);
	}

	public static string ToJson(object obj)
	{
		return ToJson(obj, prettyPrint: false);
	}

	public static string ToJson(object obj, bool prettyPrint)
	{
		return JsonUtility.ToJson(WrapObject(obj), prettyPrint);
	}

	private static object WrapObject(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		Type type = obj.GetType();
		if (!ValueWrapper.IsSupport(type))
		{
			return obj;
		}
		if (_wrapperTable == null)
		{
			_wrapperTable = new Dictionary<Type, ValueWrapper>();
		}
		if (!_wrapperTable.TryGetValue(type, out var wrapObj))
		{
			wrapObj = (ValueWrapper)Activator.CreateInstance(ValueWrapper.GetWrapperType(type));
			_wrapperTable[type] = wrapObj;
		}
		wrapObj.obj = obj;
		return wrapObj;
	}
}
