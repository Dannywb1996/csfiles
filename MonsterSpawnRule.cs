using UnityEngine;

[CreateAssetMenu(fileName = "New Monster Spawn Rule", menuName = "Monster Spawn Rule")]
public class MonsterSpawnRule : ScriptableObject
{
	public MonsterPool pool;

	public Vector2 initialDelay = new Vector2(0.25f, 1f);

	public bool isBossSpawn;

	public int wavesMin = 2;

	public int wavesMax = 4;

	public Vector2 populationPerWave = new Vector2(2f, 3f);

	public Vector2 nextWavePopulationThreshold = new Vector2(1f, 2f);

	public float waveTimeoutMin = 12f;

	public float waveTimeoutMax = 16f;

	public float spawnMinDistance = 4f;

	public float spawnMaxDistance = 9f;

	public OverpopulationBehavior onOverPopulation;

	public float stallCancelTimeout = 90f;
}
