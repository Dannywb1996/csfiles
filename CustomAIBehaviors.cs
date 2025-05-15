using System;
using System.Collections.Generic;
using UnityEngine;

public class CustomAIBehaviors
{
	private struct CustomAIEntry
	{
		public int priority;

		public CustomAIUpdate update;
	}

	private List<CustomAIEntry> _entries;

	public void Add(CustomAIUpdate onUpdate, int priority = 0)
	{
		if (_entries == null)
		{
			_entries = new List<CustomAIEntry>();
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
		_entries.Insert(insertIndex, new CustomAIEntry
		{
			priority = priority,
			update = onUpdate
		});
	}

	public void Remove(CustomAIUpdate onUpdate)
	{
		if (_entries == null)
		{
			return;
		}
		for (int i = _entries.Count - 1; i >= 0; i--)
		{
			if (_entries[i].update == onUpdate)
			{
				_entries.RemoveAt(i);
			}
		}
	}

	internal bool Execute(ref EntityAIContext context)
	{
		if (_entries == null)
		{
			return false;
		}
		for (int i = 0; i < _entries.Count; i++)
		{
			try
			{
				if (_entries[i].update(ref context))
				{
					return true;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		return false;
	}
}
