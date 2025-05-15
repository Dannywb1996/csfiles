using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class FxCameraShake : MonoBehaviour, IEffectComponent, IEffectWithOwnerContext
{
	public const float Global_AmplitudeMultiplier = 1.25f;

	public const float Global_FrequencyMultiplier = 0.9f;

	public const float Global_TimeMultiplier = 1.25f;

	public SignalSourceAsset signalSource;

	public float delay;

	public float amplitude = 3f;

	public float frequency = 1f;

	public float attackTime;

	public float sustainTime = 0.1f;

	public float decayTime = 0.5f;

	[SerializeField]
	[HideInInspector]
	private CinemachineImpulseSource _impulse;

	private EffectOwnerContext _context;

	public bool isPlaying { get; private set; }

	private void CreateImpulseSource()
	{
		if (!(_impulse != null))
		{
			_impulse = base.gameObject.AddComponent<CinemachineImpulseSource>();
			_impulse.m_ImpulseDefinition.m_ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Legacy;
			_impulse.m_ImpulseDefinition.m_RawSignal = signalSource;
			_impulse.m_ImpulseDefinition.m_PropagationSpeed = float.PositiveInfinity;
		}
	}

	public void Play()
	{
		if (_impulse == null)
		{
			CreateImpulseSource();
		}
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			isPlaying = true;
			if (delay > 0.0001f)
			{
				yield return new WaitForSeconds(delay);
			}
			isPlaying = false;
			CinemachineImpulseDefinition def = _impulse.m_ImpulseDefinition;
			def.m_AmplitudeGain = amplitude * 1.25f;
			def.m_FrequencyGain = frequency * 0.9f;
			def.m_TimeEnvelope.m_AttackTime = attackTime * 1.25f;
			def.m_TimeEnvelope.m_SustainTime = sustainTime * 1.25f;
			def.m_TimeEnvelope.m_DecayTime = decayTime * 1.25f;
			switch (_context)
			{
			case EffectOwnerContext.OtherPlayers:
				def.m_AmplitudeGain = Mathf.Max(0f, (def.m_AmplitudeGain - 0.6f) * 0.25f);
				def.m_TimeEnvelope.m_AttackTime *= 0.5f;
				def.m_TimeEnvelope.m_SustainTime *= 0.5f;
				def.m_TimeEnvelope.m_DecayTime *= 0.5f;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case EffectOwnerContext.None:
			case EffectOwnerContext.Self:
			case EffectOwnerContext.Boss:
			case EffectOwnerContext.Others:
				break;
			}
			if (DewInput.currentMode == InputMode.Gamepad && Gamepad.current != null)
			{
				ManagerBase<InputManager>.instance.AddShakeInstance(new ShakeInstance
				{
					position = base.transform.position,
					frequency = def.m_FrequencyGain,
					amplitude = def.m_AmplitudeGain,
					attackTime = def.m_TimeEnvelope.m_AttackTime,
					sustainTime = def.m_TimeEnvelope.m_SustainTime,
					decayTime = def.m_TimeEnvelope.m_DecayTime,
					startTime = Time.time
				});
			}
			def.m_AmplitudeGain = Mathf.Max(def.m_AmplitudeGain - ShaderManager._tooMuchShakeScore, def.m_AmplitudeGain * 0.3f);
			ShaderManager._tooMuchShakeScore += def.m_AmplitudeGain * (def.m_TimeEnvelope.m_AttackTime * 0.5f + def.m_TimeEnvelope.m_DecayTime * 0.5f + def.m_TimeEnvelope.m_SustainTime);
			if (ManagerBase<CameraManager>.softInstance == null || !ManagerBase<CameraManager>.softInstance.isPlayingCutscene)
			{
				def.m_AmplitudeGain *= DewSave.profile.gameplay.screenShakeStrength;
			}
			_impulse.GenerateImpulse();
		}
	}

	public void Stop()
	{
		StopAllCoroutines();
		isPlaying = false;
	}

	public void SetOwnerContext(EffectOwnerContext context)
	{
		_context = context;
	}
}
