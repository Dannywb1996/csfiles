using UnityEngine;

public static class QuaternionUtil
{
	public static Quaternion AngVelToDeriv(Quaternion Current, Vector3 AngVel)
	{
		Quaternion Result = new Quaternion(AngVel.x, AngVel.y, AngVel.z, 0f) * Current;
		return new Quaternion(0.5f * Result.x, 0.5f * Result.y, 0.5f * Result.z, 0.5f * Result.w);
	}

	public static Vector3 DerivToAngVel(Quaternion Current, Quaternion Deriv)
	{
		Quaternion Result = Deriv * Quaternion.Inverse(Current);
		return new Vector3(2f * Result.x, 2f * Result.y, 2f * Result.z);
	}

	public static Quaternion IntegrateRotation(Quaternion Rotation, Vector3 AngularVelocity, float DeltaTime)
	{
		if (DeltaTime < Mathf.Epsilon)
		{
			return Rotation;
		}
		Quaternion Deriv = AngVelToDeriv(Rotation, AngularVelocity);
		Vector4 Pred = new Vector4(Rotation.x + Deriv.x * DeltaTime, Rotation.y + Deriv.y * DeltaTime, Rotation.z + Deriv.z * DeltaTime, Rotation.w + Deriv.w * DeltaTime).normalized;
		return new Quaternion(Pred.x, Pred.y, Pred.z, Pred.w);
	}

	public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time)
	{
		if (Time.deltaTime < Mathf.Epsilon)
		{
			return rot;
		}
		float Multi = ((Quaternion.Dot(rot, target) > 0f) ? 1f : (-1f));
		target.x *= Multi;
		target.y *= Multi;
		target.z *= Multi;
		target.w *= Multi;
		Vector4 Result = new Vector4(Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time), Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time), Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time), Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)).normalized;
		Vector4 derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
		deriv.x -= derivError.x;
		deriv.y -= derivError.y;
		deriv.z -= derivError.z;
		deriv.w -= derivError.w;
		return new Quaternion(Result.x, Result.y, Result.z, Result.w);
	}
}
