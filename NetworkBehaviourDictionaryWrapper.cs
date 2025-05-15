using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;

public class NetworkBehaviourDictionaryWrapper<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TValue : NetworkBehaviour
{
	private readonly IDictionary<TKey, SyncedNetworkBehaviour> _wrapped;

	public int Count => _wrapped.Count;

	public bool IsReadOnly => _wrapped.IsReadOnly;

	public TValue this[TKey key]
	{
		get
		{
			return (TValue)(NetworkBehaviour)_wrapped[key];
		}
		set
		{
			_wrapped[key] = value;
		}
	}

	IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => ((IDictionary<TKey, TValue>)this).Keys;

	IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => ((IDictionary<TKey, TValue>)this).Values;

	public ICollection<TKey> Keys => _wrapped.Keys;

	public ICollection<TValue> Values
	{
		get
		{
			TValue[] values = new TValue[Count];
			int i = 0;
			foreach (SyncedNetworkBehaviour sval in _wrapped.Values)
			{
				values[i] = (TValue)(NetworkBehaviour)sval;
				i++;
			}
			return values;
		}
	}

	public NetworkBehaviourDictionaryWrapper(IDictionary<TKey, SyncedNetworkBehaviour> wrapped)
	{
		_wrapped = wrapped;
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return _wrapped.Select((KeyValuePair<TKey, SyncedNetworkBehaviour> e) => new KeyValuePair<TKey, TValue>(e.Key, (TValue)(NetworkBehaviour)e.Value)).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		_wrapped.Add(new KeyValuePair<TKey, SyncedNetworkBehaviour>(item.Key, item.Value));
	}

	public void Clear()
	{
		_wrapped.Clear();
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		return _wrapped.Contains(new KeyValuePair<TKey, SyncedNetworkBehaviour>(item.Key, item.Value));
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		int i = 0;
		using IEnumerator<KeyValuePair<TKey, TValue>> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<TKey, TValue> pair = enumerator.Current;
			if (arrayIndex + i >= array.Length)
			{
				break;
			}
			array[arrayIndex + i] = pair;
			i++;
		}
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		return _wrapped.Remove(new KeyValuePair<TKey, SyncedNetworkBehaviour>(item.Key, item.Value));
	}

	public void Add(TKey key, TValue value)
	{
		_wrapped.Add(key, value);
	}

	public bool ContainsKey(TKey key)
	{
		return _wrapped.ContainsKey(key);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		SyncedNetworkBehaviour val;
		bool result = _wrapped.TryGetValue(key, out val);
		value = (TValue)(NetworkBehaviour)val;
		return result;
	}

	public bool Remove(TKey key)
	{
		return _wrapped.Remove(key);
	}
}
