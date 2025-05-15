using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Room_Shortcut : DewNetworkBehaviour, IInteractable, ICustomInteractable, IPlayerPathableArea
{
	public Room_Shortcut targetShortcut;

	public Transform walkStartPos;

	public Transform walkEndPos;

	public GameObject openEffect;

	public GameObject closedEffect;

	[SyncVar(hook = "OnIsOpenChanged")]
	public bool isOpen;

	Transform IInteractable.interactPivot => base.transform;

	bool IInteractable.canInteractWithMouse => false;

	float IInteractable.focusDistance => 2.5f;

	int IInteractable.priority => 101;

	string ICustomInteractable.nameRawText => DewLocalization.GetUIValue("InGame_Interact_Shortcut");

	string ICustomInteractable.interactActionRawText => DewLocalization.GetUIValue("InGame_Interact_Shortcut_Enter");

	string ICustomInteractable.interactAltActionRawText => null;

	public float? altInteractProgress => null;

	public Cost cost => default(Cost);

	bool ICustomInteractable.canAltInteract => false;

	Vector3 IPlayerPathableArea.pathablePosition => Dew.GetPositionOnGround(walkStartPos.position);

	public bool NetworkisOpen
	{
		get
		{
			return isOpen;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isOpen, 1uL, OnIsOpenChanged);
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		OnIsOpenChanged(oldValue: false, isOpen);
	}

	private void OnIsOpenChanged(bool oldValue, bool newValue)
	{
		if (newValue)
		{
			if (openEffect != null)
			{
				FxPlay(openEffect);
			}
			if (closedEffect != null)
			{
				FxStop(closedEffect);
			}
		}
		else
		{
			if (openEffect != null)
			{
				FxStop(openEffect);
			}
			if (closedEffect != null)
			{
				FxPlay(closedEffect);
			}
		}
	}

	[Server]
	public void Open()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Shortcut::Open()' called when server was not active");
		}
		else
		{
			NetworkisOpen = true;
		}
	}

	[Server]
	public void Close()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Room_Shortcut::Close()' called when server was not active");
		}
		else
		{
			NetworkisOpen = false;
		}
	}

	private void OnValidate()
	{
		if (targetShortcut != null)
		{
			targetShortcut.targetShortcut = this;
		}
	}

	private void OnDrawGizmos()
	{
		if (walkStartPos != null && walkEndPos != null)
		{
			DewGizmos.DrawArrow(walkStartPos.position, walkEndPos.position, Color.magenta, 1f);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (targetShortcut != null)
		{
			DewGizmos.DrawLine(base.transform.position, targetShortcut.transform.position, Color.magenta);
		}
	}

	bool IInteractable.CanInteract(Entity entity)
	{
		return true;
	}

	void IInteractable.OnInteract(Entity entity, bool alt)
	{
		if (!base.isServer)
		{
			return;
		}
		if (entity is Hero { isInCombat: not false } h)
		{
			h.owner.TpcShowCenterMessage(CenterMessageType.Error, "InGame_Message_ShortcutUnavailableInCombat");
		}
		else if (!entity.Status.HasStatusEffect<Se_UsingShortcut>())
		{
			entity.CreateStatusEffect(entity, default(CastInfo), delegate(Se_UsingShortcut se)
			{
				se.startShortcut = this;
				se.targetShortcut = targetShortcut;
			});
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(isOpen);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(isOpen);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref isOpen, OnIsOpenChanged, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isOpen, OnIsOpenChanged, reader.ReadBool());
		}
	}
}
