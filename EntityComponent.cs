[LogicUpdatePriority(-300)]
public abstract class EntityComponent : DewNetworkBehaviour
{
	public Entity entity { get; internal set; }

	private void MirrorProcessed()
	{
	}
}
