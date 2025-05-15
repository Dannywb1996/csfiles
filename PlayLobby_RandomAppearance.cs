using UnityEngine;

public class PlayLobby_RandomAppearance : MonoBehaviour
{
	public float appearChance;

	private void Start()
	{
		base.gameObject.SetActive(Random.value < appearChance);
	}
}
