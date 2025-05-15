using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class BossMonster : Monster
{
	public bool isHiddenBoss;

	[NonSerialized]
	[SyncVar]
	public bool skipBossSoulFlow;

	public bool NetworkskipBossSoulFlow
	{
		get
		{
			return skipBossSoulFlow;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref skipBossSoulFlow, 128uL, null);
		}
	}

	protected virtual void OnBossSoulBeforeSpawn(Shrine_BossSoul soul)
	{
	}

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		if (!base.isServer)
		{
			return;
		}
		NetworkedManagerBase<GameManager>.instance.isGameTimePaused = true;
		foreach (Entity ent in new List<Entity>(NetworkedManagerBase<ActorManager>.instance.allEntities))
		{
			if (ent.isActive && !ent.Status.isDead && ent is Monster { type: not MonsterType.Boss })
			{
				ent.Kill();
			}
		}
		Vector3 pos = base.agentPosition;
		if (!skipBossSoulFlow)
		{
			NetworkedManagerBase<ActorManager>.instance.StartCoroutine(RewardRoutine());
		}
		NetworkedManagerBase<ActorManager>.instance.StartCoroutine(ReviveRoutine());
		IEnumerator ReviveRoutine()
		{
			yield return new WaitForSeconds(1f);
			if (!NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition)
			{
				bool isAnyoneAlive = false;
				foreach (DewPlayer h in DewPlayer.humanPlayers)
				{
					if (h.hero != null && !h.hero.isKnockedOut)
					{
						isAnyoneAlive = true;
						break;
					}
				}
				if (isAnyoneAlive)
				{
					foreach (DewPlayer h2 in DewPlayer.humanPlayers)
					{
						if (h2.hero != null && h2.hero.isKnockedOut)
						{
							Vector3 revivePos = pos + UnityEngine.Random.insideUnitSphere.Flattened() * 5f;
							revivePos = Dew.GetPositionOnGround(revivePos);
							revivePos = Dew.GetValidAgentDestination_LinearSweep(pos, revivePos);
							h2.hero.Control.Teleport(revivePos);
						}
					}
					yield return new WaitForSeconds(0.3f);
					List<Entity> heroesToRevive = new List<Entity>();
					foreach (DewPlayer h3 in DewPlayer.humanPlayers)
					{
						if (h3.hero != null && h3.hero.isKnockedOut)
						{
							heroesToRevive.Add(h3.hero);
						}
					}
					foreach (Entity h4 in heroesToRevive)
					{
						if (NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition)
						{
							yield break;
						}
						h4.CreateAbilityInstance(pos, null, new CastInfo(h4, h4), delegate(Ai_ReviveHero a)
						{
							a.reviveHealthMultiplier = 0.4f;
						});
						yield return new WaitForSeconds(0.25f);
					}
				}
			}
		}
		IEnumerator RewardRoutine()
		{
			Rift[] array = UnityEngine.Object.FindObjectsOfType<Rift>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].isLocked = true;
			}
			yield return new WaitForSeconds(2f);
			if (!NetworkedManagerBase<ZoneManager>.instance.isInRoomTransition)
			{
				Dew.CreateActor(Dew.GetGoodRewardPosition(pos, 0f), Quaternion.identity, null, delegate(Shrine_BossSoul soul)
				{
					soul.Network_bossTypeName = GetType().Name;
					OnBossSoulBeforeSpawn(soul);
				});
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(skipBossSoulFlow);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x80L) != 0L)
		{
			writer.WriteBool(skipBossSoulFlow);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref skipBossSoulFlow, null, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x80L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref skipBossSoulFlow, null, reader.ReadBool());
		}
	}
}
