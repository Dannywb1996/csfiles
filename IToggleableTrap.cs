public interface IToggleableTrap : IBanRoomNodesNearby, IBanCampsNearby
{
	bool isOn { get; }

	void StartTrap();

	void StopTrap();
}
