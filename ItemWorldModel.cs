using System;
using System.Collections;
using UnityEngine;

public abstract class ItemWorldModel : MonoBehaviour
{
	public GameObject[] tintedObjects;

	public MeshRenderer iconQuad;

	public GameObject fxPrepare;

	public GameObject fxAppear;

	public GameObject fxLoop;

	public GameObject fxDismantleTap;

	public GameObject fxDismantle;

	public float prepareDuration = 0.9f;

	private Material _mat;

	private bool _didAnimate;

	public bool isAnimating { get; private set; }

	public IItem item { get; private set; }

	protected abstract Texture GetIconTexture();

	protected void OnDismantleProgressChanged(float arg1, float arg2)
	{
		if (!(arg1 > arg2))
		{
			if (arg2 > 1f)
			{
				DewEffect.PlayNew(fxDismantle);
			}
			else
			{
				DewEffect.PlayNew(fxDismantleTap);
			}
		}
	}

	protected virtual void Awake()
	{
		item = GetComponentInParent<IItem>();
		_mat = UnityEngine.Object.Instantiate(iconQuad.sharedMaterial);
		_mat.SetTexture("_MainTex", GetIconTexture());
		GetComponent<MaterialHighlightProvider>().targetMaterial = _mat;
		iconQuad.material = _mat;
		Color color = Dew.GetRarityColor(item.rarity);
		GameObject[] array = tintedObjects;
		foreach (GameObject o in array)
		{
			DewEffect.TintRecursively(o, color);
			if (o.activeSelf)
			{
				o.SetActive(value: false);
				o.SetActive(value: true);
			}
		}
		CameraManager instance = ManagerBase<CameraManager>.instance;
		instance.onFocusedEntityChanged = (Action<Entity, Entity>)Delegate.Combine(instance.onFocusedEntityChanged, new Action<Entity, Entity>(OnFocusedEntityChanged));
		NetworkedManagerBase<ClientEventManager>.instance.OnHeroRevive += new Action<Hero>(HeroRevive);
		NetworkedManagerBase<ClientEventManager>.instance.OnHeroKnockedOut += new Action<Hero>(HeroKnockedOut);
		if (item is Actor a)
		{
			a.ClientActorEvent_OnDestroyed += new Action<Actor>(ClientActorEventOnDestroyed);
		}
	}

	protected virtual void Start()
	{
		if (!IsVisible())
		{
			_didAnimate = true;
		}
		else if (!_didAnimate && !item.skipStartAnimation)
		{
			_didAnimate = true;
			StartCoroutine(Routine());
		}
		else
		{
			DewEffect.Play(fxLoop);
		}
		IEnumerator Routine()
		{
			isAnimating = true;
			DewEffect.Play(fxPrepare);
			yield return new WaitForSeconds(prepareDuration);
			DewEffect.Play(fxAppear);
			OnAppearInAnimation();
			DewEffect.Play(fxLoop);
			isAnimating = false;
		}
	}

	protected virtual void OnDestroy()
	{
		if (_mat != null)
		{
			UnityEngine.Object.Destroy(_mat);
		}
		if (ManagerBase<CameraManager>.instance != null)
		{
			CameraManager instance = ManagerBase<CameraManager>.instance;
			instance.onFocusedEntityChanged = (Action<Entity, Entity>)Delegate.Remove(instance.onFocusedEntityChanged, new Action<Entity, Entity>(OnFocusedEntityChanged));
		}
		if (NetworkedManagerBase<ClientEventManager>.instance != null)
		{
			NetworkedManagerBase<ClientEventManager>.instance.OnHeroRevive -= new Action<Hero>(HeroRevive);
			NetworkedManagerBase<ClientEventManager>.instance.OnHeroKnockedOut -= new Action<Hero>(HeroKnockedOut);
		}
	}

	private void ClientActorEventOnDestroyed(Actor obj)
	{
		UpdateVisibility();
	}

	private void HeroKnockedOut(Hero obj)
	{
		UpdateVisibility();
	}

	private void HeroRevive(Hero obj)
	{
		UpdateVisibility();
	}

	private void OnFocusedEntityChanged(Entity arg1, Entity arg2)
	{
		UpdateVisibility();
	}

	protected virtual void OnAppearInAnimation()
	{
	}

	protected virtual void OnDisable()
	{
		isAnimating = false;
	}

	public bool IsVisible()
	{
		return IsVisible(item.owner != null || item.handOwner != null);
	}

	public bool IsVisible(bool isHeldBySomeone)
	{
		if (!(item is Actor { isActive: false }) && item as UnityEngine.Object != null && (ManagerBase<CameraManager>.softInstance.focusedEntity == null || !item.IsLockedFor(ManagerBase<CameraManager>.softInstance.focusedEntity.owner)))
		{
			return !isHeldBySomeone;
		}
		return false;
	}

	public void UpdateVisibilityLocal(bool isHeldBySomeone)
	{
		SetVisibility(IsVisible(isHeldBySomeone));
	}

	public void UpdateVisibility()
	{
		SetVisibility(IsVisible());
	}

	private void SetVisibility(bool value)
	{
		if (value)
		{
			DewEffect.Play(fxLoop);
			return;
		}
		DewEffect.Stop(fxPrepare);
		DewEffect.Stop(fxAppear);
		DewEffect.Stop(fxLoop);
	}
}
