using System;
using DG.Tweening;
using UnityEngine;

public class EntityGroundMarker : MonoBehaviour
{
	public Vector3 offset;

	public GameObject ownObject;

	public GameObject enemyObject;

	public GameObject neutralObject;

	public GameObject allyObject;

	private Entity _entity;

	private void Start()
	{
		_entity = GetComponentInParent<Entity>();
		_entity.ClientActorEvent_OnDestroyed += new Action<Actor>(ClientActorEventOnDestroyed);
		_entity.Visual.ClientEvent_OnRendererEnabledChanged += new Action<bool>(ClientEventOnRendererEnabledChanged);
		_entity.Control.ClientEvent_OnOuterRadiusChanged += new Action<float, float>(ClientEventOnOuterRadiusChanged);
		_entity.Visual.ClientEvent_OnGroundMarkerHiddenChanged += new Action<bool>(ClientEventOnGroundMarkerHiddenChanged);
		if (_entity is Hero h)
		{
			h.ClientHeroEvent_OnKnockedOut += new Action<EventInfoKill>(ClientHeroEventOnKnockedOut);
			h.ClientHeroEvent_OnRevive += new Action<Hero>(ClientHeroEventOnRevive);
		}
		ControlManager instance = ManagerBase<ControlManager>.instance;
		instance.onSelectedEntityChanged = (Action<Entity, Entity>)Delegate.Combine(instance.onSelectedEntityChanged, new Action<Entity, Entity>(OnSelectedEntityChanged));
		base.transform.position = _entity.agentPosition + offset;
		base.transform.localScale = Vector3.zero;
		UpdateScale();
		UpdateVisibility();
	}

	private void OnDestroy()
	{
		if (ManagerBase<ControlManager>.instance != null)
		{
			ControlManager instance = ManagerBase<ControlManager>.instance;
			instance.onSelectedEntityChanged = (Action<Entity, Entity>)Delegate.Remove(instance.onSelectedEntityChanged, new Action<Entity, Entity>(OnSelectedEntityChanged));
		}
	}

	private void ClientEventOnGroundMarkerHiddenChanged(bool obj)
	{
		UpdateScale();
	}

	private void ClientEventOnRendererEnabledChanged(bool obj)
	{
		UpdateVisibility();
	}

	private void ClientActorEventOnDestroyed(Actor obj)
	{
		UpdateVisibility();
	}

	private void OnSelectedEntityChanged(Entity arg1, Entity arg2)
	{
		UpdateVisibility();
	}

	private void ClientEventOnOuterRadiusChanged(float arg1, float arg2)
	{
		UpdateScale();
	}

	private void ClientHeroEventOnRevive(Hero obj)
	{
		UpdateVisibility();
	}

	private void ClientHeroEventOnKnockedOut(EventInfoKill obj)
	{
		UpdateVisibility();
	}

	private void Update()
	{
		if (!_entity.IsNullOrInactive())
		{
			UpdatePosition();
		}
	}

	private void UpdateVisibility()
	{
		if (_entity.IsNullInactiveDeadOrKnockedOut() || _entity.Visual.isRendererOff || DewPlayer.local == null)
		{
			ownObject.SetActive(value: false);
			enemyObject.SetActive(value: false);
			neutralObject.SetActive(value: false);
			allyObject.SetActive(value: false);
		}
		else
		{
			TeamRelation rel = DewPlayer.local.GetTeamRelation(_entity);
			ownObject.SetActive(rel == TeamRelation.Own);
			enemyObject.SetActive(rel == TeamRelation.Enemy);
			neutralObject.SetActive(rel == TeamRelation.Neutral);
			allyObject.SetActive(rel == TeamRelation.Ally);
		}
	}

	private void UpdateScale()
	{
		float duration = _entity.Visual.spawnDuration;
		base.transform.DOKill();
		float scale = _entity.Control.outerRadius;
		if (_entity.Visual.isGroundMarkerHidden)
		{
			scale = 0f;
		}
		base.transform.DOScale(Vector3.one * scale, (duration < 0.5f) ? 0.5f : duration);
	}

	private void UpdatePosition()
	{
		Vector3 basePos = ((_entity.Visual.modelTransform != null) ? _entity.Visual.modelTransform.position : _entity.Visual.GetBasePosition());
		base.transform.position = Dew.GetPositionOnGround(basePos) + offset;
		base.transform.rotation = Quaternion.Euler(0f, (Time.time - _entity.creationTime) * 20f, 0f);
	}
}
