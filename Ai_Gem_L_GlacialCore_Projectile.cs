using System.Runtime.InteropServices;
using Mirror;

public class Ai_Gem_L_GlacialCore_Projectile : StandardProjectile
{
	[SyncVar]
	internal float _procCoefficient;

	internal float _damageAmount;

	public float Network_procCoefficient
	{
		get
		{
			return _procCoefficient;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _procCoefficient, 65536uL, null);
		}
	}

	protected override void OnCreate()
	{
		effectOnFly.transform.localScale *= _procCoefficient;
		effectOnEntity.transform.localScale *= _procCoefficient;
		base.OnCreate();
	}

	protected override void OnEntity(EntityHit hit)
	{
		base.OnEntity(hit);
		DefaultDamage(_damageAmount, _procCoefficient).SetOriginPosition(base.info.caster.position).SetElemental(ElementalType.Cold).SetAttr(DamageAttribute.ForceMergeNumber)
			.Dispatch(hit.entity, chain);
		Destroy();
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(_procCoefficient);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x10000L) != 0L)
		{
			writer.WriteFloat(_procCoefficient);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref _procCoefficient, null, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x10000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _procCoefficient, null, reader.ReadFloat());
		}
	}
}
