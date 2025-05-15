using UnityEngine;

[AchUnlockOnComplete(typeof(Gem_C_Vengeance))]
public class ACH_TASTE_OF_THEIR_OWN_MEDICINE : DewAchievementItem
{
	private float _lastDemonDamageTimeByClawing = float.NegativeInfinity;

	public override void OnStartLocalClient()
	{
		base.OnStartLocalClient();
		AchOnDealDamage(delegate(EventInfoDamage dmg)
		{
			if (dmg.victim is Mon_Forest_BossDemon && CheckActor(dmg.actor))
			{
				_lastDemonDamageTimeByClawing = Time.time;
			}
		});
		AchOnKillOrAssist(delegate(EventInfoKill kill)
		{
			if (kill.victim is Mon_Forest_BossDemon)
			{
				if (CheckActor(kill.actor))
				{
					_lastDemonDamageTimeByClawing = Time.time;
				}
				if (Time.time - _lastDemonDamageTimeByClawing < 5f)
				{
					Complete();
				}
			}
		});
	}

	private bool CheckActor(Actor a)
	{
		Actor actor = a;
		while (actor != null)
		{
			if (actor is Ai_C_IceClaw || actor is Ai_L_Hysteria_Claw || actor is Ai_Atk_AurenaSwipe || actor is Ai_Atk_AurenaSwipe_Crit || actor is Ai_Q_MoonlightPact_Fenrir_Atk || actor is Ai_Q_SylvanCall_LeafHound_Atk)
			{
				return true;
			}
			actor = actor.parentActor;
		}
		return false;
	}
}
