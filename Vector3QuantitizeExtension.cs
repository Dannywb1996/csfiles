using UnityEngine;

public static class Vector3QuantitizeExtension
{
	public static Vector3 Quantitized(this Vector3 v)
	{
		return new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
	}

	public static void Quantitize(this ref Vector3 v)
	{
		v = new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
	}

	public static Vector2 Quantitized(this Vector2 v)
	{
		return new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
	}

	public static void Quantitize(this ref Vector2 v)
	{
		v = new Vector3(Mathf.Round(v.x), Mathf.Round(v.y));
	}

	public static Vector3 Rounded(this Vector3 v, int dec)
	{
		float pow = Mathf.Pow(10f, dec);
		return (v * pow).Quantitized() / pow;
	}

	public static void Round(this ref Vector3 v, int dec)
	{
		float pow = Mathf.Pow(10f, dec);
		Vector3 a = v * pow;
		v = a.Quantitized() / pow;
	}

	public static Vector2 Rounded(this Vector2 v, int dec)
	{
		float pow = Mathf.Pow(10f, dec);
		return (v * pow).Quantitized() / pow;
	}

	public static void Round(this ref Vector2 v, int dec)
	{
		float pow = Mathf.Pow(10f, dec);
		Vector2 a = v * pow;
		v = a.Quantitized() / pow;
	}
}
