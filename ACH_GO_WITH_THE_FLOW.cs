[AchUnlockOnComplete(typeof(St_R_Parry))]
public class ACH_GO_WITH_THE_FLOW : DewAchievementItem
{
	private const int KillsCount = 70;

	private int _kills;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		if (DewPlayer.local.hero.GetType() != typeof(Hero_Mist))
		{
			return;
		}
		AchOnKillOrAssist(delegate(EventInfoKill k)
		{
			if (DewPlayer.local.hero.CheckEnemyOrNeutral(k.victim))
			{
				_kills++;
				if (_kills >= 70)
				{
					Complete();
				}
			}
		});
		AchOnTakeDamage(delegate(EventInfoDamage info)
		{
			if (!(info.damage.amount < 1f))
			{
				Entity firstEntity = info.actor.firstEntity;
				if (!(firstEntity == null) && firstEntity.GetRelation(DewPlayer.local.hero) == EntityRelation.Enemy)
				{
					_kills = 0;
				}
			}
		});
	}
}
