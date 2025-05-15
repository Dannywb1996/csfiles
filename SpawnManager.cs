using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class SpawnManager : ManagerBase<SpawnManager>
{
	private void Start()
	{
		foreach (KeyValuePair<uint, string> item in DewResources.database.netObjectAssetIdToGuid)
		{
			NetworkClient.RegisterSpawnHandler(item.Key, SpawnFromDewDatabaseHandler, CustomDespawnHandler);
		}
	}

	private GameObject SpawnFromDewDatabaseHandler(SpawnMessage msg)
	{
		GameObject prefab = DewResources.GetNetworkedPrefab(msg.assetId);
		GameObject obj = Object.Instantiate(prefab, msg.position, msg.rotation, null);
		obj.transform.localScale = msg.scale;
		obj.name = prefab.name;
		return obj;
	}

	private void CustomDespawnHandler(GameObject gobj)
	{
		ListReturnHandle<ICustomDestroyRoutine> handle;
		List<ICustomDestroyRoutine> handlers = gobj.GetComponentsNonAlloc(out handle);
		if (handlers.Count == 0)
		{
			Object.Destroy(gobj);
		}
		else
		{
			foreach (ICustomDestroyRoutine item in handlers)
			{
				item.CustomDestroyRoutine();
			}
		}
		handle.Return();
	}
}
