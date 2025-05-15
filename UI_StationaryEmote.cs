using System;
using UnityEngine;

public class UI_StationaryEmote : MonoBehaviour
{
	public RectTransform customParent;

	[NonSerialized]
	public string currentEmoteName;

	[NonSerialized]
	public Emote currentEmotePrefab;

	[NonSerialized]
	public Emote currentEmoteInstance;

	public virtual void Setup(string emoteName)
	{
		currentEmoteName = emoteName;
		if (currentEmoteInstance != null)
		{
			UnityEngine.Object.Destroy(currentEmoteInstance.gameObject);
			currentEmoteInstance = null;
		}
		if (string.IsNullOrEmpty(emoteName))
		{
			SetupEmpty();
			return;
		}
		Emote emote = DewResources.GetByName<Emote>(emoteName);
		if (emote == null)
		{
			SetupEmpty();
			return;
		}
		currentEmotePrefab = emote;
		currentEmoteInstance = UnityEngine.Object.Instantiate(emote, (customParent != null) ? customParent : base.transform);
		currentEmoteInstance.SetupStationary();
		currentEmoteInstance.transform.localPosition = Vector3.zero;
	}

	private void SetupEmpty()
	{
		if (currentEmoteInstance != null)
		{
			UnityEngine.Object.Destroy(currentEmoteInstance.gameObject);
			currentEmoteInstance = null;
		}
	}
}
