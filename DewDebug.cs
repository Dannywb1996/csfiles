using UnityEngine;

public static class DewDebug
{
	public static void DrawRect(Rect rect, Color color, float duration)
	{
		Vector2 a = new Vector2(rect.xMin, rect.yMin);
		Vector2 b = new Vector2(rect.xMin, rect.yMax);
		Vector2 c = new Vector2(rect.xMax, rect.yMax);
		Vector2 d = new Vector2(rect.xMax, rect.yMin);
		Debug.DrawLine(a, b, color, duration);
		Debug.DrawLine(b, c, color, duration);
		Debug.DrawLine(c, d, color, duration);
		Debug.DrawLine(d, a, color, duration);
	}
}
