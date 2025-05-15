using UnityEngine;

public class LogicBehaviour : MonoBehaviour, ILogicUpdate
{
	private bool _didRegisterLogic;

	public virtual bool shouldRegisterUpdates => true;

	protected virtual void OnEnable()
	{
		if (shouldRegisterUpdates)
		{
			LogicUpdateManager.Register(this);
			_didRegisterLogic = true;
		}
	}

	protected virtual void OnDisable()
	{
		if (_didRegisterLogic)
		{
			LogicUpdateManager.Unregister(this);
			_didRegisterLogic = false;
		}
	}

	public virtual void LogicUpdate(float dt)
	{
	}

	public virtual void FrameUpdate()
	{
	}

	protected void GetComponent<T>(out T comp)
	{
		comp = GetComponent<T>();
	}
}
