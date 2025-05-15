using UnityEngine;

public class Room_HeroSpawnPos : MonoBehaviour, IPlayerPathableArea, IBanRoomNodesNearby
{
	Vector3 IPlayerPathableArea.pathablePosition => base.transform.position;
}
