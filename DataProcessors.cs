using System;
using System.Collections.Generic;
using UnityEngine;

public class DataProcessors<T> where T : struct
{
	private struct DataProcessorEntry
	{
		public int priority;

		public DataProcessor<T> processor;
	}

	private List<DataProcessorEntry> _entries;

	public void Add(DataProcessor<T> processor, int priority = 0)
	{
		if (_entries == null)
		{
			_entries = new List<DataProcessorEntry>();
		}
		int insertIndex = _entries.Count;
		for (int i = 0; i < _entries.Count; i++)
		{
			if (_entries[i].priority > priority)
			{
				insertIndex = i;
				break;
			}
		}
		_entries.Insert(insertIndex, new DataProcessorEntry
		{
			priority = priority,
			processor = processor
		});
	}

	public void Remove(DataProcessor<T> processor)
	{
		if (_entries == null)
		{
			return;
		}
		for (int i = _entries.Count - 1; i >= 0; i--)
		{
			if (_entries[i].processor == processor)
			{
				_entries.RemoveAt(i);
			}
		}
	}

	public void Process(ref T data, Actor actor, Entity target)
	{
		if (_entries == null)
		{
			return;
		}
		for (int i = 0; i < _entries.Count; i++)
		{
			try
			{
				_entries[i].processor(ref data, actor, target);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}
}
