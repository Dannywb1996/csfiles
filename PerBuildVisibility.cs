using System;
using UnityEngine;

public class PerBuildVisibility : MonoBehaviour
{
	public bool editor;

	public bool indev;

	public bool demoLite;

	public bool demoPrivate;

	public bool release;

	private void Awake()
	{
		switch (DewBuildProfile.current.buildType)
		{
		case BuildType.Indev:
			if (!indev)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			break;
		case BuildType.DemoLite:
			if (!demoLite)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			break;
		case BuildType.DemoPrivate:
			if (!demoPrivate)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			break;
		case BuildType.Release:
			if (!release)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
