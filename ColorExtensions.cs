using UnityEngine;

public static class ColorExtensions
{
	public static Color WithR(this Color c, float value)
	{
		Color color = c;
		color.r = value;
		return color;
	}

	public static Color WithG(this Color c, float value)
	{
		Color color = c;
		color.g = value;
		return color;
	}

	public static Color WithB(this Color c, float value)
	{
		Color color = c;
		color.b = value;
		return color;
	}

	public static Color WithA(this Color c, float value)
	{
		Color color = c;
		color.a = value;
		return color;
	}

	public static float GetH(this Color c)
	{
		Color.RGBToHSV(c, out var h, out var _, out var _);
		return h;
	}

	public static float GetS(this Color c)
	{
		Color.RGBToHSV(c, out var _, out var s, out var _);
		return s;
	}

	public static float GetV(this Color c)
	{
		Color.RGBToHSV(c, out var _, out var _, out var v);
		return v;
	}

	public static void ToHSV(this Color c, out float h, out float s, out float v)
	{
		Color.RGBToHSV(c, out h, out s, out v);
	}

	public static Color WithH(this Color c, float value)
	{
		Color.RGBToHSV(c, out var _, out var s, out var v);
		return Color.HSVToRGB(Mathf.Repeat(value, 1f), s, v, hdr: true).WithA(c.a);
	}

	public static Color WithS(this Color c, float value)
	{
		Color.RGBToHSV(c, out var h, out var _, out var v);
		return Color.HSVToRGB(h, value, v, hdr: true).WithA(c.a);
	}

	public static Color WithV(this Color c, float value)
	{
		Color.RGBToHSV(c, out var h, out var s, out var _);
		return Color.HSVToRGB(h, s, value, hdr: true).WithA(c.a);
	}
}
