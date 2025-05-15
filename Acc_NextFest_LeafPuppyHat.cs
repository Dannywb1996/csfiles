using System.Collections;
using UnityEngine;

public class Acc_NextFest_LeafPuppyHat : MonoBehaviour
{
	private static readonly int Bark = Animator.StringToHash("Bark");

	public Transform pawL;

	public Transform pawR;

	private Animator _animator;

	private void Awake()
	{
		_animator = GetComponentInChildren<Animator>();
	}

	private void OnEnable()
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			while (true)
			{
				yield return new WaitForSeconds(10f);
				_animator.SetTrigger(Bark);
			}
		}
	}

	private void LateUpdate()
	{
		pawL.localRotation = Quaternion.Euler(0f, 0f, 0f);
		pawR.localRotation = Quaternion.Euler(0f, 0f, 0f);
	}
}
