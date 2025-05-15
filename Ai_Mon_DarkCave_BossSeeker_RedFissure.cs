using System;
using System.Runtime.InteropServices;
using Mirror;

public class Ai_Mon_DarkCave_BossSeeker_RedFissure : InstantDamageInstance
{
	[NonSerialized]
	[SyncVar]
	public float scaleMultiplier;

	public float stunDuration;

	public float NetworkscaleMultiplier
	{
		get
		{
			/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
		}
		[param: In]
		set
		{
		}
	}

	protected override void OnCreate()
	{
	}

	protected override void OnHit(Entity entity)
	{
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
	}
}
