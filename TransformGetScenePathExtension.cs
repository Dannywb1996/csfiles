using UnityEngine;

public static class TransformGetScenePathExtension
{
	public static string GetScenePath(this Transform t)
	{
		string path = t.name;
		while (t.parent != null)
		{
			t = t.parent;
			path = t.name + "/" + path;
		}
		return path;
	}
}
