using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

[DewResourceLink(ResourceLinkBy.Type)]
public class PlayGameManager : GameManager
{
	public new static PlayGameManager instance => NetworkedManagerBase<GameManager>.instance as PlayGameManager;

	public override void OnStartClient()
	{
		base.OnStartClient();
		GameManager.CallOnReady(delegate
		{
			DewSave.profile.preferredHero = DewPlayer.local.hero.GetType().Name;
			if (base.isServer)
			{
				DewSave.profile.preferredLucidDreams.Clear();
				DewSave.profile.preferredLucidDreams.AddRange(NetworkedManagerBase<GameSettingsManager>.instance.lucidDreams);
			}
			if (base.isServer)
			{
				NetworkedManagerBase<QuestManager>.instance.StartQuest<Quest_ShapeOfDreams>();
			}
		});
		if (DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			GameManager.CallOnReady(delegate
			{
				Dew.GetControlPresetWindow().Show(showCancel: true);
			});
		}
		StartCoroutine(IncrementPlaytime());
		static IEnumerator IncrementPlaytime()
		{
			while (NetworkClient.active)
			{
				yield return new WaitForSeconds(60f);
				DewSave.profile.totalPlayTimeMinutes++;
			}
		}
	}

	private void SpawnHero(DewPlayer player, Hero hero, HeroLoadoutData loadout, List<string> accs)
	{
		if (player.hero != null)
		{
			Dew.Destroy(player.hero.gameObject);
		}
		Vector3 position = ((NetworkedManagerBase<ZoneManager>.instance.currentRoom == null) ? Vector3.zero : NetworkedManagerBase<ZoneManager>.instance.currentRoom.GetHeroSpawnPosition());
		Hero hero2 = Dew.SpawnHero(hero, position, Quaternion.identity, player, 1, loadout, delegate(Hero h)
		{
			h.accessories.AddRange(accs);
		});
		if (DewBuildProfile.current.bonusMemoryHaste > 0.1f)
		{
			hero2.Status.AddStatBonus(new StatBonus
			{
				abilityHasteFlat = DewBuildProfile.current.bonusMemoryHaste
			});
		}
		player.hero = hero2;
		player.controllingEntity = hero2;
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return Dew.WaitForClientsReadyRoutine();
			foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
			{
				SpawnHero(humanPlayer, DewResources.GetByShortTypeName<Hero>(humanPlayer.selectedHeroType), humanPlayer.selectedLoadout, humanPlayer.selectedAccessories.ToList());
			}
			yield return new WaitForSecondsRealtime(0.1f);
			LoadNextZone();
			if (DewBuildProfile.current.buildType != BuildType.DemoLite)
			{
				NetworkedManagerBase<ZoneManager>.instance.CallOnReadyAfterTransition(DoDejavuSpawns);
			}
		}
	}

	private void DoDejavuSpawns()
	{
		foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
		{
			try
			{
				if (humanPlayer.hero == null || string.IsNullOrEmpty(humanPlayer.selectedDejavuItem))
				{
					continue;
				}
				Vector3 goodRewardPosition = Dew.GetGoodRewardPosition(humanPlayer.hero.agentPosition);
				if (humanPlayer.selectedDejavuItem.StartsWith("St_"))
				{
					if (Dew.IsSkillIncludedInGame(humanPlayer.selectedDejavuItem))
					{
						SkillTrigger byShortTypeName = DewResources.GetByShortTypeName<SkillTrigger>(humanPlayer.selectedDejavuItem);
						Rarity rarity = byShortTypeName.rarity;
						if (rarity != Rarity.Character && rarity != Rarity.Identity)
						{
							Dew.CreateSkillTrigger(byShortTypeName, goodRewardPosition, 1, humanPlayer);
							goto IL_00cc;
						}
					}
				}
				else if (humanPlayer.selectedDejavuItem.StartsWith("Gem_") && Dew.IsGemIncludedInGame(humanPlayer.selectedDejavuItem))
				{
					Dew.CreateGem(DewResources.GetByShortTypeName<Gem>(humanPlayer.selectedDejavuItem), goodRewardPosition, 100, humanPlayer);
					goto IL_00cc;
				}
				goto end_IL_0017;
				IL_00cc:
				humanPlayer.TpcNotifyDejavuUse();
				end_IL_0017:;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public override void LoadNextZone()
	{
		base.LoadNextZone();
		if (NetworkedManagerBase<ZoneManager>.instance.currentZone != null && DewBuildProfile.current.buildType == BuildType.DemoLite && DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth))
		{
			int num = DewBuildProfile.current.content.zoneCountByTier.Sum();
			if (NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex >= num - 1)
			{
				ConcludeDemo();
				return;
			}
		}
		NetworkedManagerBase<ZoneManager>.instance.LoadNextZoneByContentSettings();
	}

	protected override DewDifficultySettings GetDifficulty()
	{
		return DewResources.GetByName<DewDifficultySettings>(NetworkedManagerBase<GameSettingsManager>.instance.difficulty);
	}

	private void MirrorProcessed()
	{
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		AttrCustomizeManager.ExecuteInGameOnce();
	}
}
