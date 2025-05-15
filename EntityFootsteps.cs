using UnityEngine;

public class EntityFootsteps : MonoBehaviour
{
	private EntitySound _sound;

	private void Start()
	{
		_sound = GetComponentInParent<EntitySound>();
	}

	public void DoFootstep(AnimationEvent e)
	{
		if (!(e.animatorClipInfo.weight < 0.2f) && !(_sound == null))
		{
			_sound.DoFootstep();
		}
	}
}
