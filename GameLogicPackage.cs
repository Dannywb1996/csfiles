using UnityEngine;

public class GameLogicPackage : ManagerBase<GameLogicPackage>
{
	private void Start()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}
}
