using DG.Tweening;
using UnityEngine;

public class ArtifactWorldModel : MonoBehaviour
{
	private static readonly int MainTex = Shader.PropertyToID("_MainTex");

	private static readonly int ShineColor = Shader.PropertyToID("_ShineColor");

	private static readonly int FinalColor = Shader.PropertyToID("_FinalColor");

	public GameObject fxAppear;

	public GameObject fxLoop;

	public GameObject fxPickUp;

	public GameObject[] tinted;

	public MeshRenderer icon;

	public ParticleSystemRenderer fire;

	public Transform interactPivot;

	private Material _iconMat;

	private float _nextShineTime;

	private float _startTime;

	public void Setup(Artifact a)
	{
		GameObject[] array = tinted;
		for (int i = 0; i < array.Length; i++)
		{
			DewEffect.TintRecursively(array[i], a.mainColor);
		}
		_iconMat = Object.Instantiate(icon.sharedMaterial);
		icon.sharedMaterial = _iconMat;
		_iconMat.SetTexture(MainTex, a.icon.texture);
		_iconMat.SetColor(ShineColor, a.mainColor);
		DewEffect.Play(fxAppear);
		DewEffect.Play(fxLoop);
		_nextShineTime = Time.time + 2f;
		Color c = a.mainColor;
		c.a = 1f;
		fire.material.SetColor("_RampColorTint", c);
		_startTime = Time.time;
	}

	private void Update()
	{
		icon.transform.localPosition = new Vector3(0f, 0.3f + Mathf.Sin((Time.time - _startTime) * 1.25f) * 0.2f, 0f);
		if (Time.time > _nextShineTime)
		{
			_nextShineTime = Time.time + 5f;
			DoShine();
		}
	}

	private void DoShine()
	{
		Material iconMat = _iconMat;
		iconMat.DOKill(complete: true);
		iconMat.SetFloat("_ShineLocation", 0f);
		iconMat.SetFloat("_ShineGlow", 5f);
		iconMat.DOFloat(1f, "_ShineLocation", 0.4f);
		iconMat.DOFloat(0f, "_ShineGlow", 0.75f);
	}
}
