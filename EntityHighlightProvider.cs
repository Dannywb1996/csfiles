using System;
using System.Collections;
using HighlightPlus;
using UnityEngine;

public class EntityHighlightProvider : MeshHighlightProvider
{
	public const float HitEffectDuration = 0.1f;

	public bool disableHighlight;

	private const bool AlwaysDisplayEnemyOutline = false;

	private Entity _entity;

	private float _hitValue;

	protected override void Awake()
	{
		base.Awake();
		_entity = GetComponent<Entity>();
		_entity.EntityEvent_OnDeath += (Action<EventInfoKill>)delegate
		{
			base.meshHighlight.highlighted = false;
		};
		base.meshHighlight.effectGroup = TargetOptions.Scripting;
	}

	protected override void Start()
	{
		base.Start();
		UpdateStyle();
		OnCursorStatusUpdated();
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return null;
			base.meshHighlight.SetTargets(_entity.transform, _entity.Visual.solidRenderers.ToArray());
			base.meshHighlight.highlighted = true;
		}
	}

	protected override void OnCursorStatusUpdated()
	{
		switch (base.cursorStatus)
		{
		case CursorStatus.None:
			base.meshHighlight.outline = 0f;
			break;
		case CursorStatus.Hover:
			base.meshHighlight.outline = 0.4f;
			break;
		case CursorStatus.Active:
			base.meshHighlight.outline = 0.6f;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (disableHighlight)
		{
			base.meshHighlight.outline = 0f;
		}
		base.meshHighlight.outlineWidth = 1.25f;
		base.meshHighlight.UpdateMaterialProperties();
		UpdateStyle();
	}

	protected override void OnClickTimeUpdated()
	{
		base.OnClickTimeUpdated();
		UpdateStyle();
	}

	public void ShowHit()
	{
		_hitValue = 1f;
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		_hitValue = Mathf.MoveTowards(_hitValue, 0f, Time.deltaTime / 0.1f);
		base.meshHighlight.innerGlow = _hitValue;
	}

	private void UpdateStyle()
	{
		if (!(_entity == null))
		{
			switch ((DewPlayer.local == null) ? TeamRelation.Neutral : DewPlayer.local.GetTeamRelation(_entity))
			{
			case TeamRelation.Own:
				base.meshHighlight.outlineColor = ManagerBase<ObjectHighlightManager>.instance.own.color;
				break;
			case TeamRelation.Neutral:
				base.meshHighlight.outlineColor = ManagerBase<ObjectHighlightManager>.instance.neutral.color;
				break;
			case TeamRelation.Enemy:
				base.meshHighlight.outlineColor = ManagerBase<ObjectHighlightManager>.instance.enemy.color;
				break;
			case TeamRelation.Ally:
				base.meshHighlight.outlineColor = ManagerBase<ObjectHighlightManager>.instance.ally.color;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			base.meshHighlight.UpdateMaterialProperties();
		}
	}
}
