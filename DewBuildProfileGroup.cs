using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Build Profile Group", menuName = "Build Profile Group")]
public class DewBuildProfileGroup : ScriptableObject
{
	public List<DewBuildProfile> profiles = new List<DewBuildProfile>();

	public void Validate()
	{
		if (profiles == null || profiles.Count == 0)
		{
			throw new Exception("Profiles not set");
		}
		foreach (DewBuildProfile profile in profiles)
		{
			profile.Validate();
		}
	}

	private void OnValidate()
	{
		Validate();
	}
}
