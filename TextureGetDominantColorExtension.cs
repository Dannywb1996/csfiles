using UnityEngine;

public static class TextureGetDominantColorExtension
{
	public static Color GetDominantColor(this Texture2D tex)
	{
		int sampleSize = 5;
		Color best = default(Color);
		float bestScore = float.NegativeInfinity;
		for (int x = 0; x < sampleSize; x++)
		{
			for (int y = 0; y < sampleSize; y++)
			{
				Color c = tex.GetPixel(Mathf.RoundToInt((float)tex.width * (float)(x + 1) / (float)(sampleSize + 2)), Mathf.RoundToInt((float)tex.width * (float)(y + 1) / (float)(sampleSize + 2)));
				if (!(c.a < 0.5f))
				{
					Color.RGBToHSV(c, out var _, out var s, out var v);
					float score = s * s * v * v * v;
					if (score > bestScore)
					{
						bestScore = score;
						best = c;
					}
				}
			}
		}
		return best.WithS(best.GetS() * 1.15f).WithA(1f);
	}

	public static Color GetBrightDominantColor(this Texture2D tex)
	{
		Color.RGBToHSV(tex.GetDominantColor(), out var h, out var s, out var v);
		s = (2f + s) / 3f;
		v = (2f + s) / 3f;
		return Color.HSVToRGB(h, s, v);
	}
}
