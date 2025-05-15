using System;

[AchUnlockOnComplete(typeof(Gem_L_ChaosApple))]
public class ACH_CHAOTIC_GOOD : DewAchievementItem
{
	private const int RequiredLegendaryChaos = 5;

	[AchPersistentVar]
	private int _currentActivationCount;

	public override int GetMaxProgress()
	{
		return 5;
	}

	public override int GetCurrentProgress()
	{
		return _currentActivationCount;
	}

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		NetworkedManagerBase<ActorManager>.instance.onActorAdd += new Action<Actor>(OnActorAdd);
	}

	public override void OnStopLocalClient()
	{
		base.OnStopLocalClient();
		if (NetworkedManagerBase<ActorManager>.instance != null)
		{
			NetworkedManagerBase<ActorManager>.instance.onActorAdd -= new Action<Actor>(OnActorAdd);
		}
	}

	private void OnActorAdd(Actor obj)
	{
		Shrine_Chaos chaos = obj as Shrine_Chaos;
		if ((object)chaos == null)
		{
			return;
		}
		chaos.ClientEvent_OnSuccessfulUse += (Action<Entity>)delegate(Entity user)
		{
			if (chaos.rarity == Rarity.Legendary && !(user == null) && !(user.owner != DewPlayer.local))
			{
				_currentActivationCount++;
				if (_currentActivationCount >= 5)
				{
					Complete();
				}
			}
		};
	}
}
