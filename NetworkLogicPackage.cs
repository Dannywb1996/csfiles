using Mirror;
using UnityEngine;

public class NetworkLogicPackage : ManagerBase<NetworkLogicPackage>
{
	private void Start()
	{
		if (ManagerBase<NetworkLogicPackage>.instance != null && ManagerBase<NetworkLogicPackage>.instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		NetworkIdentity[] array = Object.FindObjectsOfType<NetworkIdentity>(includeInactive: true);
		for (int i = 0; i < array.Length; i++)
		{
			Object.Destroy(array[i].gameObject);
		}
	}
}
