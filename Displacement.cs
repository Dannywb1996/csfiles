using System;

public abstract class Displacement
{
	public const float MinimumDisplacementDuration = 0.0001f;

	public bool isFriendly;

	public bool isCanceledByCC = true;

	public bool affectedByMovementSpeed;

	public Action onCancel;

	public Action onFinish;

	public bool rotateForward = true;

	public bool rotateSmoothly;

	public bool isAlive { get; internal set; }

	public bool hasStarted { get; internal set; }

	public float elapsedTime { get; internal set; }
}
