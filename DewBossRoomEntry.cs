using System.Collections;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(DewHeroesTeleporter))]
[RequireComponent(typeof(DewAllHeroesPresentZone))]
public class DewBossRoomEntry : MonoBehaviour
{
	public DewCutsceneDirector cutscene;

	private DewAllHeroesPresentZone _zone;

	private DewHeroesTeleporter _teleporter;

	private void Awake()
	{
		_zone = GetComponent<DewAllHeroesPresentZone>();
		_teleporter = GetComponent<DewHeroesTeleporter>();
	}

	private void Start()
	{
		_zone.onActivate.AddListener(delegate
		{
			if (NetworkServer.active)
			{
				StartCoroutine(Routine());
			}
		});
		IEnumerator Routine()
		{
			cutscene.PlayNetworked();
			yield return new WaitForSeconds(ManagerBase<CameraManager>.instance.cutsceneFadeTime);
			for (int i = 0; i < 5; i++)
			{
				_teleporter.TeleportAllHeroes();
				yield return new WaitForSeconds(0.2f);
				if (!ManagerBase<CameraManager>.instance.isPlayingCutscene)
				{
					break;
				}
			}
		}
	}
}
