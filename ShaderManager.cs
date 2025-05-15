using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class ShaderManager : ManagerBase<ShaderManager>, ISettingsChangedCallback
{
	private static readonly int UnscaledTime = Shader.PropertyToID("_UnscaledTime");

	private static readonly int LightRamp = Shader.PropertyToID("_LightRamp");

	private static readonly int DirectionalLightRamp = Shader.PropertyToID("_DirectionalLightRamp");

	private static readonly int TermAlpha = Shader.PropertyToID("_TermAlpha");

	private static readonly int TermBeta = Shader.PropertyToID("_TermBeta");

	private static readonly int TermGamma = Shader.PropertyToID("_TermGamma");

	private static readonly int WindEnabled = Shader.PropertyToID("_WindEnabled");

	private static readonly int GustEnabled = Shader.PropertyToID("_GustEnabled");

	private static readonly int DisableOcclusionTransparencyVar = Shader.PropertyToID("_DisableOcclusionTransparency");

	private static DewShadingProfile _shadingProfile;

	private Texture _lastLightRamp;

	private Texture _lastDirectionalLightRamp;

	private static readonly int EntityCamYInvRotationMatrix = Shader.PropertyToID("_EntityCamYInvRotationMatrix");

	internal static float _tooMuchShakeScore = 0f;

	private static DewShadingProfile GetSettings()
	{
		if (_shadingProfile == null)
		{
			_shadingProfile = Resources.Load<DewShadingProfile>("Shading");
		}
		return _shadingProfile;
	}

	private void Start()
	{
		LoadTextures();
		UpdatePerFrameVariables();
		UpdateInitVariables();
		UpdateSettingsVariables();
		UpdateRoomRotation();
		SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= SceneManagerOnsceneLoaded;
	}

	private void SceneManagerOnsceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		Dew.RefreshRenderer();
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		UpdatePerFrameVariables();
		_tooMuchShakeScore = Mathf.MoveTowards(_tooMuchShakeScore, 0f, Time.deltaTime * (_tooMuchShakeScore * 5f));
	}

	private static void LoadTextures()
	{
		Shader.SetGlobalTexture(LightRamp, GetSettings().lightRamp);
		Shader.SetGlobalTexture(DirectionalLightRamp, GetSettings().directionalLightRamp);
	}

	private static void UpdatePerFrameVariables()
	{
		Shader.SetGlobalFloat(UnscaledTime, Time.unscaledTime);
	}

	private static void UpdateInitVariables()
	{
		Shader.SetGlobalFloat(TermAlpha, GetSettings().termAlpha);
		Shader.SetGlobalFloat(TermBeta, GetSettings().termBeta);
		Shader.SetGlobalFloat(TermGamma, GetSettings().termGamma);
		if (DisableOcclusionTransparency.instance == null || !DisableOcclusionTransparency.instance.isActiveAndEnabled)
		{
			Shader.SetGlobalInt(DisableOcclusionTransparencyVar, 0);
		}
	}

	private static void UpdateSettingsVariables()
	{
		Quality3Levels shaderQuality = ((DewSave.platformSettings != null) ? DewSave.platformSettings.graphics.shaderQuality : Quality3Levels.High);
		Shader.SetGlobalInt(WindEnabled, (shaderQuality >= Quality3Levels.Medium) ? 1 : 0);
		Shader.SetGlobalInt(GustEnabled, (shaderQuality >= Quality3Levels.High) ? 1 : 0);
	}

	public static void UpdateRoomRotation()
	{
		if (ManagerBase<CameraManager>.softInstance == null)
		{
			UpdateRoomRotation(0f);
		}
		else
		{
			UpdateRoomRotation(ManagerBase<CameraManager>.softInstance.entityCamAngle);
		}
	}

	private static void UpdateRoomRotation(float yRot)
	{
		Matrix4x4 rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f - yRot, 0f));
		Shader.SetGlobalMatrix(EntityCamYInvRotationMatrix, rotationMatrix);
	}

	public void OnSettingsChanged()
	{
		UpdateSettingsVariables();
	}
}
