using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemIdDatabase", menuName = "Dew Item Id Database")]
public class ItemNumericIdDatabase : SerializedScriptableObject
{
	[SerializeField]
	private uint _nextEmptyId = 1u;

	[SerializeField]
	private Dictionary<string, uint> _itemToId = new Dictionary<string, uint>();

	[SerializeField]
	private Dictionary<uint, string> _idToItem = new Dictionary<uint, string>();

	public static ItemNumericIdDatabase instance => Resources.Load<ItemNumericIdDatabase>("ItemIdDatabase");

	private void RegisterNewItem(string item)
	{
		_itemToId.Add(item, _nextEmptyId);
		_idToItem.Add(_nextEmptyId, item);
		checked
		{
			_nextEmptyId++;
		}
	}

	public bool TryGetHash(string item, out uint id)
	{
		return _itemToId.TryGetValue(item, out id);
	}

	public bool TryGetItem(uint id, out string item)
	{
		return _idToItem.TryGetValue(id, out item);
	}
}
