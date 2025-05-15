using System.Runtime.InteropServices;
using Mirror;

public class Ai_C_Pew : AbilityInstance
{
	public ChargingChannelData channel;

	public float widthMin;

	public float widthMax;

	public float fullGraceThreshold;

	public float refundMax;

	private ChargingChannel _channel;

	[SyncVar]
	private float _chargeAmount;

	public float Network_chargeAmount
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

	protected override void ActiveLogicUpdate(float dt)
	{
	}

	private void OnCast(ChargingChannel obj)
	{
	}

	private float GetWidth()
	{
		/*Error: Method body consists only of 'ret', but nothing is being returned. Decompiled assembly might be a reference assembly.*/;
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
