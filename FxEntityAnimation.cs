using System.Collections;
using UnityEngine;

public class FxEntityAnimation : MonoBehaviour, IEffectComponent, IAttachableToEntity
{
	public DewAnimationClip animClip;

	public float delay;

	public float speed = 1f;

	public bool stopAnimationWhenStopped;

	private Entity _target;

	public bool isPlaying => false;

	public void Play()
	{
		if (!(animClip == null) && !(_target == null))
		{
			StopAllCoroutines();
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(delay);
			_target.Animation.PlayAbilityAnimation(animClip, speed);
		}
	}

	public void Stop()
	{
		if (!(animClip == null) && !(_target == null) && stopAnimationWhenStopped)
		{
			_target.Animation.StopAbilityAnimation(animClip);
		}
	}

	public void OnAttachToEntity(Entity target)
	{
		_target = target;
	}
}
