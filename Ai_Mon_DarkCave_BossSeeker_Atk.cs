using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Ai_Mon_DarkCave_BossSeeker_Atk : AbilityInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__9 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_Mon_DarkCave_BossSeeker_Atk _003C_003E4__this;

		private Ai_Mon_DarkCave_BossSeeker_Atk_Projectile _003CaiPrefab_003E5__2;

		private int _003CcurrentPerWaveCount_003E5__3;

		private int _003Ci_003E5__4;

		private int _003Cj_003E5__5;

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
		public _003COnCreateSequenced_003Ed__9(int _003C_003E1__state)
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
			Ai_Mon_DarkCave_BossSeeker_Atk ai_Mon_DarkCave_BossSeeker_Atk = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				if (!ai_Mon_DarkCave_BossSeeker_Atk.isServer)
				{
					return false;
				}
				bool flag = ai_Mon_DarkCave_BossSeeker_Atk.info.caster is Mon_DarkCave_SeekerHallucination || ai_Mon_DarkCave_BossSeeker_Atk.info.caster.Status.HasStatusEffect<Se_Mon_DarkCave_BossSeeker_Hallucination>();
				_003CaiPrefab_003E5__2 = DewResources.GetByType<Ai_Mon_DarkCave_BossSeeker_Atk_Projectile>();
				_003CcurrentPerWaveCount_003E5__3 = (flag ? ai_Mon_DarkCave_BossSeeker_Atk.perWaveSpawnCountHallucinating : ai_Mon_DarkCave_BossSeeker_Atk.perWaveSpawnCount);
				ai_Mon_DarkCave_BossSeeker_Atk.info.caster.Control.StartChannel(new Channel
				{
					blockedActions = Channel.BlockedAction.EverythingCancelable,
					duration = (float)ai_Mon_DarkCave_BossSeeker_Atk.waveCount * ai_Mon_DarkCave_BossSeeker_Atk.waveInterval + (float)(ai_Mon_DarkCave_BossSeeker_Atk.waveCount * _003CcurrentPerWaveCount_003E5__3) * ai_Mon_DarkCave_BossSeeker_Atk.perShotInterval + ai_Mon_DarkCave_BossSeeker_Atk.postDaze,
					onCancel = ai_Mon_DarkCave_BossSeeker_Atk.Destroy,
					onComplete = ai_Mon_DarkCave_BossSeeker_Atk.Destroy,
					uncancellableTime = 1f
				});
				_003Ci_003E5__4 = 0;
				goto IL_0294;
			}
			case 1:
				_003C_003E1__state = -1;
				_003Cj_003E5__5++;
				goto IL_023b;
			case 2:
				{
					_003C_003E1__state = -1;
					goto IL_0282;
				}
				IL_023b:
				if (_003Cj_003E5__5 < _003CcurrentPerWaveCount_003E5__3)
				{
					Vector3 vector = AbilityTrigger.PredictPoint_SpeedAcceleration((_003CcurrentPerWaveCount_003E5__3 == 1) ? NetworkedManagerBase<GameManager>.instance.GetPredictionStrength() : ((float)_003Cj_003E5__5 / (float)(_003CcurrentPerWaveCount_003E5__3 - 1) * 0.7f + 0.3f), ai_Mon_DarkCave_BossSeeker_Atk.info.target, ai_Mon_DarkCave_BossSeeker_Atk.info.caster.position, 0f, _003CaiPrefab_003E5__2.frontDist, _003CaiPrefab_003E5__2.initialSpeed, _003CaiPrefab_003E5__2.targetSpeed, _003CaiPrefab_003E5__2.acceleration);
					vector = Dew.GetPositionOnGround(vector + UnityEngine.Random.insideUnitSphere * ai_Mon_DarkCave_BossSeeker_Atk.randomMagnitude);
					ai_Mon_DarkCave_BossSeeker_Atk.CreateAbilityInstance<Ai_Mon_DarkCave_BossSeeker_Atk_Projectile>(ai_Mon_DarkCave_BossSeeker_Atk.position, Quaternion.identity, new CastInfo(ai_Mon_DarkCave_BossSeeker_Atk.info.caster, vector));
					_003C_003E2__current = new SI.WaitForSeconds(ai_Mon_DarkCave_BossSeeker_Atk.perShotInterval);
					_003C_003E1__state = 1;
					return true;
				}
				if (_003Ci_003E5__4 != ai_Mon_DarkCave_BossSeeker_Atk.waveCount - 1)
				{
					_003C_003E2__current = new SI.WaitForSeconds(ai_Mon_DarkCave_BossSeeker_Atk.waveInterval);
					_003C_003E1__state = 2;
					return true;
				}
				goto IL_0282;
				IL_0282:
				_003Ci_003E5__4++;
				goto IL_0294;
				IL_0294:
				if (_003Ci_003E5__4 < ai_Mon_DarkCave_BossSeeker_Atk.waveCount)
				{
					ai_Mon_DarkCave_BossSeeker_Atk.FxPlayNetworked(ai_Mon_DarkCave_BossSeeker_Atk.fxShootEffect, ai_Mon_DarkCave_BossSeeker_Atk.info.caster);
					_003Cj_003E5__5 = 0;
					goto IL_023b;
				}
				ai_Mon_DarkCave_BossSeeker_Atk.info.caster.Animation.PlayAbilityAnimation(ai_Mon_DarkCave_BossSeeker_Atk.animEnd);
				return false;
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

	public GameObject fxShootEffect;

	public int waveCount;

	public float waveInterval;

	public int perWaveSpawnCount;

	public int perWaveSpawnCountHallucinating;

	public float perShotInterval;

	public float randomMagnitude;

	public DewAnimationClip animEnd;

	public float postDaze;

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__9))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void MirrorProcessed()
	{
	}
}
