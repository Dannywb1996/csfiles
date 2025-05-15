using UnityEngine;

public class Ai_E_Blink_Back : AbilityInstance
{
	public GameObject teleportDestEffect;

	public GameObject prepareEffect;

	protected override void OnCreate()
	{
		base.OnCreate();
		Vector3 validAgentDestination_Closest = Dew.GetValidAgentDestination_Closest(base.info.caster.agentPosition, ((St_E_Blink)base.firstTrigger).backLocation);
		base.position = base.info.caster.agentPosition;
		Vector3 vector = validAgentDestination_Closest - base.position;
		base.rotation = Quaternion.LookRotation(vector).Flattened();
		FxPlay(prepareEffect);
		if (base.isServer)
		{
			base.info.caster.Control.Rotate(-vector, immediately: true);
			Teleport(base.info.caster, validAgentDestination_Closest);
			FxPlayNewNetworked(teleportDestEffect, validAgentDestination_Closest, Quaternion.identity);
			Destroy();
		}
	}

	private void MirrorProcessed()
	{
	}
}
