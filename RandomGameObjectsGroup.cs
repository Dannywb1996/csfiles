using UnityEngine;

public class RandomGameObjectsGroup : MonoBehaviour
{
	public GameObject[] gameObjects;

	private void Awake()
	{
		if (gameObjects != null && gameObjects.Length != 0)
		{
			GameObject[] array = gameObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
			gameObjects[Random.Range(0, gameObjects.Length)].SetActive(value: true);
		}
	}
}
