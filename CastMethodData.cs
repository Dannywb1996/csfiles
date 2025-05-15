using System;
using UnityEngine;

[Serializable]
public class CastMethodData : ICastMethodDataArrow, ICastMethodDataCone, ICastMethodDataNone, ICastMethodDataPoint, ICastMethodDataTarget
{
	public CastMethodType type;

	[SerializeField]
	public float _angle;

	[SerializeField]
	public float _length;

	[SerializeField]
	public float _width;

	[SerializeField]
	public float _range;

	[SerializeField]
	public float _radius;

	[SerializeField]
	public bool _isClamping;

	public ICastMethodDataArrow arrowData => this;

	public ICastMethodDataCone coneData => this;

	public ICastMethodDataNone noneData => this;

	public ICastMethodDataPoint pointData => this;

	public ICastMethodDataTarget targetData => this;

	float ICastMethodDataCone.angle => _angle;

	float ICastMethodDataCone.radius => _radius;

	float ICastMethodDataNone.radius => _radius;

	float ICastMethodDataPoint.radius => _radius;

	float ICastMethodDataTarget.radius => _radius;

	float ICastMethodDataArrow.length => _length;

	float ICastMethodDataArrow.width => _width;

	float ICastMethodDataPoint.range => _range;

	float ICastMethodDataTarget.range => _range;

	bool ICastMethodDataPoint.isClamping => _isClamping;

	private bool ShouldAngleFieldShow => type == CastMethodType.Cone;

	private bool ShouldLengthFieldShow => type == CastMethodType.Arrow;

	private bool ShouldWidthFieldShow => type == CastMethodType.Arrow;

	private bool ShouldRangeFieldShow
	{
		get
		{
			if (type != CastMethodType.Target)
			{
				return type == CastMethodType.Point;
			}
			return true;
		}
	}

	private bool ShouldRadiusFieldShow
	{
		get
		{
			if (type != CastMethodType.None && type != CastMethodType.Cone && type != CastMethodType.Target)
			{
				return type == CastMethodType.Point;
			}
			return true;
		}
	}

	private bool ShouldIsClampingFieldShow => type == CastMethodType.Point;

	public CastMethodData()
	{
	}

	public CastMethodData(CastMethodData data)
	{
		type = data.type;
		_angle = data._angle;
		_length = data._length;
		_width = data._width;
		_range = data._range;
		_radius = data._radius;
		_isClamping = data._isClamping;
	}

	public float GetEffectiveRange()
	{
		return type switch
		{
			CastMethodType.None => noneData.radius, 
			CastMethodType.Cone => coneData.radius, 
			CastMethodType.Arrow => arrowData.length, 
			CastMethodType.Target => targetData.range, 
			CastMethodType.Point => targetData.range, 
			_ => float.PositiveInfinity, 
		};
	}
}
