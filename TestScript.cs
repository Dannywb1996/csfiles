using System.Collections;
using TMPro;
using UnityEngine;

public class TestScript : MonoBehaviour
{
	private void OnEnable()
	{
		SingletonBehaviour<UI_GamepadTextInput>.instance.StartInput(GetComponent<TMP_InputField>());
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(1f);
			SingletonBehaviour<UI_GamepadTextInput>.instance.StartInput(GetComponent<TMP_InputField>());
		}
	}
}
