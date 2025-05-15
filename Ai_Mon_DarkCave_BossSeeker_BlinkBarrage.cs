using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class Ai_Mon_DarkCave_BossSeeker_BlinkBarrage : AbilityInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__2 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_Mon_DarkCave_BossSeeker_BlinkBarrage _003C_003E4__this;

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
		public _003COnCreateSequenced_003Ed__2(int _003C_003E1__state)
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
			Ai_Mon_DarkCave_BossSeeker_BlinkBarrage ai_Mon_DarkCave_BossSeeker_BlinkBarrage = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (!ai_Mon_DarkCave_BossSeeker_BlinkBarrage.isServer)
				{
					return false;
				}
				ai_Mon_DarkCave_BossSeeker_BlinkBarrage.DestroyOnDeath(ai_Mon_DarkCave_BossSeeker_BlinkBarrage.info.caster);
				ai_Mon_DarkCave_BossSeeker_BlinkBarrage.info.caster.Control.StartDaze(ai_Mon_DarkCave_BossSeeker_BlinkBarrage.blinkInterval * (float)ai_Mon_DarkCave_BossSeeker_BlinkBarrage.blinkCount);
				_003Ci_003E5__2 = 0;
				break;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				break;
			}
			if (_003Ci_003E5__2 < ai_Mon_DarkCave_BossSeeker_BlinkBarrage.blinkCount)
			{
				ai_Mon_DarkCave_BossSeeker_BlinkBarrage.CreateStatusEffect<Se_Mon_DarkCave_BossSeeker_Blink>(ai_Mon_DarkCave_BossSeeker_BlinkBarrage.info.caster, new CastInfo(ai_Mon_DarkCave_BossSeeker_BlinkBarrage.info.caster));
				_003C_003E2__current = new SI.WaitForSeconds(ai_Mon_DarkCave_BossSeeker_BlinkBarrage.blinkInterval);
				_003C_003E1__state = 1;
				return true;
			}
			ai_Mon_DarkCave_BossSeeker_BlinkBarrage.Destroy();
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

	public int blinkCount;

	public float blinkInterval;

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__2))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void MirrorProcessed()
	{
	}
}
