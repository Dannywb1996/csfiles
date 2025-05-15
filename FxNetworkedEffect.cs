using Mirror;

public class FxNetworkedEffect : NetworkBehaviour
{
	public void PlayNetworked()
	{
		DewEffect.PlayNetworked(base.netIdentity, base.gameObject);
	}

	public void StopNetworked()
	{
		DewEffect.StopNetworked(base.netIdentity, base.gameObject);
	}

	public void Play()
	{
		DewEffect.Play(base.gameObject);
	}

	public void Stop()
	{
		DewEffect.Stop(base.gameObject);
	}

	private void MirrorProcessed()
	{
	}
}
