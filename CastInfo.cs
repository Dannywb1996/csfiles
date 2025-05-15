using UnityEngine;

public struct CastInfo
{
	public Entity caster;

	public Entity target;

	public Vector3 point;

	public float angle;

	public float animSelectValue;

	public Vector3 forward => Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

	public Quaternion rotation => Quaternion.Euler(0f, angle, 0f);

	public CastInfo(Entity caster)
	{
		this = default(CastInfo);
		this.caster = caster;
		target = null;
		point = Vector3.zero;
		angle = 0f;
	}

	public CastInfo(Entity caster, Entity target)
	{
		this = default(CastInfo);
		this.caster = caster;
		this.target = target;
	}

	public CastInfo(Entity caster, float angle)
	{
		this = default(CastInfo);
		this.caster = caster;
		this.angle = angle % 360f;
	}

	public CastInfo(Entity caster, Vector3 point)
	{
		this = default(CastInfo);
		this.caster = caster;
		this.point = point;
		angle = GetAngle(point - caster.position);
	}

	public static float GetAngle(Vector3 forward)
	{
		return Vector3.SignedAngle(Vector3.forward, forward.Flattened(), Vector3.up) % 360f;
	}

	public static float GetAngle(Quaternion rotation)
	{
		return rotation.eulerAngles.y;
	}
}
