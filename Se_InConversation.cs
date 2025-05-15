public class Se_InConversation : StatusEffect
{
	private bool _previousAIState;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			DoInvisible();
			DoUncollidable();
			DoDeathInterrupt(delegate
			{
				base.victim.Status.SetHealth(1f);
			}, -10000);
			base.victim.Control.CancelOngoingChannels();
			base.victim.Control.Stop();
			_previousAIState = base.victim.AI.disableAI;
			base.victim.AI.disableAI = true;
			base.victim.Status.isInConversation = true;
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer && base.victim != null)
		{
			base.victim.AI.disableAI = _previousAIState;
			base.victim.Status.isInConversation = false;
		}
	}

	private void MirrorProcessed()
	{
	}
}
