using System;
using UnityEngine;

public class GameCursorManager : ManagerBase<GameCursorManager>
{
	[Serializable]
	public class CursorData
	{
		public Texture2D texture;

		public Vector2 hotspot;
	}

	public CursorData[] cursors;

	public CursorData[] downCursors;

	public CursorData currentCursor;

	private void Update()
	{
	}
}
