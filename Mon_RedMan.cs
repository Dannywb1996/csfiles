using UnityEngine;

public class Mon_RedMan : Monster
{
	public GameObject deathEffects;

	protected override void OnDeath(EventInfoKill info)
	{
		base.OnDeath(info);
		base.Visual.DisableRenderersLocal();
		Quaternion rotation = base.transform.rotation;
		if (info.actor.firstEntity != null)
		{
			rotation = Quaternion.LookRotation(base.transform.position - info.actor.firstEntity.transform.position).Flattened();
		}
		deathEffects.transform.rotation = rotation;
		deathEffects.transform.parent = null;
		deathEffects.SetActive(value: true);
		Rigidbody[] componentsInChildren = deathEffects.GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody obj in componentsInChildren)
		{
			obj.AddForce(Random.Range(0f, 1f) * deathEffects.transform.forward, ForceMode.VelocityChange);
			obj.transform.localScale = Vector3.one * Random.Range(0.1f, 0.2f);
			obj.transform.rotation = Random.rotation;
		}
		Object.Destroy(deathEffects, 10f);
	}

	private void MirrorProcessed()
	{
	}
}
