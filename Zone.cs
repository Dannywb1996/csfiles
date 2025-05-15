using System.Collections.Generic;
using UnityEngine;

[DewResourceLink(ResourceLinkBy.Name)]
public class Zone : MonoBehaviour
{
	public List<string> rooms = new List<string>();

	public List<string> combatRooms = new List<string>();

	public List<string> startRooms = new List<string>();

	public List<string> bossRooms = new List<string>();

	public List<string> shopRooms = new List<string>();

	public List<string> eventRooms = new List<string>();

	public int zoneTier;

	public Color mainColor;

	[Header("World Generation")]
	public Vector2Int numOfNodes;

	public Vector2Int numOfMerchants;

	public Vector2Int numOfEvents;

	[Header("Settings")]
	public DewMusicItem defaultMusic;

	public MonsterSpawnRule defaultMonsters;

	public PropSpawnRule defaultProps;
}
