using UnityEngine;

public interface IProp
{
	bool scaleSpawnRateWithPlayers { get; }

	bool isSingleton => false;

	Vector3? customSpawnPosition => null;

	Quaternion? customSpawnRotation => null;

	bool isRegularReward => false;
}
