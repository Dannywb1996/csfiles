using System;
using System.Collections.Generic;

public class VariantDataContainer
{
	private Dictionary<Type, object> _data = new Dictionary<Type, object>();

	public bool Has<T>()
	{
		return _data.ContainsKey(typeof(T));
	}

	public bool TryGet<T>(out T data)
	{
		object dataRaw;
		bool result = _data.TryGetValue(typeof(T), out dataRaw);
		data = ((dataRaw is T castData) ? castData : default(T));
		return result;
	}

	public T Get<T>()
	{
		return (T)_data[typeof(T)];
	}

	public void Set<T>(T data)
	{
		_data[typeof(T)] = data;
	}
}
