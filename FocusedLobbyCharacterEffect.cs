using UnityEngine;

public class FocusedLobbyCharacterEffect : MonoBehaviour, ILobbyCharacterModelOnFocus
{
	public void OnLobbyCharacterFocus(bool value)
	{
		if (value)
		{
			DewEffect.Play(base.gameObject);
		}
		else
		{
			DewEffect.Stop(base.gameObject);
		}
	}
}
