using UnityEngine;

public class PerFeatureVisibility_Booth : MonoBehaviour
{
	public bool destroyIfBooth;

	public bool destroyIfNotBooth;

	private void Start()
	{
		bool num = DewBuildProfile.current.HasFeature(BuildFeatureTag.Booth);
		if (num && destroyIfBooth)
		{
			Object.Destroy(base.gameObject);
		}
		if (!num && destroyIfNotBooth)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
