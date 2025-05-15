using UnityEngine;

public static class RenderTextureExtensions
{
	public static Texture2D ToTexture2D(this RenderTexture rt)
	{
		RenderTexture prev = RenderTexture.active;
		RenderTexture.active = rt;
		Texture2D texture2D = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, rt.width, rt.height), 0, 0);
		texture2D.Apply();
		RenderTexture.active = prev;
		return texture2D;
	}
}
