using System.Collections;
using UnityEngine;

public class InstantDamageInstance : DamageInstance
{
	public float damageDelay = 0.05f;

	public GameObject mainEffectAfterDelay;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!base.isServer)
		{
			yield break;
		}
		if (CheckShouldBeDestroyed())
		{
			Destroy();
			yield break;
		}
		if (damageDelay > 0f)
		{
			yield return new SI.WaitForSeconds(damageDelay);
		}
		if (CheckShouldBeDestroyed())
		{
			Destroy();
			yield break;
		}
		if (mainEffectAfterDelay != null)
		{
			FxPlayNetworked(mainEffectAfterDelay);
		}
		DoCollisionChecks();
		OnAfterDelay();
		if (destroyWhenDone)
		{
			Destroy();
		}
	}

	protected virtual void OnAfterDelay()
	{
	}

	private void MirrorProcessed()
	{
	}
}
