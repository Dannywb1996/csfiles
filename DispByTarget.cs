public class DispByTarget : Displacement
{
	public Entity target;

	public float goalDistance;

	public float speed;

	public float cancelTime = float.PositiveInfinity;
}
