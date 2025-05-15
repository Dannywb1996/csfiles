using UnityEngine;

public class SingletonDewNetworkBehaviour<T> : DewNetworkBehaviour where T : MonoBehaviour
{
	public static T instance
	{
		get
		{
			if (softInstance == null)
			{
				softInstance = Object.FindObjectOfType<T>();
			}
			if (softInstance == null)
			{
				softInstance = Object.FindObjectOfType<T>(includeInactive: true);
			}
			return softInstance;
		}
	}

	public static T softInstance { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		softInstance = this as T;
	}

	private void MirrorProcessed()
	{
	}
}
