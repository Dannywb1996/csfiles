using System.Collections.Generic;
using UnityEngine;

public class EnvParticle : MonoBehaviour
{
	public static List<EnvParticle> instances = new List<EnvParticle>();

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		instances = new List<EnvParticle>();
	}

	private void Awake()
	{
		instances.Add(this);
	}

	private void OnDestroy()
	{
		instances.Remove(this);
	}
}
