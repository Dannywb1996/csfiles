using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Ai_Hatred_Explosion : PunishmentInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__11 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_Hatred_Explosion _003C_003E4__this;

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
		public _003COnCreateSequenced_003Ed__11(int _003C_003E1__state)
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
			Ai_Hatred_Explosion ai_Hatred_Explosion = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (!ai_Hatred_Explosion.isServer)
				{
					return false;
				}
				_003C_003E2__current = new SI.WaitForSeconds(ai_Hatred_Explosion.initialDelay);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				ai_Hatred_Explosion.FxPlayNetworked(ai_Hatred_Explosion.telegraphEffect);
				_003C_003E2__current = new SI.WaitForSeconds(ai_Hatred_Explosion.telegraphTime);
				_003C_003E1__state = 2;
				return true;
			case 2:
			{
				_003C_003E1__state = -1;
				ai_Hatred_Explosion.FxStopNetworked(ai_Hatred_Explosion.telegraphEffect);
				ai_Hatred_Explosion.FxPlayNetworked(ai_Hatred_Explosion.explodeEffect);
				ArrayReturnHandle<Entity> handle;
				ReadOnlySpan<Entity> entities = ai_Hatred_Explosion.range.GetEntities(out handle);
				for (int i = 0; i < entities.Length; i++)
				{
					Entity entity = entities[i];
					ai_Hatred_Explosion.FxPlayNewNetworked(ai_Hatred_Explosion.hitEffect, entity);
					ai_Hatred_Explosion.DefaultDamage(ai_Hatred_Explosion.damageMaxHpRatio * entity.maxHealth).SetOriginPosition(ai_Hatred_Explosion.position).Dispatch(entity);
					ai_Hatred_Explosion.CreateBasicEffect(entity, new SlowEffect
					{
						strength = ai_Hatred_Explosion.slowAmount
					}, ai_Hatred_Explosion.slowDuration, "hatred_slow");
					if (ai_Hatred_Explosion.doKnockback)
					{
						ai_Hatred_Explosion.knockback.ApplyWithOrigin(ai_Hatred_Explosion.position, entity);
					}
				}
				handle.Return();
				ai_Hatred_Explosion.Destroy();
				return false;
			}
			}
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

	public DewCollider range;

	public GameObject telegraphEffect;

	public GameObject explodeEffect;

	public GameObject hitEffect;

	public float initialDelay;

	public float telegraphTime;

	public float damageMaxHpRatio;

	public float slowAmount;

	public float slowDuration;

	public bool doKnockback;

	public Knockback knockback;

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__11))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void MirrorProcessed()
	{
	}
}
