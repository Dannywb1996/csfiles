using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class GameResultManager : NetworkedManagerBase<GameResultManager>
{
	public DewGameResult current;

	public SafeAction<DewGameResult> ClientEvent_OnGameResult;

	private DewGameResult _tracked;

	private uint _localPlayerNetId;

	private long _startTimestamp;

	public override void OnStartServer()
	{
		base.OnStartServer();
		GameManager.CallOnReady(StartTrackingLazy);
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += (Action<EventInfoLoadRoom>)delegate
		{
			GameManager.CallOnReady(UpdateAndSendMidGameResultToClients);
		};
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		_startTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
		if (DewPlayer.local != null)
		{
			_localPlayerNetId = DewPlayer.local.netId;
		}
		DewNetworkManager dewNetworkManager = DewNetworkManager.instance;
		dewNetworkManager.onSessionEnd = (Action)Delegate.Combine(dewNetworkManager.onSessionEnd, (Action)delegate
		{
			if (current != null)
			{
				DewSave.profile.AddNewGameResult(current);
				DewSave.SaveProfile();
			}
		});
	}

	private void StartTrackingLazy()
	{
		_tracked = new DewGameResult();
		foreach (DewPlayer h in DewPlayer.humanPlayers)
		{
			TrackPlayer(h);
		}
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnRoomLoaded += (Action<EventInfoLoadRoom>)delegate
		{
			if (_tracked != null)
			{
				_tracked.visitedLocations++;
			}
		};
		NetworkedManagerBase<ZoneManager>.instance.ClientEvent_OnZoneLoaded += (Action)delegate
		{
			if (_tracked != null)
			{
				_tracked.visitedWorlds++;
			}
		};
		Debug.Log("Started tracking game statistics");
		RpcRegisterResult(_tracked, didGameEnd: false);
	}

	private void TrackPlayer(DewPlayer p)
	{
		Hero hero = p.hero;
		if (hero == null)
		{
			return;
		}
		DewGameResult.PlayerData newData = new DewGameResult.PlayerData
		{
			playerNetId = p.netId,
			playerProfileName = p.playerName,
			heroType = hero.GetType().Name,
			level = hero.level
		};
		_tracked.players.Add(newData);
		p.ClientEvent_OnEarnStardust += (Action<int>)delegate(int amount)
		{
			newData.totalStardustIncome += amount;
		};
		hero.ClientHeroEvent_OnLevelChanged += (Action<EventInfoHeroLevelUp>)delegate(EventInfoHeroLevelUp up)
		{
			newData.level = up.newLevel;
		};
		hero.ClientHeroEvent_OnKillOrAssist += (Action<EventInfoKill>)delegate(EventInfoKill kill)
		{
			if (hero.CheckEnemyOrNeutral(kill.victim))
			{
				newData.kills++;
				if (kill.victim is Monster monster)
				{
					if (monster is BossMonster)
					{
						newData.heroicBossKills++;
					}
					if (monster.type == Monster.MonsterType.MiniBoss)
					{
						newData.miniBossKills++;
					}
					if (monster.isHunter)
					{
						newData.hunterKills++;
					}
				}
			}
		};
		p.ClientEvent_OnEarnGold += (Action<int>)delegate(int amount)
		{
			newData.totalGoldIncome += amount;
		};
		p.ClientEvent_OnDreamDustChanged += (Action<int, int>)delegate(int from, int to)
		{
			if (to > from)
			{
				newData.totalDreamDustIncome += to - from;
			}
		};
		hero.ClientHeroEvent_OnKnockedOut += (Action<EventInfoKill>)delegate
		{
			newData.deaths++;
		};
		hero.ActorEvent_OnDealDamage += (Action<EventInfoDamage>)delegate(EventInfoDamage dmg)
		{
			if (hero.GetRelation(dmg.victim) == EntityRelation.Enemy)
			{
				newData.dealtDamageToEnemies += dmg.damage.amount;
				newData.maxDealtSingleDamageToEnemy = Mathf.Max(newData.maxDealtSingleDamageToEnemy, dmg.damage.amount + dmg.damage.discardedAmount);
			}
		};
		hero.ActorEvent_OnDoHeal += (Action<EventInfoHeal>)delegate(EventInfoHeal heal)
		{
			if (heal.target == hero)
			{
				newData.healToSelf += heal.amount;
			}
			else
			{
				newData.healToOthers += heal.amount;
			}
		};
		hero.EntityEvent_OnTakeDamage += (Action<EventInfoDamage>)delegate(EventInfoDamage dmg)
		{
			newData.receivedDamage += dmg.damage.amount;
		};
		hero.ClientHeroEvent_OnKnockedOut += (Action<EventInfoKill>)delegate(EventInfoKill kill)
		{
			newData.causeOfDeathActor = ((kill.actor != null) ? kill.actor.GetType().Name : "");
			newData.causeOfDeathEntity = ((kill.actor != null && kill.actor.firstEntity != null) ? kill.actor.firstEntity.GetType().Name : "");
		};
		hero.StartCoroutine(CombatTimeRoutine());
		IEnumerator CombatTimeRoutine()
		{
			while (_tracked != null && !(this == null) && !hero.IsNullOrInactive())
			{
				if (!NetworkedManagerBase<ZoneManager>.instance.isInAnyTransition && !ManagerBase<CameraManager>.instance.isPlayingCutscene && hero.isInCombat)
				{
					newData.combatTime++;
				}
				yield return new WaitForSeconds(1f);
			}
		}
	}

	[Server]
	public void WrapUp(DewGameResult.ResultType type)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameResultManager::WrapUp(DewGameResult/ResultType)' called when server was not active");
			return;
		}
		if (_tracked == null)
		{
			Debug.LogWarning("No game result to wrap up");
			return;
		}
		UpdateGameResult();
		DewGameResult result = _tracked;
		_tracked = null;
		result.result = type;
		foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
		{
			humanPlayer.isReady = false;
		}
		RpcRegisterResult(result, didGameEnd: true);
	}

	[Server]
	public void UpdateAndSendMidGameResultToClients()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameResultManager::UpdateAndSendMidGameResultToClients()' called when server was not active");
			return;
		}
		if (_tracked == null)
		{
			Debug.LogWarning("No game result to update and send");
			return;
		}
		UpdateGameResult();
		RpcRegisterResult(_tracked, didGameEnd: false);
	}

	[Server]
	private void UpdateGameResult()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void GameResultManager::UpdateGameResult()' called when server was not active");
			return;
		}
		int i;
		for (i = _tracked.players.Count - 1; i >= 0; i--)
		{
			DewPlayer player = DewPlayer.humanPlayers.FirstOrDefault((DewPlayer p) => p.netId == _tracked.players[i].playerNetId);
			if (!(player != null) || !(player.hero != null))
			{
				_tracked.players.RemoveAt(i);
			}
		}
		foreach (DewGameResult.PlayerData data in _tracked.players)
		{
			DewPlayer player2 = data.GetPlayer();
			Hero hero = player2.hero;
			data.stars.Clear();
			data.skills.Clear();
			data.gems.Clear();
			data.maxGemCounts.Clear();
			data.stars.AddRange(player2.stars);
			foreach (KeyValuePair<int, AbilityTrigger> pair in hero.Ability.abilities)
			{
				if (pair.Value is SkillTrigger skill && pair.Key >= 0 && pair.Key <= 4)
				{
					data.skills.Add(new DewGameResult.SkillData((HeroSkillLocation)pair.Key, skill));
				}
			}
			foreach (KeyValuePair<GemLocation, Gem> pair2 in hero.Skill.gems)
			{
				data.gems.Add(new DewGameResult.GemData(pair2.Key, pair2.Value));
			}
			for (HeroSkillLocation i2 = HeroSkillLocation.Q; i2 <= HeroSkillLocation.Movement; i2++)
			{
				data.maxGemCounts.Add(hero.Skill.GetMaxGemCount(i2));
			}
			data.maxHealth = hero.maxHealth;
			data.attackDamage = hero.Status.attackDamage;
			data.abilityPower = hero.Status.abilityPower;
			data.skillHaste = hero.Status.abilityHaste;
			data.attackSpeed = 1f / hero.Ability.attackAbility.configs[0].cooldownTime * hero.Status.attackSpeedMultiplier;
			data.fireAmp = hero.Status.fireEffectAmp + 1f;
			data.critChance = hero.Status.critChance;
		}
		_tracked.elapsedGameTimeSeconds = (int)NetworkedManagerBase<GameManager>.instance.elapsedGameTime;
		_tracked.difficulty = NetworkedManagerBase<GameManager>.instance.difficulty.name;
	}

	[ClientRpc]
	private void RpcRegisterResult(DewGameResult result, bool didGameEnd)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_DewGameResult(writer, result);
		writer.WriteBool(didGameEnd);
		SendRPCInternal("System.Void GameResultManager::RpcRegisterResult(DewGameResult,System.Boolean)", 1458202908, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcRegisterResult__DewGameResult__Boolean(DewGameResult result, bool didGameEnd)
	{
		Debug.Log("Received game result from server. didGameEnd: " + didGameEnd);
		current = result;
		current.startTimestamp = _startTimestamp;
		foreach (DewGameResult.PlayerData p in result.players)
		{
			p.isLocalPlayer = _localPlayerNetId == p.playerNetId;
			for (int i = 0; i < p.skills.Count; i++)
			{
				DewGameResult.SkillData s = p.skills[i];
				s.capturedTooltipFields.Clear();
				if (NetworkClient.spawned.TryGetValue(s.netId, out var identity) && identity.TryGetComponent<SkillTrigger>(out var trg))
				{
					s.CaptureTooltipFields(trg);
				}
				p.skills[i] = s;
			}
			for (int j = 0; j < p.gems.Count; j++)
			{
				DewGameResult.GemData g = p.gems[j];
				g.capturedTooltipFields.Clear();
				if (NetworkClient.spawned.TryGetValue(g.netId, out var identity2) && identity2.TryGetComponent<Gem>(out var gem))
				{
					g.CaptureTooltipFields(gem);
				}
				p.gems[j] = g;
			}
			p.capturedStarTooltipFields.Clear();
			if (NetworkClient.spawned.TryGetValue(p.playerNetId, out var playerIdentity) && playerIdentity.TryGetComponent<DewPlayer>(out var player))
			{
				for (int k = 0; k < p.stars.Count; k++)
				{
					DewLocalization.CaptureDescriptionExpressions(DewLocalization.GetStarDescription(p.stars[k].type), p.capturedStarTooltipFields, new DewLocalization.DescriptionSettings
					{
						contextEntity = player.hero
					});
				}
			}
		}
		if (didGameEnd)
		{
			InGameUIManager.instance.SetState("Result");
			if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
			{
				ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
				{
					rawContent = DewLocalization.GetUIValue("InGame_Message_DemoFinished_Booth"),
					buttons = DewMessageSettings.ButtonType.Ok,
					defaultButton = DewMessageSettings.ButtonType.Ok,
					owner = this
				});
			}
			try
			{
				ClientEvent_OnGameResult?.Invoke(result);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			DewSave.SaveProfile();
		}
	}

	protected static void InvokeUserCode_RpcRegisterResult__DewGameResult__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRegisterResult called on server.");
		}
		else
		{
			((GameResultManager)obj).UserCode_RpcRegisterResult__DewGameResult__Boolean(GeneratedNetworkCode._Read_DewGameResult(reader), reader.ReadBool());
		}
	}

	static GameResultManager()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(GameResultManager), "System.Void GameResultManager::RpcRegisterResult(DewGameResult,System.Boolean)", InvokeUserCode_RpcRegisterResult__DewGameResult__Boolean);
	}
}
