using System;
using UnityEngine;

[ExecuteAlways]
public class DewChildrenCullingGroup : MonoBehaviour
{
	public Vector3 sphereWorldOffset;

	public Vector3 sphereLocalOffset;

	public float sphereRadius;

	private CullingGroup _cg;

	private GameObject[] _children;

	private BoundingSphere[] _spheres;

	private bool[] _isVisible;

	private void OnDrawGizmosSelected()
	{
		if (_spheres == null)
		{
			return;
		}
		for (int i = 0; i < _spheres.Length; i++)
		{
			if (Application.IsPlaying(this))
			{
				Gizmos.color = (_children[i].activeSelf ? Color.green : Color.red);
			}
			else
			{
				Gizmos.color = (_isVisible[i] ? Color.green : Color.red);
			}
			Gizmos.DrawWireSphere(_spheres[i].position, _spheres[i].radius);
		}
		Gizmos.color = Color.white;
	}

	private void OnValidate()
	{
		OnDisable();
		OnEnable();
	}

	private void OnEnable()
	{
		_cg = new CullingGroup();
		int childCount = base.transform.childCount;
		_spheres = new BoundingSphere[childCount];
		_children = new GameObject[childCount];
		for (int i = 0; i < childCount; i++)
		{
			Transform t = base.transform.GetChild(i);
			_spheres[i].position = GetSpherePosition(t);
			_spheres[i].radius = sphereRadius;
			_children[i] = t.gameObject;
			if (Application.IsPlaying(this))
			{
				t.gameObject.SetActive(value: false);
			}
		}
		_cg.SetBoundingSpheres(_spheres);
		_cg.SetBoundingSphereCount(_spheres.Length);
		CullingGroup cg = _cg;
		cg.onStateChanged = (CullingGroup.StateChanged)Delegate.Combine(cg.onStateChanged, new CullingGroup.StateChanged(OnStateChanged));
		if (!Application.IsPlaying(this))
		{
			_isVisible = new bool[_spheres.Length];
		}
	}

	private void Update()
	{
		if (!(_cg.targetCamera == Dew.mainCamera))
		{
			_cg.targetCamera = Dew.mainCamera;
		}
	}

	private void OnDisable()
	{
		if (_cg != null)
		{
			_cg.Dispose();
			_cg = null;
		}
	}

	private void OnDestroy()
	{
		if (_cg != null)
		{
			_cg.Dispose();
			_cg = null;
		}
	}

	private void OnStateChanged(CullingGroupEvent e)
	{
		if (_children == null || _children.Length <= e.index || _children[e.index] == null)
		{
			return;
		}
		if (Application.IsPlaying(this))
		{
			if (e.hasBecomeVisible)
			{
				_children[e.index].SetActive(value: true);
			}
			else if (e.hasBecomeInvisible)
			{
				_children[e.index].SetActive(value: false);
			}
		}
		else if (e.hasBecomeVisible)
		{
			_isVisible[e.index] = true;
		}
		else if (e.hasBecomeInvisible)
		{
			_isVisible[e.index] = false;
		}
	}

	private Vector3 GetSpherePosition(Transform child)
	{
		return child.position + sphereWorldOffset + child.rotation * sphereLocalOffset;
	}
}
