using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;

public class FxMeshTrail : FxInterpolatedEffectBase, IAttachableToEntity
{
	private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

	private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

	public Gradient trailGradient;

	public Material meshTrailMaterial;

	public int trailCount = 10;

	public float trailInterval = 0.1f;

	public Vector3 localVelocity;

	public Vector3 worldVelocity;

	private MeshRenderer[] _renderers;

	private MeshFilter[] _filters;

	private Material[] _materials;

	private Mesh[] _meshes;

	private float[] _opacities;

	private int _currentIndex;

	private float _lastTrailUpdateTime;

	private List<SkinnedMeshRenderer> _skins;

	private ListReturnHandle<SkinnedMeshRenderer> _skinsHandle;

	private Entity _target;

	private Color _originalBaseColor;

	private Color _originalEmissionColor;

	public void OnAttachToEntity(Entity target)
	{
		if (_renderers != null)
		{
			Cleanup();
		}
		_target = target;
		_skins = ((Component)target).GetComponentsInChildrenNonAlloc(out _skinsHandle);
		_renderers = ArrayPool<MeshRenderer>.Shared.Rent(trailCount * _skins.Count);
		_filters = ArrayPool<MeshFilter>.Shared.Rent(trailCount * _skins.Count);
		_meshes = ArrayPool<Mesh>.Shared.Rent(trailCount * _skins.Count);
		_materials = ArrayPool<Material>.Shared.Rent(trailCount);
		_opacities = ArrayPool<float>.Shared.Rent(trailCount);
		_originalBaseColor = meshTrailMaterial.GetColor(BaseColor);
		_originalEmissionColor = meshTrailMaterial.GetColor(EmissionColor);
		for (int i = 0; i < trailCount; i++)
		{
			_opacities[i] = 0f;
			_materials[i] = UnityEngine.Object.Instantiate(meshTrailMaterial);
			_materials[i].SetColor(BaseColor, Color.clear);
			_materials[i].SetColor(EmissionColor, Color.clear);
		}
		for (int j = 0; j < trailCount; j++)
		{
			for (int k = 0; k < _skins.Count; k++)
			{
				int index = j * _skins.Count + k;
				GameObject gobj = new GameObject();
				_renderers[index] = gobj.AddComponent<MeshRenderer>();
				_filters[index] = gobj.AddComponent<MeshFilter>();
				_renderers[index].sharedMaterial = _materials[j];
				_meshes[index] = new Mesh();
				_filters[index].sharedMesh = _meshes[index];
				_skins[k].BakeMesh(_meshes[index], useScale: false);
				Transform st = _skins[k].transform;
				gobj.transform.SetPositionAndRotation(st.position, st.rotation);
			}
		}
		_currentIndex = -1;
		UpdateMeshTrail();
	}

	private void LateUpdate()
	{
		if (_renderers == null)
		{
			if (!(_target == null) && !(base.currentValue < 0.001f))
			{
				OnAttachToEntity(_target);
			}
			return;
		}
		if (base.currentValue < 0.001f)
		{
			bool isAlive = false;
			for (int i = 0; i < _opacities.Length; i++)
			{
				if (_opacities[i] > 0.01f)
				{
					isAlive = true;
					break;
				}
			}
			if (!isAlive)
			{
				Cleanup();
				return;
			}
		}
		for (int j = 0; j < _filters.Length; j++)
		{
			if (!(_filters[j] == null))
			{
				Transform t = _filters[j].transform;
				t.position += worldVelocity * Time.deltaTime + t.rotation * localVelocity * Time.deltaTime;
			}
		}
		if (!(Time.time - _lastTrailUpdateTime <= trailInterval))
		{
			UpdateMeshTrail();
		}
	}

	protected override void ValueSetter(float value)
	{
	}

	private void UpdateMeshTrail()
	{
		try
		{
			if (_renderers != null && !(_target == null) && trailCount > 0)
			{
				_lastTrailUpdateTime = Time.time;
				_currentIndex++;
				if (_currentIndex >= trailCount)
				{
					_currentIndex = 0;
				}
				for (int i = 0; i < _skins.Count; i++)
				{
					int index = _currentIndex * _skins.Count + i;
					_skins[i].BakeMesh(_filters[index].sharedMesh, useScale: false);
					Transform st = _skins[i].transform;
					_filters[index].transform.SetPositionAndRotation(st.position, st.rotation);
				}
				for (int j = 0; j < trailCount; j++)
				{
					_opacities[j] = ((!_target.Visual.isRendererOff && j == _currentIndex) ? base.currentValue : Mathf.MoveTowards(_opacities[j], 0f, 1f / (float)trailCount));
					Color col = trailGradient.Evaluate(1f - _opacities[j]);
					_materials[j].SetColor(BaseColor, col * _originalBaseColor);
					_materials[j].SetColor(EmissionColor, col * _originalEmissionColor);
				}
			}
		}
		catch (Exception)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		Cleanup();
	}

	private void Cleanup()
	{
		if (_renderers == null)
		{
			return;
		}
		for (int i = 0; i < trailCount; i++)
		{
			UnityEngine.Object.Destroy(_materials[i]);
		}
		for (int j = 0; j < trailCount; j++)
		{
			for (int k = 0; k < _skins.Count; k++)
			{
				int index = j * _skins.Count + k;
				MeshRenderer ren = _renderers[index];
				UnityEngine.Object.Destroy(_meshes[index]);
				if (ren != null && ren.gameObject != null)
				{
					UnityEngine.Object.Destroy(ren.gameObject);
				}
			}
		}
		_skins = null;
		_skinsHandle.Return();
		ArrayPool<MeshRenderer>.Shared.Return(_renderers);
		ArrayPool<MeshFilter>.Shared.Return(_filters);
		ArrayPool<Material>.Shared.Return(_materials);
		ArrayPool<Mesh>.Shared.Return(_meshes);
		ArrayPool<float>.Shared.Return(_opacities);
		_renderers = null;
		_filters = null;
		_materials = null;
		_meshes = null;
		_opacities = null;
	}
}
