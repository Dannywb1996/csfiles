using System.Buffers;
using System.Collections;
using UnityEngine;

public class FxGibs : FxInterpolatedEffectBase
{
	public float minImpulse = 4f;

	public float maxImpulse = 8f;

	public float randomImpulse = 6f;

	public float angularSpeed = 720f;

	[HideInInspector]
	public GibInfo? info;

	private int _childCount;

	private Rigidbody[] _children;

	private Vector3[] _localScales;

	private void EnableAllChildren()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			base.transform.GetChild(i).gameObject.SetActive(value: true);
		}
	}

	private void DisableAllChildren()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			base.transform.GetChild(i).gameObject.SetActive(value: false);
		}
	}

	protected override void OnInit()
	{
		base.OnInit();
		_childCount = 0;
		_children = ArrayPool<Rigidbody>.Shared.Rent(base.transform.childCount);
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Rigidbody r = base.transform.GetChild(i).GetComponent<Rigidbody>();
			if (!(r == null))
			{
				_children[_childCount] = r;
				_childCount++;
				r.gameObject.SetActive(value: false);
				Collider col = r.GetComponentInChildren<Collider>();
				if (!(col == null))
				{
					col.gameObject.layer = 16;
				}
			}
		}
	}

	private void OnDestroy()
	{
		if (_children == null)
		{
			return;
		}
		Rigidbody[] children = _children;
		foreach (Rigidbody c in children)
		{
			if (c != null && c.gameObject != null)
			{
				Object.Destroy(c.gameObject);
			}
		}
		ArrayPool<Rigidbody>.Shared.Return(_children);
		_children = null;
	}

	public override void Play()
	{
		base.Play();
		Vector3 baseForce = (info.HasValue ? info.Value.normalizedCurrentDamage : Random.insideUnitSphere.normalized);
		float mag = baseForce.magnitude;
		baseForce = ((mag < 0.0001f) ? Vector3.zero : ((mag > 0.8f) ? (baseForce.normalized * maxImpulse) : ((!(mag < 0.25f)) ? (Mathf.Lerp(minImpulse, maxImpulse, (mag - 0.25f) / 0.55f) * baseForce.normalized) : (baseForce.normalized * minImpulse))));
		_localScales = new Vector3[_childCount];
		for (int i = 0; i < _childCount; i++)
		{
			Rigidbody r = _children[i];
			if (!(r == null))
			{
				r.gameObject.SetActive(value: true);
				r.transform.parent = null;
				_localScales[i] = r.transform.localScale;
				StartCoroutine(DelayedForce(r, baseForce));
			}
		}
		IEnumerator DelayedForce(Rigidbody rigidbody, Vector3 f)
		{
			yield return null;
			rigidbody.AddForce(f + Random.onUnitSphere * randomImpulse, ForceMode.Impulse);
			if (info.HasValue)
			{
				rigidbody.velocity += info.Value.velocity + info.Value.yVelocity * Vector3.up;
			}
			rigidbody.angularVelocity = Random.onUnitSphere * angularSpeed;
		}
	}

	protected override void ValueSetter(float value)
	{
		for (int i = 0; i < _childCount; i++)
		{
			Rigidbody r = _children[i];
			if (!(r == null))
			{
				r.gameObject.SetActive(value > 0.0001f);
				if (_localScales != null)
				{
					r.transform.localScale = _localScales[i] * value;
				}
			}
		}
	}
}
