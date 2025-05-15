using UnityEngine;

public class FxRandomTransform : MonoBehaviour, IEffectComponent
{
	public bool randomizePosition;

	public Vector3[] localPositions;

	public bool randomizeRotation;

	public Vector3[] localRotations;

	public bool randomizeScale;

	public Vector3[] localScales;

	public bool isPlaying { get; }

	public void Play()
	{
		float value = Random.value;
		if (randomizePosition)
		{
			base.transform.localPosition = Get<Vector3>(localPositions);
		}
		if (randomizeRotation)
		{
			base.transform.localRotation = Quaternion.Euler(Get<Vector3>(localRotations));
		}
		if (randomizeScale)
		{
			base.transform.localScale = Get<Vector3>(localScales);
		}
		T Get<T>(T[] arr)
		{
			return arr[Mathf.FloorToInt(value * (float)arr.Length)];
		}
	}

	public void Stop()
	{
	}
}
