public abstract class HeroComponent : EntityComponent
{
	public Hero hero => (Hero)base.entity;

	private void MirrorProcessed()
	{
	}
}
