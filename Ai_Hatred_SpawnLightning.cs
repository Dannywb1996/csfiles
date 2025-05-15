using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Ai_Hatred_SpawnLightning : PunishmentInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__3 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_Hatred_SpawnLightning _003C_003E4__this;

		private float _003Cdelay_003E5__2;

		private int _003Ccount_003E5__3;

		private int _003Ci_003E5__4;

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
		public _003COnCreateSequenced_003Ed__3(int _003C_003E1__state)
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
			Ai_Hatred_SpawnLightning ai_Hatred_SpawnLightning = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (!ai_Hatred_SpawnLightning.isServer)
				{
					return false;
				}
				_003C_003E2__current = new SI.WaitForSeconds(ai_Hatred_SpawnLightning.initialDelay);
				_003C_003E1__state = 1;
				return true;
			case 1:
			{
				_003C_003E1__state = -1;
				Ai_Hatred_SpawnLightning_Lightning byType = DewResources.GetByType<Ai_Hatred_SpawnLightning_Lightning>();
				_003Cdelay_003E5__2 = byType.initialDelay + byType.telegraphTime;
				_003Ccount_003E5__3 = UnityEngine.Random.Range(ai_Hatred_SpawnLightning.spawnCount.x, ai_Hatred_SpawnLightning.spawnCount.y);
				_003Ci_003E5__4 = 0;
				break;
			}
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__4++;
				break;
			}
			if (_003Ci_003E5__4 < _003Ccount_003E5__3)
			{
				Vector3 position = AbilityTrigger.PredictPoint_Simple(NetworkedManagerBase<GameManager>.instance.GetPredictionStrength(), ai_Hatred_SpawnLightning.info.target, _003Cdelay_003E5__2);
				ai_Hatred_SpawnLightning.CreateAbilityInstance<Ai_Hatred_SpawnLightning_Lightning>(Dew.GetPositionOnGround(position), null, ai_Hatred_SpawnLightning.info);
				_003C_003E2__current = new SI.WaitForSeconds(ai_Hatred_SpawnLightning.interval);
				_003C_003E1__state = 2;
				return true;
			}
			ai_Hatred_SpawnLightning.Destroy();
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

	public Vector2Int spawnCount;

	public float initialDelay;

	public float interval;

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__3))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void MirrorProcessed()
	{
	}
}
