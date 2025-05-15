using System.Collections.Generic;

public class DewItemsData
{
	public List<string> encryptedItems = new List<string>();

	public void Validate()
	{
		if (encryptedItems == null)
		{
			encryptedItems = new List<string>();
		}
	}

	public bool Contains(DecryptedItemData data)
	{
		foreach (string encryptedItem in encryptedItems)
		{
			DecryptedItemData dec = DewItem.GetDecryptedItemData(encryptedItem);
			if (dec != null && dec.item == data.item && dec.owner == data.owner)
			{
				return true;
			}
		}
		return false;
	}
}
