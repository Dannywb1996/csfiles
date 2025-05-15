using System;
using System.Collections.Generic;
using UnityEngine;

[DewResourceLink(ResourceLinkBy.Name)]
public class Accessory : MonoBehaviour
{
	[Serializable]
	public class AttachOffset
	{
		public string entityName;

		public Vector3 localPosition;

		public Vector3 localRotation;

		public float localScale;
	}

	public AccType type;

	public bool generatedFromServer;

	public List<AttachOffset> customOffsets = new List<AttachOffset>();

	[Space(30f)]
	public Sprite previewImage;

	[Range(-0.25f, 0.25f)]
	public float previewPadding;

	public Transform previewCustomCenter;

	public Vector2 previewOffset;

	private Entity _target;

	private Transform _attachPoint;

	public void Setup(Transform target, string entityName)
	{
		if (!Application.IsPlaying(this))
		{
			throw new InvalidOperationException();
		}
		ListReturnHandle<EntityAccPoint> handle;
		EntityAccPoint point = ((Component)target).GetComponentsInChildrenNonAlloc(out handle).Find((EntityAccPoint p) => p.type == type);
		handle.Return();
		if (point == null)
		{
			throw new InvalidOperationException();
		}
		base.transform.parent = point.transform;
		AttachOffset customOffset = customOffsets.Find((AttachOffset p) => p.entityName == entityName);
		if (customOffset != null)
		{
			base.transform.localPosition = customOffset.localPosition;
			base.transform.localRotation = Quaternion.Euler(customOffset.localRotation);
			base.transform.localScale *= customOffset.localScale;
		}
		else
		{
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
		}
	}
}
