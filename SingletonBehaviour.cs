using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
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

	protected virtual void Awake()
	{
		softInstance = this as T;
	}
}
