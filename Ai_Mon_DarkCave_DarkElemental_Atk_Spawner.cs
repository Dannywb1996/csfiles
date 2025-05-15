using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class Ai_Mon_DarkCave_DarkElemental_Atk_Spawner : AbilityInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__4 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_Mon_DarkCave_DarkElemental_Atk_Spawner _003C_003E4__this;

		private int _003Ci_003E5__2;

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
		public _003COnCreateSequenced_003Ed__4(int _003C_003E1__state)
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
			Ai_Mon_DarkCave_DarkElemental_Atk_Spawner ai_Mon_DarkCave_DarkElemental_Atk_Spawner = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (!ai_Mon_DarkCave_DarkElemental_Atk_Spawner.isServer)
				{
					return false;
				}
				ai_Mon_DarkCave_DarkElemental_Atk_Spawner.info.caster.Control.StartChannel(new Channel
				{
					blockedActions = Channel.BlockedAction.Everything,
					duration = ai_Mon_DarkCave_DarkElemental_Atk_Spawner.interval * (float)(ai_Mon_DarkCave_DarkElemental_Atk_Spawner.count - 1) + ai_Mon_DarkCave_DarkElemental_Atk_Spawner.postDelay,
					onCancel = ai_Mon_DarkCave_DarkElemental_Atk_Spawner.DestroyIfActive
				});
				_003Ci_003E5__2 = 0;
				break;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				break;
			}
			if (_003Ci_003E5__2 < ai_Mon_DarkCave_DarkElemental_Atk_Spawner.count)
			{
				float angle = ai_Mon_DarkCave_DarkElemental_Atk_Spawner.info.angle + ai_Mon_DarkCave_DarkElemental_Atk_Spawner.angleDeviation[_003Ci_003E5__2];
				ai_Mon_DarkCave_DarkElemental_Atk_Spawner.CreateAbilityInstance<Ai_Mon_DarkCave_DarkElemental_Atk_Projectile>(ai_Mon_DarkCave_DarkElemental_Atk_Spawner.info.caster.position, null, new CastInfo(ai_Mon_DarkCave_DarkElemental_Atk_Spawner.info.caster, angle));
				_003C_003E2__current = new SI.WaitForSeconds(ai_Mon_DarkCave_DarkElemental_Atk_Spawner.interval);
				_003C_003E1__state = 1;
				return true;
			}
			ai_Mon_DarkCave_DarkElemental_Atk_Spawner.Destroy();
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

	public int count;

	public float[] angleDeviation;

	public float interval;

	public float postDelay;

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__4))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void MirrorProcessed()
	{
	}
}
