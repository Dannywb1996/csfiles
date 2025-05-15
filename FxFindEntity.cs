using UnityEngine;

public class FxFindEntity : MonoBehaviour, IEffectSetupComponent
{
	public string findType;

	public bool exactMatch;

	public void OnEffectSetup()
	{
		if (NetworkedManagerBase<ActorManager>.instance == null || string.IsNullOrEmpty(findType))
		{
			return;
		}
		string substr = findType.Trim();
		Entity target = null;
		foreach (Entity e in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if ((exactMatch && e.GetType().Name == substr) || (!exactMatch && e.GetType().Name.Contains(substr)))
			{
				target = e;
			}
		}
		if (target == null)
		{
			Debug.Log("FxFindEntity target not found: " + substr);
			return;
		}
		Debug.Log("FxFindEntity target: " + target.GetActorReadableName());
		ListReturnHandle<IAttachableToEntity> handle;
		foreach (IAttachableToEntity item in ((Component)this).GetComponentsInChildrenNonAlloc(out handle))
		{
			item.OnAttachToEntity(target);
		}
		handle.Return();
	}
}
