using UnityEngine;

public class LucidDream : Actor, IExcludeFromPool
{
	public Sprite icon;

	public Color color;

	public DewAudioClip selectSound;

	public bool excludeFromPool;

	bool IExcludeFromPool.excludeFromPool => excludeFromPool;

	public override bool isDestroyedOnRoomChange => false;

	private void MirrorProcessed()
	{
	}
}
