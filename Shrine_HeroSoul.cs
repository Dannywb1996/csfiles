using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Shrine_HeroSoul : Shrine, IShrineCustomName
{
	[CompilerGenerated]
	[SyncVar]
	private Hero _003CtargetHero_003Ek__BackingField;

	public GameObject[] tintedObjects;

	public Color goldTint;

	public Color blueTint;

	public bool validateOnLogicUpdate;

	public float reviveDelay = 1.5f;

	protected NetworkBehaviourSyncVar ____003CtargetHero_003Ek__BackingFieldNetId;

	public Hero targetHero
	{
		[CompilerGenerated]
		get
		{
			return Network_003CtargetHero_003Ek__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CtargetHero_003Ek__BackingField = value;
		}
	}

	public Hero Network_003CtargetHero_003Ek__BackingField
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____003CtargetHero_003Ek__BackingFieldNetId, ref targetHero);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref targetHero, 256uL, null, ref ____003CtargetHero_003Ek__BackingFieldNetId);
		}
	}

	protected override void OnCreate()
	{
		if (Network_003CtargetHero_003Ek__BackingField != null)
		{
			Color color = (Network_003CtargetHero_003Ek__BackingField.Visual.hasGoldDissolve ? goldTint : blueTint);
			GameObject[] array = tintedObjects;
			for (int i = 0; i < array.Length; i++)
			{
				DewEffect.TintRecursively(array[i], color);
			}
		}
		base.OnCreate();
		FxStop(availableEffect);
		FxPlay(availableEffect);
		if (base.isServer)
		{
			if (Network_003CtargetHero_003Ek__BackingField.IsNullOrInactive() || !Network_003CtargetHero_003Ek__BackingField.isKnockedOut)
			{
				Destroy();
				return;
			}
			Network_003CtargetHero_003Ek__BackingField.Control.Teleport(Dew.GetValidAgentPosition(base.position));
			Network_003CtargetHero_003Ek__BackingField.Visual.DisableRenderers();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (base.isServer && !Network_003CtargetHero_003Ek__BackingField.IsNullOrInactive())
		{
			Network_003CtargetHero_003Ek__BackingField.Visual.EnableRenderers();
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		if (base.isServer && validateOnLogicUpdate && (Network_003CtargetHero_003Ek__BackingField.IsNullOrInactive() || !Network_003CtargetHero_003Ek__BackingField.isKnockedOut))
		{
			Destroy();
		}
	}

	protected override bool OnUse(Entity entity)
	{
		if (Network_003CtargetHero_003Ek__BackingField.IsNullOrInactive() || !Network_003CtargetHero_003Ek__BackingField.isKnockedOut)
		{
			return false;
		}
		Network_003CtargetHero_003Ek__BackingField.StartCoroutine(Routine());
		return true;
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(reviveDelay);
			if (!Network_003CtargetHero_003Ek__BackingField.IsNullOrInactive() && Network_003CtargetHero_003Ek__BackingField.isKnockedOut)
			{
				Network_003CtargetHero_003Ek__BackingField.CreateAbilityInstance<Ai_ReviveHero>(Network_003CtargetHero_003Ek__BackingField.position, null, new CastInfo(Network_003CtargetHero_003Ek__BackingField, Network_003CtargetHero_003Ek__BackingField));
				Destroy();
			}
		}
	}

	public string GetRawName()
	{
		return string.Format(DewLocalization.GetUIValue("Shrine_HeroSoul_Name"), ChatManager.GetDescribedPlayerName(Network_003CtargetHero_003Ek__BackingField.owner));
	}

	public override bool ShouldBeSaved()
	{
		return false;
	}

	private void MirrorProcessed()
	{
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteNetworkBehaviour(Network_003CtargetHero_003Ek__BackingField);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 0x100L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_003CtargetHero_003Ek__BackingField);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref targetHero, null, reader, ref ____003CtargetHero_003Ek__BackingFieldNetId);
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 0x100L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref targetHero, null, reader, ref ____003CtargetHero_003Ek__BackingFieldNetId);
		}
	}
}
