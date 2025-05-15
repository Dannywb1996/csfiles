using UnityEngine;
using UnityEngine.UI;

[LogicUpdatePriority(1000)]
[RequireComponent(typeof(Image))]
public class AnimatedImage : LogicBehaviour
{
	public Sprite[] frames;

	public float framesPerSecond = 10f;

	public bool useUnscaledTime = true;

	private Image _image;

	private float _startTime;

	protected override void OnEnable()
	{
		base.OnEnable();
		_image = GetComponent<Image>();
		_startTime = (useUnscaledTime ? Time.unscaledTime : Time.time);
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (frames != null && frames.Length != 0)
		{
			float elapsedTime = (useUnscaledTime ? Time.unscaledTime : Time.time) - _startTime;
			_image.sprite = frames[Mathf.RoundToInt(elapsedTime * framesPerSecond) % frames.Length];
		}
	}
}
