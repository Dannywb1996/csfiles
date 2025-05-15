using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Ai_Hatred_Monsters : PunishmentInstance
{
	[CompilerGenerated]
	private sealed class _003COnCreateSequenced_003Ed__4 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Ai_Hatred_Monsters _003C_003E4__this;

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
			Ai_Hatred_Monsters ai_Hatred_Monsters = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (!ai_Hatred_Monsters.isServer)
				{
					return false;
				}
				_003C_003E2__current = new SI.WaitForSeconds(ai_Hatred_Monsters.spawnDelay);
				_003C_003E1__state = 1;
				return true;
			case 1:
			{
				_003C_003E1__state = -1;
				int num2 = UnityEngine.Random.Range(ai_Hatred_Monsters.spawnCount.x, ai_Hatred_Monsters.spawnCount.y);
				float num3 = UnityEngine.Random.Range(0f, 360f);
				Entity entity = DewResources.GetByType<Mon_Forest_SpiderWarrior>();
				RoomMonsters roomMonsters = UnityEngine.Object.FindObjectOfType<RoomMonsters>();
				if (roomMonsters != null)
				{
					List<Entity> list = new List<Entity>();
					if (roomMonsters.defaultRule != null)
					{
						foreach (MonsterPool.SpawnRuleEntry filteredEntry in roomMonsters.defaultRule.pool.GetFilteredEntries())
						{
							if (!list.Contains(filteredEntry.monster) && filteredEntry.monster.type != Monster.MonsterType.Boss && filteredEntry.monster.type != Monster.MonsterType.Lesser)
							{
								list.Add(filteredEntry.monster);
							}
						}
					}
					if (list.Count > 0)
					{
						entity = list[UnityEngine.Random.Range(0, list.Count)];
					}
				}
				for (int i = 0; i < num2; i++)
				{
					Vector3 validAgentDestination_Closest = Dew.GetValidAgentDestination_Closest(ai_Hatred_Monsters.position, ai_Hatred_Monsters.position + Quaternion.Euler(0f, num3, 0f) * Vector3.forward * ai_Hatred_Monsters.spawnDistance);
					Entity entity2 = Dew.SpawnEntity(entity, validAgentDestination_Closest, Quaternion.LookRotation(ai_Hatred_Monsters.position - validAgentDestination_Closest), null, DewPlayer.creep, Mathf.Clamp(NetworkedManagerBase<GameManager>.instance.ambientLevel + ai_Hatred_Monsters.levelOffset, 1, int.MaxValue));
					if (ai_Hatred_Monsters.info.target != null)
					{
						entity2.AI.Aggro(ai_Hatred_Monsters.info.target);
					}
					num3 += 360f / (float)num2;
				}
				ai_Hatred_Monsters.Destroy();
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

	public Vector2Int spawnCount;

	public int levelOffset;

	public float spawnDelay;

	public float spawnDistance;

	[IteratorStateMachine(typeof(_003COnCreateSequenced_003Ed__4))]
	protected override IEnumerator OnCreateSequenced()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
	}

	private void MirrorProcessed()
	{
	}
}
