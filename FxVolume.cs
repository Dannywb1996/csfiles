using System;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class FxVolume : FxInterpolatedEffectBase, IEffectWithOwnerContext
{
	public float targetStrength = 1f;

	public bool useCustomOtherPlayersMultiplier;

	public float otherPlayersMultiplier = 0.5f;

	private Volume _volume;

	private EffectOwnerContext _context;

	private float _contextMultiplier;

	protected override void OnInit()
	{
		base.OnInit();
		_volume = GetComponent<Volume>();
	}

	protected override void Start()
	{
		base.Start();
		base.gameObject.layer = 11;
	}

	public override void Play()
	{
		_contextMultiplier = 1f;
		switch (_context)
		{
		case EffectOwnerContext.OtherPlayers:
			_contextMultiplier = (useCustomOtherPlayersMultiplier ? otherPlayersMultiplier : 0.5f);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case EffectOwnerContext.None:
		case EffectOwnerContext.Self:
		case EffectOwnerContext.Boss:
		case EffectOwnerContext.Others:
			break;
		}
		base.Play();
	}

	protected override void ValueSetter(float value)
	{
		UpdateVolume();
	}

	public void UpdateVolume()
	{
		_volume.weight = base.currentValue * targetStrength * _contextMultiplier;
		_volume.enabled = base.currentValue > 0f;
	}

	public void SetOwnerContext(EffectOwnerContext context)
	{
		_context = context;
	}
}
