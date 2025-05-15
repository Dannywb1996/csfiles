using System;
using DG.Tweening;
using UnityEngine;

public class GemWorldModel : ItemWorldModel
{
	protected override void Awake()
	{
		base.Awake();
		Gem obj = (Gem)base.item;
		obj.onDismantleProgressChanged = (Action<float, float>)Delegate.Combine(obj.onDismantleProgressChanged, new Action<float, float>(base.OnDismantleProgressChanged));
		EditSkillManager instance = ManagerBase<EditSkillManager>.instance;
		instance.OnGemVisibleClientState = (Action<Gem, bool>)Delegate.Combine(instance.OnGemVisibleClientState, new Action<Gem, bool>(GemVisibleClientState));
		obj.ClientEvent_OnTempOwnerChanged += (Action<DewPlayer, DewPlayer>)delegate
		{
			UpdateVisibility();
		};
		obj.ClientEvent_OnHandOwnerChanged += (Action<Hero, Hero>)delegate
		{
			UpdateVisibility();
		};
		obj.ClientEvent_OnOwnerChanged += (Action<Hero, Hero>)delegate
		{
			UpdateVisibility();
		};
	}

	private void GemVisibleClientState(Gem gem, bool visible)
	{
		if (!((Gem)base.item != gem))
		{
			UpdateVisibilityLocal(!visible);
		}
	}

	protected override void OnAppearInAnimation()
	{
		base.OnAppearInAnimation();
		Vector3 originalPos = iconQuad.transform.localPosition;
		iconQuad.transform.localPosition = Vector3.up * 2f;
		DOTween.Sequence().Append(iconQuad.transform.DOLocalMoveY(originalPos.y, 0.8f).SetEase(Ease.OutBounce));
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		iconQuad.transform.DOKill(complete: true);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (ManagerBase<EditSkillManager>.instance != null)
		{
			EditSkillManager instance = ManagerBase<EditSkillManager>.instance;
			instance.OnGemVisibleClientState = (Action<Gem, bool>)Delegate.Remove(instance.OnGemVisibleClientState, new Action<Gem, bool>(GemVisibleClientState));
		}
	}

	protected override Texture GetIconTexture()
	{
		return ((Gem)base.item).icon.texture;
	}
}
