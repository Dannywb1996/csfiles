using System;
using System.Collections.Generic;

public class DewConversationSettings
{
	public string startConversationKey;

	public DewPlayer player;

	public Entity[] speakers;

	public bool rotateTowardsCenter = true;

	public ConversationVisibility visibility;

	public Dictionary<string, string> variables;

	internal int _seed;

	[NonSerialized]
	public Action onStop;

	[NonSerialized]
	public Dictionary<string, Action> callFunctions;

	public int seed => _seed;

	public bool isLocalAuthority => player.isLocalPlayer;

	public bool isVisibleToLocal => visibility switch
	{
		ConversationVisibility.Everyone => true, 
		ConversationVisibility.OnlyToPlayer => player.isLocalPlayer, 
		_ => throw new ArgumentOutOfRangeException(), 
	};
}
