using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;

public class NetworkBehaviourListWrapper<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T> where T : NetworkBehaviour
{
	private readonly IList<SyncedNetworkBehaviour> _wrapped;

	public int Count => _wrapped.Count;

	public bool IsReadOnly => _wrapped.IsReadOnly;

	public T this[int index]
	{
		get
		{
			return (T)(NetworkBehaviour)_wrapped[index];
		}
		set
		{
			_wrapped[index] = value;
		}
	}

	public NetworkBehaviourListWrapper(IList<SyncedNetworkBehaviour> wrapped)
	{
		_wrapped = wrapped;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _wrapped.Select((SyncedNetworkBehaviour e) => (T)(NetworkBehaviour)e).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(T item)
	{
		_wrapped.Add(item);
	}

	public void Clear()
	{
		_wrapped.Clear();
	}

	public bool Contains(T item)
	{
		return _wrapped.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		for (int i = 0; i < Count && arrayIndex + i < array.Length; i++)
		{
			array[arrayIndex + i] = this[i];
		}
	}

	public bool Remove(T item)
	{
		return _wrapped.Remove(item);
	}

	public int IndexOf(T item)
	{
		return _wrapped.IndexOf(item);
	}

	public void Insert(int index, T item)
	{
		_wrapped.Insert(index, item);
	}

	public void RemoveAt(int index)
	{
		_wrapped.RemoveAt(index);
	}
}
