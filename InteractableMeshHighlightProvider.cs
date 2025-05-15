public class InteractableMeshHighlightProvider : MeshHighlightProvider
{
	protected override void Start()
	{
		base.Start();
		base.meshHighlight.outlineColor = ManagerBase<ObjectHighlightManager>.instance.interactable.color;
	}
}
