using UnityEngine;

[DisallowMultipleComponent]
public class FxSelectiveVisibility : MonoBehaviour, ISerializationCallbackReceiver
{
	public static bool forceFail;

	public EffectVisibility visibleTo;

	public bool invertVisibility;

	[HideInInspector]
	public Actor parent;

	[HideInInspector]
	public bool wasDisabled;

	private string _helpText => "Parent: " + ((parent == null) ? ((object)"null") : ((object)parent));

	public bool IsVisibleLocally()
	{
		if (parent == null && visibleTo != EffectVisibility.Everyone)
		{
			Debug.LogWarning("Selectively visible effect '" + base.name + "' has no proper parent");
			return invertVisibility;
		}
		if (forceFail && visibleTo != EffectVisibility.Everyone)
		{
			return invertVisibility;
		}
		DewPlayer local = null;
		if (ManagerBase<CameraManager>.instance != null && ManagerBase<CameraManager>.instance.focusedEntity != null)
		{
			local = ManagerBase<CameraManager>.instance.focusedEntity.owner;
		}
		switch (visibleTo)
		{
		case EffectVisibility.Everyone:
			return !invertVisibility;
		case EffectVisibility.Caster:
		{
			Entity caster = ((AbilityInstance)parent).info.caster;
			if (caster == null)
			{
				return invertVisibility;
			}
			if (caster.owner != local)
			{
				return invertVisibility;
			}
			return !invertVisibility;
		}
		case EffectVisibility.Target:
		{
			Entity target = ((AbilityInstance)parent).info.target;
			if (target == null)
			{
				return invertVisibility;
			}
			if (target.owner != local)
			{
				return invertVisibility;
			}
			return !invertVisibility;
		}
		case EffectVisibility.Victim:
		{
			Entity victim = ((StatusEffect)parent).victim;
			if (victim == null)
			{
				return invertVisibility;
			}
			if (victim.owner != local)
			{
				return invertVisibility;
			}
			return !invertVisibility;
		}
		case EffectVisibility.Owner:
			if (parent.firstEntity.owner != local)
			{
				return invertVisibility;
			}
			return !invertVisibility;
		default:
			return !invertVisibility;
		}
	}

	private void OnValidate()
	{
		if (Application.IsPlaying(this) && parent == null && visibleTo != EffectVisibility.Everyone)
		{
			Debug.LogWarning("Selectively visible effect '" + base.name + "' has no proper parent");
		}
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
	}
}
