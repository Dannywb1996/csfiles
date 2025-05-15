using UnityEngine;

public class Title_LastGameCharacters : LogicBehaviour
{
	public CharacterModelDisplay[] models;

	public GameObject displayingObject;

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (models.Length > 1)
		{
			DewGameResult lastGame = null;
			if (DewSave.profile != null && DewSave.profile.lastGameResults != null && DewSave.profile.lastGameResults.Count > 0)
			{
				lastGame = DewSave.profile.lastGameResults[0];
			}
			bool isDisplaying = false;
			for (int i = 0; i < models.Length; i++)
			{
				if (lastGame == null || lastGame.players == null || i >= lastGame.players.Count)
				{
					models[i].heroType = "";
					continue;
				}
				models[i].heroType = lastGame.players[i].heroType;
				isDisplaying = true;
			}
			displayingObject.SetActive(isDisplaying);
		}
		else
		{
			models[0].heroType = DewSave.profile.preferredHero;
			displayingObject.SetActive(!string.IsNullOrEmpty(models[0].heroType));
		}
	}
}
