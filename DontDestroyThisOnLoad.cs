using UnityEngine;

public class DontDestroyThisOnLoad : MonoBehaviour
{
	private void Start()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}
}
