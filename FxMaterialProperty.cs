using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FxMaterialProperty : FxInterpolatedEffectBase
{
	public enum PropertyType
	{
		Float,
		Color
	}

	[Serializable]
	public struct PropertyEntry
	{
		public PropertyType type;

		public string property;

		public Renderer target;

		public Graphic targetUi;

		public AnimationCurve valueCurve;

		[ColorUsage(true, true)]
		public Color startColor;

		[ColorUsage(true, true)]
		public Color endColor;

		internal int _propertyId;

		internal List<Material> _instance;
	}

	public PropertyEntry[] properties;

	private Dictionary<Material, Material> _replacements;

	protected override void OnInit()
	{
		base.OnInit();
		_replacements = new Dictionary<Material, Material>();
		for (int i = 0; i < properties.Length; i++)
		{
			PropertyEntry p = properties[i];
			p._propertyId = Shader.PropertyToID(p.property);
			Material[] oldMats = ((p.target != null) ? p.target.sharedMaterials : new Material[1] { p.targetUi.material });
			Material[] newMats = new Material[oldMats.Length];
			p._instance = new List<Material>();
			for (int j = 0; j < oldMats.Length; j++)
			{
				Material m = oldMats[j];
				if (_replacements.TryGetValue(m, out var instance))
				{
					newMats[j] = instance;
					p._instance.Add(instance);
					continue;
				}
				Material newMat = UnityEngine.Object.Instantiate(m);
				_replacements.Add(m, newMat);
				newMats[j] = newMat;
				p._instance.Add(newMat);
				properties[i] = p;
			}
			if (p.target != null)
			{
				p.target.sharedMaterials = newMats;
			}
			else
			{
				p.targetUi.material = newMats[0];
			}
		}
	}

	protected override void ValueSetter(float value)
	{
		if (_replacements == null)
		{
			return;
		}
		PropertyEntry[] array = properties;
		for (int i = 0; i < array.Length; i++)
		{
			PropertyEntry p = array[i];
			if (p._instance == null || p._instance.Count < 0)
			{
				continue;
			}
			foreach (Material m in p._instance)
			{
				switch (p.type)
				{
				case PropertyType.Float:
					m.SetFloat(p._propertyId, p.valueCurve.Evaluate(value));
					break;
				case PropertyType.Color:
					m.SetColor(p._propertyId, Color.Lerp(p.startColor, p.endColor, p.valueCurve.Evaluate(value)));
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}
	}

	private void OnDestroy()
	{
		ClearMaterials();
	}

	private void ClearMaterials()
	{
		if (_replacements == null)
		{
			return;
		}
		foreach (KeyValuePair<Material, Material> m in _replacements)
		{
			if (!(m.Value == null))
			{
				UnityEngine.Object.Destroy(m.Value);
			}
		}
		_replacements = null;
	}
}
