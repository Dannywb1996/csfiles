using UnityEngine;

public class FxParticleSystem : MonoBehaviour
{
	public enum ClearParticlesBehavior
	{
		Dont,
		ClearSelf,
		ClearWithChildren
	}

	public ClearParticlesBehavior clearParticlesOnStop;

	public bool dontPauseAttachedWhenTeleport;

	public bool dontPauseAttachedWhenRendererDisabled;

	public bool dontDisableAsPartOfEntityRenderer;
}
