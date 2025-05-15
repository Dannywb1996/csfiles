using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Ai_E_StygianRush_Rush : AbilityInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__20 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_E_StygianRush_Rush _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003COnCreateSequenced_003Ed__20(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			Ai_E_StygianRush_Rush ai_E_StygianRush_Rush = _003C_003E4__this;
			if (num != 0)
			{
				return false;
			}
			_003C_003E1__state = -1;
			DewAudioSource[] adjustedAudios = ai_E_StygianRush_Rush.adjustedAudios;
			foreach (DewAudioSource obj in adjustedAudios)
			{
				obj.pitchMultiplier *= Mathf.Max(0.5f, ai_E_StygianRush_Rush.pitchMultiplier);
				obj.volumeMultiplier *= Mathf.Max(0.5f, ai_E_StygianRush_Rush.volumeMultiplier);
			}
			if (!ai_E_StygianRush_Rush.isServer)
			{
				return false;
			}
			ai_E_StygianRush_Rush._previousPos = ai_E_StygianRush_Rush.info.caster.agentPosition;
			Vector3 end = ai_E_StygianRush_Rush.info.caster.agentPosition + ai_E_StygianRush_Rush.info.forward * (ai_E_StygianRush_Rush.maxDistance + 0.5f);
			end = Dew.GetValidAgentDestination_LinearSweep(ai_E_StygianRush_Rush.info.caster.agentPosition, end);
			float duration = Vector3.Distance(ai_E_StygianRush_Rush.info.caster.agentPosition, end) / (ai_E_StygianRush_Rush.dashSpeed * Mathf.Lerp(0.4f, 0.7f, ai_E_StygianRush_Rush.chargeAmount));
			ai_E_StygianRush_Rush.info.caster.Control.StartDisplacement(new DispByDestination
			{
				affectedByMovementSpeed = false,
				canGoOverTerrain = false,
				destination = end,
				duration = duration,
				ease = DewEase.Linear,
				isCanceledByCC = false,
				isFriendly = true,
				rotateForward = true,
				onCancel = ai_E_StygianRush_Rush.OnRushComplete,
				onFinish = ai_E_StygianRush_Rush.OnRushComplete
			});
			ai_E_StygianRush_Rush.DoCollisionCheck();
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[NonSerialized]
	[SyncVar]
	internal float chargeAmount;

	[NonSerialized]
	[SyncVar]
	internal float maxDistance;

	[NonSerialized]
	[SyncVar]
	internal float damage;

	[NonSerialized]
	[SyncVar]
	internal float shakeMultiplier;

	[NonSerialized]
	[SyncVar]
	internal float pitchMultiplier;

	[NonSerialized]
	[SyncVar]
	internal float volumeMultiplier;

	public DewCollider range;

	public float dashSpeed;

	public float stunDuration;

	public GameObject fxImpact;

	public GameObject fxHitImpact;

	public GameObject fxHitSub;

	public FxCameraShake[] shakes;

	public DewAudioSource[] adjustedAudios;

	public Transform[] adjustedTransforms;

	public Knockback knockback;

	private Entity _ent;

	private Vector3 _currentPos;

	private Vector3 _previousPos;

	public float NetworkchargeAmount
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
		[param: In]
		set
		{
		}
	}

	public float NetworkmaxDistance
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
		[param: In]
		set
		{
		}
	}

	public float Networkdamage
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
		[param: In]
		set
		{
		}
	}

	public float NetworkshakeMultiplier
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
		[param: In]
		set
		{
		}
	}

	public float NetworkpitchMultiplier
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
		[param: In]
		set
		{
		}
	}

	public float NetworkvolumeMultiplier
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
		[param: In]
		set
		{
		}
	}

	protected override void OnCreate()
	{
	}

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__20))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void OnRushComplete()
	{
	}

	protected override void ActiveFrameUpdate()
	{
	}

	private void DoCollisionCheck()
	{
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
	}
}
