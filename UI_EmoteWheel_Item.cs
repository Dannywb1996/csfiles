using UnityEngine;

public class UI_EmoteWheel_Item : UI_StationaryEmote
{
	public GameObject highlightObject;

	public GameObject nonHighlightObject;

	public override void Setup(string emoteName)
	{
		base.Setup(emoteName);
		if (currentEmoteInstance != null)
		{
			currentEmoteInstance.transform.localScale *= 0.575f;
		}
	}

	public void SetHighlight(bool value)
	{
		if (highlightObject != null)
		{
			highlightObject.SetActive(value);
		}
		if (nonHighlightObject != null)
		{
			nonHighlightObject.SetActive(!value);
		}
	}
}
