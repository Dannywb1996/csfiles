using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CharacterModelDisplay : LogicBehaviour
{
	private static readonly int DissolveAmount = Shader.PropertyToID("_DissolveStrength");

	private static readonly int CMBaseColor = Shader.PropertyToID("_CMBaseColor");

	private static readonly int CMOpacity = Shader.PropertyToID("_CMOpacity");

	private static readonly int CMDissolveColor = Shader.PropertyToID("_CMDissolveColor");

	public float appearDissolveAnimateTime = 0.2f;

	public GameObject fxCharacterChangeEffect;

	public string heroType;

	public List<string> accessories = new List<string>();

	public float opacity = 1f;

	public bool isFocused;

	private GameObject _currentModel;

	private List<Renderer> _currentRenderers = new List<Renderer>();

	private string _currentType;

	private Bounds _bounds;

	private bool _wasFocused;

	private ILobbyCharacterModelOnFocus[] _focusHandlers;

	private List<Accessory> _instantiatedAccessories = new List<Accessory>();

	private List<Material> _currentMats;

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (_currentMats != null)
		{
			SetFloat(CMOpacity, opacity);
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (heroType != _currentType)
		{
			Setup(heroType);
		}
		if (_wasFocused != isFocused)
		{
			UpdateFocused();
		}
		if (_currentRenderers != null && _currentRenderers.Count != 0)
		{
			_bounds = _currentRenderers[0].bounds;
			for (int i = 1; i < _currentRenderers.Count; i++)
			{
				_bounds.Encapsulate(_currentRenderers[i].bounds);
			}
		}
	}

	private void UpdateFocused()
	{
		_wasFocused = isFocused;
		if (_focusHandlers == null)
		{
			return;
		}
		ILobbyCharacterModelOnFocus[] focusHandlers = _focusHandlers;
		foreach (ILobbyCharacterModelOnFocus f in focusHandlers)
		{
			if (f != null && f is UnityEngine.Object o && !(o == null))
			{
				try
				{
					f.OnLobbyCharacterFocus(isFocused);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
	}

	private void Setup(string t)
	{
		if (_currentMats != null)
		{
			foreach (Material currentMat in _currentMats)
			{
				UnityEngine.Object.Destroy(currentMat);
			}
			_currentMats = null;
		}
		if (_currentModel != null)
		{
			_currentModel.GetComponent<Coroutiner>().StopAllCoroutines();
			UnityEngine.Object.Destroy(_currentModel);
			_currentRenderers.Clear();
			_currentModel = null;
			_instantiatedAccessories.Clear();
		}
		_currentType = t;
		if (string.IsNullOrEmpty(t))
		{
			return;
		}
		Hero h = DewResources.GetByShortTypeName<Hero>(t);
		Transform model = h.transform.Find("Model");
		if (model == null)
		{
			return;
		}
		_currentModel = UnityEngine.Object.Instantiate(model.gameObject, base.transform);
		_wasFocused = false;
		_focusHandlers = _currentModel.GetComponentsInChildren<ILobbyCharacterModelOnFocus>();
		ILobbyCharacterModelSetup[] componentsInChildren = _currentModel.GetComponentsInChildren<ILobbyCharacterModelSetup>();
		foreach (ILobbyCharacterModelSetup s in componentsInChildren)
		{
			try
			{
				s.OnLobbyCharacterSetup();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		_currentRenderers.Clear();
		_currentRenderers.AddRange(_currentModel.GetComponentsInChildren<SkinnedMeshRenderer>());
		_currentRenderers.AddRange(_currentModel.GetComponentsInChildren<MeshRenderer>());
		Renderer[] bodyRenderers = h.GetComponent<EntityVisual>().bodyRenderers;
		List<string> brNames = new List<string>();
		Renderer[] array = bodyRenderers;
		foreach (Renderer r in array)
		{
			brNames.Add(r.name);
		}
		for (int i2 = _currentRenderers.Count - 1; i2 >= 0; i2--)
		{
			if (!brNames.Contains(_currentRenderers[i2].name))
			{
				_currentRenderers.RemoveAt(i2);
			}
		}
		DewEffect.Play(fxCharacterChangeEffect);
		SetupAnimation(h);
		SetupVisual(h);
		UpdateAccessories();
	}

	private void SetupVisual(Hero h)
	{
		List<Renderer> list = new List<Renderer>();
		list.AddRange(_currentModel.GetComponentsInChildren<MeshRenderer>());
		list.AddRange(_currentModel.GetComponentsInChildren<SkinnedMeshRenderer>());
		_currentMats = new List<Material>();
		foreach (Renderer ren in list)
		{
			Material[] sharedMats = ren.sharedMaterials;
			for (int i = 0; i < sharedMats.Length; i++)
			{
				Material newMat = UnityEngine.Object.Instantiate(sharedMats[i]);
				_currentMats.Add(newMat);
				sharedMats[i] = newMat;
			}
			ren.sharedMaterials = sharedMats;
		}
		_currentModel.AddComponent<Coroutiner>().StartCoroutine(AnimateAppear());
		IEnumerator AnimateAppear()
		{
			SetFloat(DissolveAmount, 1f);
			for (float t = 0f; t < 1f; t += Time.deltaTime / appearDissolveAnimateTime)
			{
				SetFloat(DissolveAmount, 1f - t);
				yield return null;
			}
			SetFloat(DissolveAmount, 0f);
		}
	}

	private void SetupAnimation(Hero h)
	{
		EntityAnimation entityAnim = h.GetComponent<EntityAnimation>();
		Animator animator = _currentModel.GetComponentInChildren<Animator>();
		if (animator == null)
		{
			return;
		}
		AnimatorOverrideController overrideController = (AnimatorOverrideController)(animator.runtimeAnimatorController = new AnimatorOverrideController(animator.runtimeAnimatorController));
		string[] names = Enum.GetNames(typeof(EntityAnimation.ReplaceableAnimationType));
		AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
		int[] values = (int[])Enum.GetValues(typeof(EntityAnimation.ReplaceableAnimationType));
		AnimationClip[] baseClips = new AnimationClip[12];
		for (int i = 0; i < clips.Length; i++)
		{
			for (int j = 0; j < names.Length; j++)
			{
				if (names[j] == clips[i].name)
				{
					baseClips[values[j]] = clips[i];
					break;
				}
			}
		}
		List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
		EntityAnimation.AnimationClipWithSpeed idle = ((entityAnim.lobby.clip != null) ? entityAnim.lobby : entityAnim.idle);
		overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(baseClips[0], idle.clip));
		overrideController.ApplyOverrides(overrides);
		animator.SetFloat("speedIdle", idle.speed);
		animator.SetLayerWeight(1, 0f);
	}

	private void SetFloat(int nameID, float value)
	{
		if (_currentMats == null)
		{
			return;
		}
		foreach (Material currentMat in _currentMats)
		{
			currentMat.SetFloat(nameID, value);
		}
	}

	private float GetFloat(int nameID)
	{
		if (_currentMats == null || _currentMats.Count == 0)
		{
			return 0f;
		}
		return _currentMats[0].GetFloat(nameID);
	}

	public Vector3 GetCenterPosition()
	{
		_ = base.transform.position;
		return _bounds.center;
	}

	public Vector3 GetAbovePosition()
	{
		Vector3 origin = base.transform.position;
		Vector3 vec = _bounds.center + Vector3.up * _bounds.extents.y;
		vec.x = origin.x;
		vec.z = origin.z;
		return vec;
	}

	public void UpdateAccessories()
	{
		for (int i = _instantiatedAccessories.Count - 1; i >= 0; i--)
		{
			if (_instantiatedAccessories[i] == null)
			{
				_instantiatedAccessories.RemoveAt(i);
			}
			else if (!accessories.Contains(_instantiatedAccessories[i].name))
			{
				UnityEngine.Object.Destroy(_instantiatedAccessories[i].gameObject);
				_instantiatedAccessories.RemoveAt(i);
			}
		}
		if (_currentModel == null)
		{
			return;
		}
		foreach (string a in accessories)
		{
			if (!(_instantiatedAccessories.Find((Accessory acc) => acc.name == a) != null))
			{
				Accessory accPrefab = DewResources.GetByName<Accessory>(a);
				if (!(accPrefab == null))
				{
					Accessory newInstance = UnityEngine.Object.Instantiate(accPrefab);
					newInstance.Setup(_currentModel.transform, _currentType);
					newInstance.name = a;
					newInstance.transform.DOPunchScale(Vector3.one * 0.3f * newInstance.transform.localScale.x, 0.4f);
					_instantiatedAccessories.Add(newInstance);
				}
			}
		}
	}
}
