using UnityEngine;

[LogicUpdatePriority(-380)]
public class ManagerBase<T> : LogicBehaviour where T : LogicBehaviour
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

	protected override void OnEnable()
	{
		base.OnEnable();
		if (softInstance == null || !softInstance.enabled)
		{
			softInstance = this as T;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (softInstance == this)
		{
			softInstance = Object.FindObjectOfType<T>();
		}
	}
}
