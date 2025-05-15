using System;
using UnityEngine;

public class SkillWorldModel : ItemWorldModel
{
	private float _appearTime;

	protected override void Awake()
	{
		base.Awake();
		SkillTrigger obj = (SkillTrigger)base.item;
		obj.ClientSkillEvent_OnDismantleProgressChanged += new Action<float, float>(base.OnDismantleProgressChanged);
		EditSkillManager instance = ManagerBase<EditSkillManager>.instance;
		instance.OnSkillVisibleClientState = (Action<SkillTrigger, bool>)Delegate.Combine(instance.OnSkillVisibleClientState, new Action<SkillTrigger, bool>(SkillVisibleClientState));
		_appearTime = Time.time;
		obj.ClientEvent_OnTempOwnerChanged += (Action<DewPlayer, DewPlayer>)delegate
		{
			UpdateVisibility();
		};
		obj.ClientEvent_OnHandOwnerChanged += (Action<Hero, Hero>)delegate
		{
			UpdateVisibility();
		};
		obj.ClientEvent_OnOwnerChanged += (Action<Entity, Entity>)delegate
		{
			UpdateVisibility();
		};
	}

	protected override void OnAppearInAnimation()
	{
		base.OnAppearInAnimation();
		_appearTime = Time.time;
	}

	private void Update()
	{
		iconQuad.transform.localPosition = Vector3.up * (2f + Mathf.Sin((Time.time - _appearTime) * 2f) * 0.125f);
	}

	private void SkillVisibleClientState(SkillTrigger arg1, bool arg2)
	{
		if (!((SkillTrigger)base.item != arg1))
		{
			UpdateVisibilityLocal(!arg2);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (ManagerBase<EditSkillManager>.instance != null)
		{
			EditSkillManager instance = ManagerBase<EditSkillManager>.instance;
			instance.OnSkillVisibleClientState = (Action<SkillTrigger, bool>)Delegate.Remove(instance.OnSkillVisibleClientState, new Action<SkillTrigger, bool>(SkillVisibleClientState));
		}
	}

	protected override Texture GetIconTexture()
	{
		return ((SkillTrigger)base.item).configs[0].triggerIcon?.texture;
	}
}
