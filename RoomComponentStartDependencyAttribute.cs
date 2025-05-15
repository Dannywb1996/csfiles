using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RoomComponentStartDependencyAttribute : Attribute
{
	public Type targetRoomComponent;

	public RoomComponentStartDependencyAttribute(Type targetRoomComponent)
	{
		this.targetRoomComponent = targetRoomComponent;
	}
}
