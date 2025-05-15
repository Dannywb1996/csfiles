using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorManager : ManagerBase<CursorManager>
{
	[NonSerialized]
	public bool disableSoftwareCursor;

	public Texture2D[] textures;

	public float scaleMultiplier;

	private Texture2D[] _resizedTextures;

	private int _currentIndex = -1;

	private void Start()
	{
		SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
		ResizeTextures();
		ApplyCursor(0);
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (NetworkedManagerBase<GameManager>.softInstance != null && DewSave.profile.controls.lockCursorInGame && InGameUIManager.softInstance != null && InGameUIManager.instance.IsState("Playing"))
		{
			Cursor.lockState = CursorLockMode.Confined;
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
		}
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.Mouse2))
		{
			ApplyCursor(1);
		}
		else
		{
			ApplyCursor(0);
		}
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= SceneManagerOnsceneLoaded;
		ClearTextures();
	}

	private void SceneManagerOnsceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		ApplyCursor(0);
	}

	public void ApplyCursor(int index)
	{
		if (disableSoftwareCursor)
		{
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			return;
		}
		if (_resizedTextures == null)
		{
			ResizeTextures();
		}
		if (_currentIndex != index)
		{
			Cursor.SetCursor(_resizedTextures[index], new Vector2(22f, 22f) * GetScaleMultiplier(), CursorMode.ForceSoftware);
			_currentIndex = index;
		}
	}

	public void ResizeTextures()
	{
		ClearTextures();
		_resizedTextures = new Texture2D[textures.Length];
		for (int i = 0; i < textures.Length; i++)
		{
			_resizedTextures[i] = CreateScaledCopy(textures[i], GetScaleMultiplier());
		}
		_currentIndex = -1;
		ApplyCursor(0);
	}

	private float GetScaleMultiplier()
	{
		return DewSave.profile.gameplay.cursorScale * scaleMultiplier * (float)Screen.height / 1440f * 1.15f;
	}

	private void ClearTextures()
	{
		if (_resizedTextures != null)
		{
			Texture2D[] resizedTextures = _resizedTextures;
			for (int i = 0; i < resizedTextures.Length; i++)
			{
				UnityEngine.Object.Destroy(resizedTextures[i]);
			}
			_resizedTextures = null;
		}
	}

	public Texture2D CreateScaledCopy(Texture2D source, float scaleFactor)
	{
		int newWidth = Mathf.RoundToInt((float)source.width * scaleFactor);
		int newHeight = Mathf.RoundToInt((float)source.height * scaleFactor);
		Color[] scaledTexels = new Color[newWidth * newHeight];
		for (int y = 0; y < newHeight; y++)
		{
			float v = (float)y / ((float)newHeight * 1f);
			int scanLineIndex = y * newWidth;
			for (int x = 0; x < newWidth; x++)
			{
				float u = (float)x / ((float)newWidth * 1f);
				Color c = source.GetPixelBilinear(u, v);
				scaledTexels[scanLineIndex + x] = c.WithH(0.55f + DewSave.profile.gameplay.cursorColor);
			}
		}
		Texture2D texture2D = new Texture2D(newWidth, newHeight, source.format, mipChain: false);
		texture2D.SetPixels(scaledTexels, 0);
		texture2D.Apply();
		return texture2D;
	}
}
