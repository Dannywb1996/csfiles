using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.AI;

public class Rift_RoomExit : Rift, IBanRoomNodesNearbyMultiple
{
	public new static Rift_RoomExit instance { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		instance = this;
	}

	protected override bool OnInteractRift(Hero hero)
	{
		TpcInteract(hero.owner);
		return true;
	}

	[TargetRpc]
	public void TpcInteract(NetworkConnectionToClient target)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendTargetRPCInternal(target, "System.Void Rift_RoomExit::TpcInteract(Mirror.NetworkConnectionToClient)", -397471636, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	public void GetSidetrackPortalPositions(List<(Vector3, Quaternion)> list)
	{
		Vector3 riftPos = base.transform.position;
		Vector3 riftNavPos = Dew.GetValidAgentPosition(riftPos);
		Vector3 center = riftPos + base.transform.forward * 8f;
		float baseAngle = base.transform.eulerAngles.y;
		if (list.Count <= 3 && TryPos(new Vector3(3.75f, 0f, 1f), out var pos, out var rot))
		{
			list.Add((pos, rot));
		}
		if (list.Count <= 3 && TryPos(new Vector3(-3.75f, 0f, 1f), out pos, out rot))
		{
			list.Add((pos, rot));
		}
		if (list.Count <= 3 && TryPos(new Vector3(6.75f, 0f, 2.75f), out pos, out rot))
		{
			list.Add((pos, rot));
		}
		if (list.Count <= 3 && TryPos(new Vector3(-6.75f, 0f, 2.75f), out pos, out rot))
		{
			list.Add((pos, rot));
		}
		if (list.Count <= 3 && TryPos(new Vector3(2.55f, 0f, 3.85f), out pos, out rot))
		{
			list.Add((pos, rot));
		}
		if (list.Count <= 3 && TryPos(new Vector3(-2.55f, 0f, 3.85f), out pos, out rot))
		{
			list.Add((pos, rot));
		}
		bool TryPos(Vector3 offset, out Vector3 reference, out Quaternion reference2)
		{
			Quaternion baseRot = Quaternion.Euler(0f, baseAngle, 0f);
			Vector3 originalPos = (reference = center + baseRot * Vector3.forward * -8f + baseRot * offset);
			reference = Dew.GetPositionOnGround(originalPos);
			reference = Dew.GetValidAgentPosition(reference, 1f);
			if (Vector3.Distance(reference, originalPos) > 1f)
			{
				reference2 = Quaternion.identity;
				return false;
			}
			reference2 = Quaternion.LookRotation(center - reference);
			return Dew.GetNavMeshPathStatus(riftNavPos, reference) == NavMeshPathStatus.PathComplete;
		}
	}

	public Vector3[] GetBanSpots()
	{
		List<(Vector3, Quaternion)> sidetracks = new List<(Vector3, Quaternion)>();
		GetSidetrackPortalPositions(sidetracks);
		Vector3[] arr = new Vector3[sidetracks.Count + 1];
		arr[0] = base.transform.position;
		for (int i = 0; i < sidetracks.Count; i++)
		{
			arr[i + 1] = sidetracks[i].Item1;
		}
		return arr;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcInteract__NetworkConnectionToClient(NetworkConnectionToClient target)
	{
		InGameUIManager.instance.currentMockExit = this as Rift_MockExit;
		if (NetworkedManagerBase<ZoneManager>.instance.currentNode.type != WorldNodeType.ExitBoss)
		{
			InGameUIManager.instance.isWorldDisplayed = WorldDisplayStatus.Shown;
			return;
		}
		int sumOfZones = DewBuildProfile.current.content.zoneCountByTier.Sum();
		if (DewBuildProfile.current.buildType == BuildType.DemoLite && DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth) && NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex >= sumOfZones - 1)
		{
			ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
			{
				rawContent = DewLocalization.GetUIValue("InGame_Message_DemoFinished"),
				buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
				defaultButton = DewMessageSettings.ButtonType.No,
				destructiveConfirm = true,
				onClose = delegate(DewMessageSettings.ButtonType b)
				{
					if (b == DewMessageSettings.ButtonType.Yes)
					{
						NetworkedManagerBase<ZoneManager>.instance.CmdTravelToNextZone();
					}
				},
				validator = () => InGameUIManager.ValidateInGameActionMessage()
			});
		}
		else if (NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex % sumOfZones == sumOfZones - 1)
		{
			ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
			{
				rawContent = DewLocalization.GetUIValue("InGame_Message_DreamAgain"),
				buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
				defaultButton = DewMessageSettings.ButtonType.No,
				destructiveConfirm = true,
				onClose = delegate(DewMessageSettings.ButtonType b)
				{
					if (b == DewMessageSettings.ButtonType.Yes)
					{
						NetworkedManagerBase<ZoneManager>.instance.CmdTravelToNextZone();
					}
				},
				validator = () => InGameUIManager.ValidateInGameActionMessage()
			});
		}
		else
		{
			NetworkedManagerBase<ZoneManager>.instance.TravelWithValidationAndConfirmation(delegate
			{
				NetworkedManagerBase<ZoneManager>.instance.CmdTravelToNextZone();
			});
		}
	}

	protected static void InvokeUserCode_TpcInteract__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcInteract called on server.");
		}
		else
		{
			((Rift_RoomExit)obj).UserCode_TpcInteract__NetworkConnectionToClient((NetworkConnectionToClient)NetworkClient.connection);
		}
	}

	static Rift_RoomExit()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Rift_RoomExit), "System.Void Rift_RoomExit::TpcInteract(Mirror.NetworkConnectionToClient)", InvokeUserCode_TpcInteract__NetworkConnectionToClient);
	}
}
