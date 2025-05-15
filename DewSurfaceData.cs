using UnityEngine;

public class DewSurfaceData : MonoBehaviour
{
	private static DewSurfaceData _defaultSurface;

	public GameObject fxHeroFootstep;

	public GameObject fxPeriodicEffect;

	public Vector2 periodicEffectInterval = new Vector2(0.2f, 0.25f);

	public bool excludeLesserMonster = true;

	public bool onlyWhenWalking = true;

	public static DewSurfaceData defaultSurface
	{
		get
		{
			if (_defaultSurface == null)
			{
				_defaultSurface = Resources.Load<GameObject>("Footsteps/Surface_Default").GetComponent<DewSurfaceData>();
			}
			return _defaultSurface;
		}
	}
}
