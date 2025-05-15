using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

[DewResourceLink(ResourceLinkBy.Type)]
public abstract class Treasure : Actor, IExcludeFromPool
{
	public Sprite icon;

	public int maxUse = 1;

	public int basePrice = 100;

	public bool excludeFromPool;

	[CompilerGenerated]
	[SyncVar]
	private int _003Cprice_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private PropEnt_Merchant_Base _003Cmerchant_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private DewPlayer _003Cplayer_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private Hero _003Chero_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private string _003CcustomData_003Ek__BackingField;

	protected NetworkBehaviourSyncVar ____003Cmerchant_003Ek__BackingFieldNetId;

	protected NetworkBehaviourSyncVar ____003Cplayer_003Ek__BackingFieldNetId;

	protected NetworkBehaviourSyncVar ____003Chero_003Ek__BackingFieldNetId;

	public int price
	{
		[CompilerGenerated]
		get
		{
			return _003Cprice_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cprice_003Ek__BackingField = value;
		}
	}

	public PropEnt_Merchant_Base merchant
	{
		[CompilerGenerated]
		get
		{
			return Network_003Cmerchant_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cmerchant_003Ek__BackingField = value;
		}
	}

	public DewPlayer player
	{
		[CompilerGenerated]
		get
		{
			return Network_003Cplayer_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cplayer_003Ek__BackingField = value;
		}
	}

	public Hero hero
	{
		[CompilerGenerated]
		get
		{
			return Network_003Chero_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Chero_003Ek__BackingField = value;
		}
	}

	public string customData
	{
		[CompilerGenerated]
		get
		{
			return _003CcustomData_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcustomData_003Ek__BackingField = value;
		}
	}

	bool IExcludeFromPool.excludeFromPool => excludeFromPool;

	public int Network_003Cprice_003Ek__BackingField
	{
		get
		{
			return price;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref price, 4uL, null);
		}
	}

	public PropEnt_Merchant_Base Network_003Cmerchant_003Ek__BackingField
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____003Cmerchant_003Ek__BackingFieldNetId, ref merchant);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref merchant, 8uL, null, ref ____003Cmerchant_003Ek__BackingFieldNetId);
		}
	}

	public DewPlayer Network_003Cplayer_003Ek__BackingField
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____003Cplayer_003Ek__BackingFieldNetId, ref player);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref player, 16uL, null, ref ____003Cplayer_003Ek__BackingFieldNetId);
		}
	}

	public Hero Network_003Chero_003Ek__BackingField
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____003Chero_003Ek__BackingFieldNetId, ref hero);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref hero, 32uL, null, ref ____003Chero_003Ek__BackingFieldNetId);
		}
	}

	public string Network_003CcustomData_003Ek__BackingField
	{
		get
		{
			return customData;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref customData, 64uL, null);
		}
	}

	public virtual void OnAddMerchandise(out Cost mercPrice, out string customData)
	{
		mercPrice = Cost.Gold(Mathf.RoundToInt((float)basePrice * (1f + (float)NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex * 0.25f + (float)NetworkedManagerBase<ZoneManager>.instance.loopIndex * 0.5f)));
		customData = null;
	}

	public virtual bool ShouldBeIncludedInPool()
	{
		return true;
	}

	public virtual bool CanBePurchased()
	{
		return true;
	}

	public virtual string GetCustomName()
	{
		return null;
	}

	public virtual string GetCustomDescription()
	{
		return null;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(price);
			writer.WriteNetworkBehaviour(Network_003Cmerchant_003Ek__BackingField);
			writer.WriteNetworkBehaviour(Network_003Cplayer_003Ek__BackingField);
			writer.WriteNetworkBehaviour(Network_003Chero_003Ek__BackingField);
			writer.WriteString(customData);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteInt(price);
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_003Cmerchant_003Ek__BackingField);
		}
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_003Cplayer_003Ek__BackingField);
		}
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_003Chero_003Ek__BackingField);
		}
		if ((base.syncVarDirtyBits & 0x40L) != 0L)
		{
			writer.WriteString(customData);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref price, null, reader.ReadInt());
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref merchant, null, reader, ref ____003Cmerchant_003Ek__BackingFieldNetId);
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref player, null, reader, ref ____003Cplayer_003Ek__BackingFieldNetId);
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref hero, null, reader, ref ____003Chero_003Ek__BackingFieldNetId);
			GeneratedSyncVarDeserialize(ref customData, null, reader.ReadString());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 4L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref price, null, reader.ReadInt());
		}
		if ((num & 8L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref merchant, null, reader, ref ____003Cmerchant_003Ek__BackingFieldNetId);
		}
		if ((num & 0x10L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref player, null, reader, ref ____003Cplayer_003Ek__BackingFieldNetId);
		}
		if ((num & 0x20L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref hero, null, reader, ref ____003Chero_003Ek__BackingFieldNetId);
		}
		if ((num & 0x40L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref customData, null, reader.ReadString());
		}
	}
}
