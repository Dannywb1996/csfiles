using System;
using System.Collections;
using Mirror;
using UnityEngine;

public abstract class Room_Trap_ActivatorBase : DewNetworkBehaviour
{
	public bool toggleTraps;

	public float durationOfToggledTraps;

	public GameObject[] activatedObjects;

	public GameObject fxActivateEffect;

	[Server]
	public void Activate()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Trap_ActivatorBase::Activate()' called when server was not active");
			return;
		}
		FxPlayNetworked(fxActivateEffect);
		HandleGameObject(base.gameObject);
		GameObject[] array = activatedObjects;
		foreach (GameObject gobj in array)
		{
			HandleGameObject(gobj);
		}
		void HandleGameObject(GameObject gameObject)
		{
			if (!(gameObject == null))
			{
				ListReturnHandle<IActivatableTrap> handle;
				foreach (IActivatableTrap a in gameObject.GetComponentsNonAlloc(out handle))
				{
					if (ValidateObject(a))
					{
						try
						{
							a.ActivateTrap();
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
						}
					}
				}
				handle.Return();
				ListReturnHandle<IToggleableTrap> handle2;
				foreach (IToggleableTrap item in gameObject.GetComponentsNonAlloc(out handle2))
				{
					IToggleableTrap t = item;
					if (ValidateObject(t))
					{
						try
						{
							if (toggleTraps)
							{
								if (t.isOn)
								{
									t.StopTrap();
								}
								else
								{
									t.StartTrap();
								}
							}
							else
							{
								StartCoroutine(Routine());
							}
						}
						catch (Exception exception2)
						{
							Debug.LogException(exception2);
						}
					}
					IEnumerator Routine()
					{
						t.StartTrap();
						yield return new WaitForSeconds(durationOfToggledTraps);
						if (ValidateObject(t))
						{
							t.StopTrap();
						}
					}
				}
				handle2.Return();
			}
		}
		static bool ValidateObject(object obj)
		{
			if (obj is Actor { isActive: false })
			{
				return false;
			}
			if (obj is MonoBehaviour { isActiveAndEnabled: false })
			{
				return false;
			}
			if (obj is NetworkBehaviour { isServer: false })
			{
				return false;
			}
			return true;
		}
	}

	private void MirrorProcessed()
	{
	}
}
