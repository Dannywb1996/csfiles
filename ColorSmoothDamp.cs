using UnityEngine;

public static class ColorSmoothDamp
{
	public static Color SmoothDamp(Color current, Color target, ref Vector4 currentVelocity, float smoothTime)
	{
		return SmoothDamp(current, target, ref currentVelocity, smoothTime, float.PositiveInfinity, Time.deltaTime);
	}

	public static Color SmoothDamp(Color current, Color target, ref Vector4 currentVelocity, float smoothTime, float maxSpeed)
	{
		return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, Time.deltaTime);
	}

	public static Color SmoothDamp(Color current, Color target, ref Vector4 currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
	{
		smoothTime = Mathf.Max(0.0001f, smoothTime);
		float omega = 2f / smoothTime;
		Vector4 currentColour = new Vector4(current.r, current.g, current.b, current.a);
		Vector4 targetColour = new Vector4(target.r, target.g, target.b, target.a);
		float x = omega * deltaTime;
		float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
		Vector4 change = currentColour - targetColour;
		Vector4 originalTo = targetColour;
		Vector4 maxChange = maxSpeed * smoothTime * Vector4.one;
		change.x = Mathf.Clamp(change.x, 0f - maxChange.x, maxChange.x);
		change.y = Mathf.Clamp(change.y, 0f - maxChange.y, maxChange.y);
		change.z = Mathf.Clamp(change.z, 0f - maxChange.z, maxChange.z);
		change.w = Mathf.Clamp(change.w, 0f - maxChange.w, maxChange.w);
		targetColour = currentColour - change;
		Vector4 temp = (currentVelocity + omega * change) * deltaTime;
		currentVelocity = (currentVelocity - omega * temp) * exp;
		Vector4 output = targetColour + (change + temp) * exp;
		if (originalTo.x - currentColour.x > 0f == output.x > originalTo.x)
		{
			output.x = originalTo.x;
			currentVelocity.x = (output.x - originalTo.x) / deltaTime;
		}
		if (originalTo.y - currentColour.y > 0f == output.y > originalTo.y)
		{
			output.y = originalTo.y;
			currentVelocity.y = (output.y - originalTo.y) / deltaTime;
		}
		if (originalTo.z - currentColour.z > 0f == output.z > originalTo.z)
		{
			output.z = originalTo.z;
			currentVelocity.z = (output.z - originalTo.z) / deltaTime;
		}
		if (originalTo.w - currentColour.w > 0f == output.w > originalTo.w)
		{
			output.w = originalTo.w;
			currentVelocity.w = (output.w - originalTo.w) / deltaTime;
		}
		return new Color(output.x, output.y, output.z, output.w);
	}
}
