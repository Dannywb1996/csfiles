using System.Linq;
using UnityEngine;

[DewResourceLink(ResourceLinkBy.Type)]
public class Artifact : Actor, IInteractable, ICustomInteractable, IExcludeFromPool
{
	public Sprite icon;

	[ColorUsage(false)]
	public Color mainColor;

	public DewAudioClip touchSound;

	public bool excludeFromPool;

	public int grantedStardust = 10;

	private ArtifactWorldModel _worldModel;

	public Transform interactPivot => _worldModel.interactPivot;

	public bool canInteractWithMouse => true;

	public float focusDistance => 3.75f;

	public int priority => 10;

	bool IExcludeFromPool.excludeFromPool => excludeFromPool;

	public string nameRawText => DewLocalization.GetArtifactName(DewLocalization.GetArtifactKey(GetType().Name));

	public string interactActionRawText => DewLocalization.GetUIValue("InGame_Tooltip_PickUp");

	public string interactAltActionRawText => null;

	public float? altInteractProgress => null;

	public Cost cost => default(Cost);

	public bool canAltInteract => false;

	protected override void OnCreate()
	{
		base.OnCreate();
		GameObject prefab = Resources.Load<GameObject>("WorldModels/Artifact World Model");
		_worldModel = Object.Instantiate(prefab, base.position, base.rotation, base.transform).GetComponent<ArtifactWorldModel>();
		_worldModel.Setup(this);
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		FxStop(_worldModel.fxAppear);
		FxStop(_worldModel.fxLoop);
	}

	public bool CanInteract(Entity entity)
	{
		return Time.time - base.creationTime > 1f;
	}

	public void OnInteract(Entity entity, bool alt)
	{
		if (alt)
		{
			return;
		}
		if (entity.isOwned && NetworkedManagerBase<QuestManager>.instance.currentArtifact != null)
		{
			InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_AlreadyHasArtifact");
		}
		if (base.isServer && NetworkedManagerBase<QuestManager>.instance.currentArtifact == null)
		{
			FxPlayNetworked(_worldModel.fxPickUp, entity);
			NetworkedManagerBase<QuestManager>.instance.PickUpArtifact(GetType().Name, entity.owner, _worldModel.icon.transform.position);
			NetworkedManagerBase<QuestManager>.instance.didCollectArtifactThisLoop = true;
			Destroy();
			if (!NetworkedManagerBase<ZoneManager>.instance.nodes.Any((WorldNodeData n) => n.HasModifier("RoomMod_DreamTeller")) && NetworkedManagerBase<ZoneManager>.instance.TryGetNodeIndexForNextGoal(new GetNodeIndexSettings
			{
				allowedTypes = new WorldNodeType[1] { WorldNodeType.Combat },
				preferCloserToExit = true,
				desiredDistance = new Vector2Int(2, 4)
			}, out var nodeIndex))
			{
				NetworkedManagerBase<ZoneManager>.instance.AddModifier<RoomMod_DreamTeller>(nodeIndex);
			}
		}
	}

	private void MirrorProcessed()
	{
	}
}
