using UnityEngine;

[LogicUpdatePriority(-300)]
public class NetworkedManagerBase<T> : DewNetworkBehaviour where T : DewNetworkBehaviour
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
		softInstance = this as T;
		base.Awake();
	}

	protected virtual void OnEnable()
	{
		if (softInstance == null || !softInstance.enabled)
		{
			softInstance = this as T;
		}
	}

	private void MirrorProcessed()
	{
	}
}
