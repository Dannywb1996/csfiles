using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[DewResourceLink(ResourceLinkBy.Name)]
[RequireComponent(typeof(LayoutElement))]
public class Nametag : MonoBehaviour
{
	public bool generatedFromServer;

	public float iconNormalizedOffsetY;

	public float iconWidthOffset;

	public void Setup(RectTransform target, bool isIcon)
	{
		if (!Application.IsPlaying(this))
		{
			throw new InvalidOperationException();
		}
		GetComponent<LayoutElement>().ignoreLayout = true;
		RectTransform rt = (RectTransform)base.transform;
		rt.SetParent(target);
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.one;
		rt.anchoredPosition = Vector2.zero;
		rt.sizeDelta = Vector2.zero;
		float targetHeight = target.GetScreenSpaceRect().height;
		if (isIcon)
		{
			rt.sizeDelta = rt.sizeDelta.WithX(iconWidthOffset);
			base.transform.position += Vector3.up * (targetHeight * iconNormalizedOffsetY);
		}
		base.transform.DOPunchScale(Vector3.one * 0.1f, 0.1f);
	}
}
