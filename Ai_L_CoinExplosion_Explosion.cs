using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Ai_L_CoinExplosion_Explosion : InstantDamageInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__3 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_L_CoinExplosion_Explosion _003C_003E4__this;

		private St_L_CoinExplosion _003Cst_003E5__2;

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
			Ai_L_CoinExplosion_Explosion ai_L_CoinExplosion_Explosion = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = ai_L_CoinExplosion_Explosion._003C_003En__0();
				_003C_003E1__state = 1;
				return true;
			case 1:
			{
				_003C_003E1__state = -1;
				AbilityTrigger firstTrigger = ai_L_CoinExplosion_Explosion.firstTrigger;
				_003Cst_003E5__2 = firstTrigger as St_L_CoinExplosion;
				if ((object)_003Cst_003E5__2 != null)
				{
					_003C_003E2__current = new SI.WaitForSeconds(UnityEngine.Random.Range(0.25f, 0.4f));
					_003C_003E1__state = 2;
					return true;
				}
				break;
			}
			case 2:
				_003C_003E1__state = -1;
				if (UnityEngine.Random.value < 0.5f || ((double)Mathf.Abs(_003Cst_003E5__2.damageMultiplier - 1f) < 0.1 && UnityEngine.Random.value < 0.4f))
				{
					ai_L_CoinExplosion_Explosion.FxPlayNetworked(ai_L_CoinExplosion_Explosion.fxSuccess, ai_L_CoinExplosion_Explosion.info.caster);
					St_L_CoinExplosion st_L_CoinExplosion = _003Cst_003E5__2;
					st_L_CoinExplosion.NetworkdamageMultiplier = st_L_CoinExplosion.damageMultiplier * 2f;
				}
				else
				{
					ai_L_CoinExplosion_Explosion.FxPlayNetworked(ai_L_CoinExplosion_Explosion.fxFail, ai_L_CoinExplosion_Explosion.info.caster);
					_003Cst_003E5__2.NetworkdamageMultiplier = 1f;
				}
				break;
			}
			ai_L_CoinExplosion_Explosion.Destroy();
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

	public GameObject fxSuccess;

	public GameObject fxFail;

	public float stunDuration;

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__3))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	protected override void OnBeforeDispatchDamage(ref DamageData dmg, Entity target)
	{
	}

	protected override void OnHit(Entity entity)
	{
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private IEnumerator _003C_003En__0()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void MirrorProcessed()
	{
	}
}
