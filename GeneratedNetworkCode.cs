using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Mirror;

[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
public static class GeneratedNetworkCode
{
	public static TimeSnapshotMessage _Read_Mirror_002ETimeSnapshotMessage(NetworkReader reader)
	{
		return default(TimeSnapshotMessage);
	}

	public static void _Write_Mirror_002ETimeSnapshotMessage(NetworkWriter writer, TimeSnapshotMessage value)
	{
	}

	public static ReadyMessage _Read_Mirror_002EReadyMessage(NetworkReader reader)
	{
		return default(ReadyMessage);
	}

	public static void _Write_Mirror_002EReadyMessage(NetworkWriter writer, ReadyMessage value)
	{
	}

	public static NotReadyMessage _Read_Mirror_002ENotReadyMessage(NetworkReader reader)
	{
		return default(NotReadyMessage);
	}

	public static void _Write_Mirror_002ENotReadyMessage(NetworkWriter writer, NotReadyMessage value)
	{
	}

	public static AddPlayerMessage _Read_Mirror_002EAddPlayerMessage(NetworkReader reader)
	{
		return default(AddPlayerMessage);
	}

	public static void _Write_Mirror_002EAddPlayerMessage(NetworkWriter writer, AddPlayerMessage value)
	{
	}

	public static SceneMessage _Read_Mirror_002ESceneMessage(NetworkReader reader)
	{
		return new SceneMessage
		{
			sceneName = reader.ReadString(),
			sceneOperation = _Read_Mirror_002ESceneOperation(reader),
			customHandling = reader.ReadBool()
		};
	}

	public static SceneOperation _Read_Mirror_002ESceneOperation(NetworkReader reader)
	{
		return (SceneOperation)NetworkReaderExtensions.ReadByte(reader);
	}

	public static void _Write_Mirror_002ESceneMessage(NetworkWriter writer, SceneMessage value)
	{
		writer.WriteString(value.sceneName);
		_Write_Mirror_002ESceneOperation(writer, value.sceneOperation);
		writer.WriteBool(value.customHandling);
	}

	public static void _Write_Mirror_002ESceneOperation(NetworkWriter writer, SceneOperation value)
	{
		NetworkWriterExtensions.WriteByte(writer, (byte)value);
	}

	public static CommandMessage _Read_Mirror_002ECommandMessage(NetworkReader reader)
	{
		return new CommandMessage
		{
			netId = reader.ReadUInt(),
			componentIndex = NetworkReaderExtensions.ReadByte(reader),
			functionHash = reader.ReadUShort(),
			payload = reader.ReadBytesAndSizeSegment()
		};
	}

	public static void _Write_Mirror_002ECommandMessage(NetworkWriter writer, CommandMessage value)
	{
		writer.WriteUInt(value.netId);
		NetworkWriterExtensions.WriteByte(writer, value.componentIndex);
		writer.WriteUShort(value.functionHash);
		writer.WriteBytesAndSizeSegment(value.payload);
	}

	public static RpcMessage _Read_Mirror_002ERpcMessage(NetworkReader reader)
	{
		return new RpcMessage
		{
			netId = reader.ReadUInt(),
			componentIndex = NetworkReaderExtensions.ReadByte(reader),
			functionHash = reader.ReadUShort(),
			payload = reader.ReadBytesAndSizeSegment()
		};
	}

	public static void _Write_Mirror_002ERpcMessage(NetworkWriter writer, RpcMessage value)
	{
		writer.WriteUInt(value.netId);
		NetworkWriterExtensions.WriteByte(writer, value.componentIndex);
		writer.WriteUShort(value.functionHash);
		writer.WriteBytesAndSizeSegment(value.payload);
	}

	public static RpcBufferMessage _Read_Mirror_002ERpcBufferMessage(NetworkReader reader)
	{
		return new RpcBufferMessage
		{
			payload = reader.ReadBytesAndSizeSegment()
		};
	}

	public static void _Write_Mirror_002ERpcBufferMessage(NetworkWriter writer, RpcBufferMessage value)
	{
		writer.WriteBytesAndSizeSegment(value.payload);
	}

	public static SpawnMessage _Read_Mirror_002ESpawnMessage(NetworkReader reader)
	{
		return new SpawnMessage
		{
			netId = reader.ReadUInt(),
			isLocalPlayer = reader.ReadBool(),
			isOwner = reader.ReadBool(),
			sceneId = reader.ReadULong(),
			assetId = reader.ReadUInt(),
			position = reader.ReadVector3(),
			rotation = reader.ReadQuaternion(),
			scale = reader.ReadVector3(),
			payload = reader.ReadBytesAndSizeSegment()
		};
	}

	public static void _Write_Mirror_002ESpawnMessage(NetworkWriter writer, SpawnMessage value)
	{
		writer.WriteUInt(value.netId);
		writer.WriteBool(value.isLocalPlayer);
		writer.WriteBool(value.isOwner);
		writer.WriteULong(value.sceneId);
		writer.WriteUInt(value.assetId);
		writer.WriteVector3(value.position);
		writer.WriteQuaternion(value.rotation);
		writer.WriteVector3(value.scale);
		writer.WriteBytesAndSizeSegment(value.payload);
	}

	public static ChangeOwnerMessage _Read_Mirror_002EChangeOwnerMessage(NetworkReader reader)
	{
		return new ChangeOwnerMessage
		{
			netId = reader.ReadUInt(),
			isOwner = reader.ReadBool(),
			isLocalPlayer = reader.ReadBool()
		};
	}

	public static void _Write_Mirror_002EChangeOwnerMessage(NetworkWriter writer, ChangeOwnerMessage value)
	{
		writer.WriteUInt(value.netId);
		writer.WriteBool(value.isOwner);
		writer.WriteBool(value.isLocalPlayer);
	}

	public static ObjectSpawnStartedMessage _Read_Mirror_002EObjectSpawnStartedMessage(NetworkReader reader)
	{
		return default(ObjectSpawnStartedMessage);
	}

	public static void _Write_Mirror_002EObjectSpawnStartedMessage(NetworkWriter writer, ObjectSpawnStartedMessage value)
	{
	}

	public static ObjectSpawnFinishedMessage _Read_Mirror_002EObjectSpawnFinishedMessage(NetworkReader reader)
	{
		return default(ObjectSpawnFinishedMessage);
	}

	public static void _Write_Mirror_002EObjectSpawnFinishedMessage(NetworkWriter writer, ObjectSpawnFinishedMessage value)
	{
	}

	public static ObjectDestroyMessage _Read_Mirror_002EObjectDestroyMessage(NetworkReader reader)
	{
		return new ObjectDestroyMessage
		{
			netId = reader.ReadUInt()
		};
	}

	public static void _Write_Mirror_002EObjectDestroyMessage(NetworkWriter writer, ObjectDestroyMessage value)
	{
		writer.WriteUInt(value.netId);
	}

	public static ObjectHideMessage _Read_Mirror_002EObjectHideMessage(NetworkReader reader)
	{
		return new ObjectHideMessage
		{
			netId = reader.ReadUInt()
		};
	}

	public static void _Write_Mirror_002EObjectHideMessage(NetworkWriter writer, ObjectHideMessage value)
	{
		writer.WriteUInt(value.netId);
	}

	public static EntityStateMessage _Read_Mirror_002EEntityStateMessage(NetworkReader reader)
	{
		return new EntityStateMessage
		{
			netId = reader.ReadUInt(),
			payload = reader.ReadBytesAndSizeSegment()
		};
	}

	public static void _Write_Mirror_002EEntityStateMessage(NetworkWriter writer, EntityStateMessage value)
	{
		writer.WriteUInt(value.netId);
		writer.WriteBytesAndSizeSegment(value.payload);
	}

	public static NetworkPingMessage _Read_Mirror_002ENetworkPingMessage(NetworkReader reader)
	{
		return new NetworkPingMessage
		{
			clientTime = reader.ReadDouble()
		};
	}

	public static void _Write_Mirror_002ENetworkPingMessage(NetworkWriter writer, NetworkPingMessage value)
	{
		writer.WriteDouble(value.clientTime);
	}

	public static NetworkPongMessage _Read_Mirror_002ENetworkPongMessage(NetworkReader reader)
	{
		return new NetworkPongMessage
		{
			clientTime = reader.ReadDouble()
		};
	}

	public static void _Write_Mirror_002ENetworkPongMessage(NetworkWriter writer, NetworkPongMessage value)
	{
		writer.WriteDouble(value.clientTime);
	}

	public static DewEffect.PlayEffectMessage _Read_DewEffect_002FPlayEffectMessage(NetworkReader reader)
	{
		return new DewEffect.PlayEffectMessage
		{
			isNew = reader.ReadBool(),
			parent = reader.ReadNetworkIdentity(),
			path = reader.ReadString()
		};
	}

	public static void _Write_DewEffect_002FPlayEffectMessage(NetworkWriter writer, DewEffect.PlayEffectMessage value)
	{
		writer.WriteBool(value.isNew);
		writer.WriteNetworkIdentity(value.parent);
		writer.WriteString(value.path);
	}

	public static DewEffect.PlayPositionedEffectMessage _Read_DewEffect_002FPlayPositionedEffectMessage(NetworkReader reader)
	{
		return new DewEffect.PlayPositionedEffectMessage
		{
			isNew = reader.ReadBool(),
			parent = reader.ReadNetworkIdentity(),
			path = reader.ReadString(),
			position = reader.ReadVector3(),
			rotation = reader.ReadQuaternion()
		};
	}

	public static void _Write_DewEffect_002FPlayPositionedEffectMessage(NetworkWriter writer, DewEffect.PlayPositionedEffectMessage value)
	{
		writer.WriteBool(value.isNew);
		writer.WriteNetworkIdentity(value.parent);
		writer.WriteString(value.path);
		writer.WriteVector3(value.position);
		writer.WriteQuaternion(value.rotation);
	}

	public static DewEffect.PlayAttachedEffectMessage _Read_DewEffect_002FPlayAttachedEffectMessage(NetworkReader reader)
	{
		return new DewEffect.PlayAttachedEffectMessage
		{
			isNew = reader.ReadBool(),
			parent = reader.ReadNetworkIdentity(),
			path = reader.ReadString(),
			entity = reader.ReadNetworkBehaviour<Entity>()
		};
	}

	public static void _Write_DewEffect_002FPlayAttachedEffectMessage(NetworkWriter writer, DewEffect.PlayAttachedEffectMessage value)
	{
		writer.WriteBool(value.isNew);
		writer.WriteNetworkIdentity(value.parent);
		writer.WriteString(value.path);
		writer.WriteNetworkBehaviour(value.entity);
	}

	public static DewEffect.PlayCastEffectMessage _Read_DewEffect_002FPlayCastEffectMessage(NetworkReader reader)
	{
		return new DewEffect.PlayCastEffectMessage
		{
			parent = reader.ReadNetworkIdentity(),
			path = reader.ReadString(),
			info = _Read_CastInfo(reader),
			method = _Read_CastMethodType(reader),
			duration = reader.ReadFloat()
		};
	}

	public static CastInfo _Read_CastInfo(NetworkReader reader)
	{
		return new CastInfo
		{
			caster = reader.ReadNetworkBehaviour<Entity>(),
			target = reader.ReadNetworkBehaviour<Entity>(),
			point = reader.ReadVector3(),
			angle = reader.ReadFloat(),
			animSelectValue = reader.ReadFloat()
		};
	}

	public static CastMethodType _Read_CastMethodType(NetworkReader reader)
	{
		return (CastMethodType)reader.ReadInt();
	}

	public static void _Write_DewEffect_002FPlayCastEffectMessage(NetworkWriter writer, DewEffect.PlayCastEffectMessage value)
	{
		writer.WriteNetworkIdentity(value.parent);
		writer.WriteString(value.path);
		_Write_CastInfo(writer, value.info);
		_Write_CastMethodType(writer, value.method);
		writer.WriteFloat(value.duration);
	}

	public static void _Write_CastInfo(NetworkWriter writer, CastInfo value)
	{
		writer.WriteNetworkBehaviour(value.caster);
		writer.WriteNetworkBehaviour(value.target);
		writer.WriteVector3(value.point);
		writer.WriteFloat(value.angle);
		writer.WriteFloat(value.animSelectValue);
	}

	public static void _Write_CastMethodType(NetworkWriter writer, CastMethodType value)
	{
		writer.WriteInt((int)value);
	}

	public static DewEffect.StopEffectMessage _Read_DewEffect_002FStopEffectMessage(NetworkReader reader)
	{
		return new DewEffect.StopEffectMessage
		{
			parent = reader.ReadNetworkIdentity(),
			path = reader.ReadString()
		};
	}

	public static void _Write_DewEffect_002FStopEffectMessage(NetworkWriter writer, DewEffect.StopEffectMessage value)
	{
		writer.WriteNetworkIdentity(value.parent);
		writer.WriteString(value.path);
	}

	public static DewNetworkManager.ChangeSceneMessage _Read_DewNetworkManager_002FChangeSceneMessage(NetworkReader reader)
	{
		return new DewNetworkManager.ChangeSceneMessage
		{
			name = reader.ReadString()
		};
	}

	public static void _Write_DewNetworkManager_002FChangeSceneMessage(NetworkWriter writer, DewNetworkManager.ChangeSceneMessage value)
	{
		writer.WriteString(value.name);
	}

	public static DewNetworkManager.SetLoadingStatusMessage _Read_DewNetworkManager_002FSetLoadingStatusMessage(NetworkReader reader)
	{
		return new DewNetworkManager.SetLoadingStatusMessage
		{
			isLoading = reader.ReadBool()
		};
	}

	public static void _Write_DewNetworkManager_002FSetLoadingStatusMessage(NetworkWriter writer, DewNetworkManager.SetLoadingStatusMessage value)
	{
		writer.WriteBool(value.isLoading);
	}

	public static DewNetworkManager.SessionEndedMessage _Read_DewNetworkManager_002FSessionEndedMessage(NetworkReader reader)
	{
		return default(DewNetworkManager.SessionEndedMessage);
	}

	public static void _Write_DewNetworkManager_002FSessionEndedMessage(NetworkWriter writer, DewNetworkManager.SessionEndedMessage value)
	{
	}

	public static DewNetworkManager.SessionRestartingMessage _Read_DewNetworkManager_002FSessionRestartingMessage(NetworkReader reader)
	{
		return default(DewNetworkManager.SessionRestartingMessage);
	}

	public static void _Write_DewNetworkManager_002FSessionRestartingMessage(NetworkWriter writer, DewNetworkManager.SessionRestartingMessage value)
	{
	}

	public static InGameAnalyticsManager.DisableAnalyticsMessage _Read_InGameAnalyticsManager_002FDisableAnalyticsMessage(NetworkReader reader)
	{
		return default(InGameAnalyticsManager.DisableAnalyticsMessage);
	}

	public static void _Write_InGameAnalyticsManager_002FDisableAnalyticsMessage(NetworkWriter writer, InGameAnalyticsManager.DisableAnalyticsMessage value)
	{
	}

	public static void _Write_Projectile_002FEntityHit(NetworkWriter writer, Projectile.EntityHit value)
	{
		writer.WriteNetworkBehaviour(value.entity);
		writer.WriteVector3(value.point);
	}

	public static Projectile.EntityHit _Read_Projectile_002FEntityHit(NetworkReader reader)
	{
		return new Projectile.EntityHit
		{
			entity = reader.ReadNetworkBehaviour<Entity>(),
			point = reader.ReadVector3()
		};
	}

	public static void _Write_Projectile_002FProjectileMode(NetworkWriter writer, Projectile.ProjectileMode value)
	{
		writer.WriteInt((int)value);
	}

	public static Projectile.ProjectileMode _Read_Projectile_002FProjectileMode(NetworkReader reader)
	{
		return (Projectile.ProjectileMode)reader.ReadInt();
	}

	public static void _Write_EventInfoKill(NetworkWriter writer, EventInfoKill value)
	{
		writer.WriteNetworkBehaviour(value.actor);
		writer.WriteNetworkBehaviour(value.victim);
	}

	public static EventInfoKill _Read_EventInfoKill(NetworkReader reader)
	{
		return new EventInfoKill
		{
			actor = reader.ReadNetworkBehaviour<Actor>(),
			victim = reader.ReadNetworkBehaviour<Entity>()
		};
	}

	public static void _Write_Monster_002FMonsterType(NetworkWriter writer, Monster.MonsterType value)
	{
		NetworkWriterExtensions.WriteByte(writer, (byte)value);
	}

	public static Monster.MonsterType _Read_Monster_002FMonsterType(NetworkReader reader)
	{
		return (Monster.MonsterType)NetworkReaderExtensions.ReadByte(reader);
	}

	public static MerchandiseData[] _Read_MerchandiseData_005B_005D(NetworkReader reader)
	{
		return reader.ReadArray<MerchandiseData>();
	}

	public static MerchandiseData _Read_MerchandiseData(NetworkReader reader)
	{
		return new MerchandiseData
		{
			type = _Read_MerchandiseType(reader),
			itemName = reader.ReadString(),
			level = reader.ReadInt(),
			price = _Read_Cost(reader),
			count = reader.ReadInt(),
			customData = reader.ReadString()
		};
	}

	public static MerchandiseType _Read_MerchandiseType(NetworkReader reader)
	{
		return (MerchandiseType)reader.ReadInt();
	}

	public static Cost _Read_Cost(NetworkReader reader)
	{
		return new Cost
		{
			gold = reader.ReadInt(),
			dreamDust = reader.ReadInt(),
			stardust = reader.ReadInt(),
			healthPercentage = reader.ReadInt()
		};
	}

	public static void _Write_MerchandiseData_005B_005D(NetworkWriter writer, MerchandiseData[] value)
	{
		writer.WriteArray(value);
	}

	public static void _Write_MerchandiseData(NetworkWriter writer, MerchandiseData value)
	{
		_Write_MerchandiseType(writer, value.type);
		writer.WriteString(value.itemName);
		writer.WriteInt(value.level);
		_Write_Cost(writer, value.price);
		writer.WriteInt(value.count);
		writer.WriteString(value.customData);
	}

	public static void _Write_MerchandiseType(NetworkWriter writer, MerchandiseType value)
	{
		writer.WriteInt((int)value);
	}

	public static void _Write_Cost(NetworkWriter writer, Cost value)
	{
		writer.WriteInt(value.gold);
		writer.WriteInt(value.dreamDust);
		writer.WriteInt(value.stardust);
		writer.WriteInt(value.healthPercentage);
	}

	public static void _Write_AbilityTrigger_002FConfigSyncData_005B_005D(NetworkWriter writer, AbilityTrigger.ConfigSyncData[] value)
	{
		writer.WriteArray(value);
	}

	public static void _Write_AbilityTrigger_002FConfigSyncData(NetworkWriter writer, AbilityTrigger.ConfigSyncData value)
	{
		writer.WriteFloat(value.manaCost);
		writer.WriteInt(value.maxCharges);
		writer.WriteInt(value.addedCharges);
		writer.WriteFloat(value.cooldownTime);
		writer.WriteFloat(value.minimumDelay);
	}

	public static AbilityTrigger.ConfigSyncData[] _Read_AbilityTrigger_002FConfigSyncData_005B_005D(NetworkReader reader)
	{
		return reader.ReadArray<AbilityTrigger.ConfigSyncData>();
	}

	public static AbilityTrigger.ConfigSyncData _Read_AbilityTrigger_002FConfigSyncData(NetworkReader reader)
	{
		return new AbilityTrigger.ConfigSyncData
		{
			manaCost = reader.ReadFloat(),
			maxCharges = reader.ReadInt(),
			addedCharges = reader.ReadInt(),
			cooldownTime = reader.ReadFloat(),
			minimumDelay = reader.ReadFloat()
		};
	}

	public static void _Write_QuestProgressType(NetworkWriter writer, QuestProgressType value)
	{
		writer.WriteInt((int)value);
	}

	public static QuestProgressType _Read_QuestProgressType(NetworkReader reader)
	{
		return (QuestProgressType)reader.ReadInt();
	}

	public static ChoiceShrineItem _Read_ChoiceShrineItem(NetworkReader reader)
	{
		return new ChoiceShrineItem
		{
			typeName = reader.ReadString(),
			level = reader.ReadInt()
		};
	}

	public static void _Write_ChoiceShrineItem(NetworkWriter writer, ChoiceShrineItem value)
	{
		writer.WriteString(value.typeName);
		writer.WriteInt(value.level);
	}

	public static ChaosReward[] _Read_ChaosReward_005B_005D(NetworkReader reader)
	{
		return reader.ReadArray<ChaosReward>();
	}

	public static ChaosReward _Read_ChaosReward(NetworkReader reader)
	{
		return new ChaosReward
		{
			type = _Read_ChaosRewardType(reader),
			quantity = reader.ReadFloat(),
			rarity = _Read_Rarity(reader)
		};
	}

	public static ChaosRewardType _Read_ChaosRewardType(NetworkReader reader)
	{
		return (ChaosRewardType)reader.ReadInt();
	}

	public static Rarity _Read_Rarity(NetworkReader reader)
	{
		return (Rarity)NetworkReaderExtensions.ReadByte(reader);
	}

	public static void _Write_ChaosReward_005B_005D(NetworkWriter writer, ChaosReward[] value)
	{
		writer.WriteArray(value);
	}

	public static void _Write_ChaosReward(NetworkWriter writer, ChaosReward value)
	{
		_Write_ChaosRewardType(writer, value.type);
		writer.WriteFloat(value.quantity);
		_Write_Rarity(writer, value.rarity);
	}

	public static void _Write_ChaosRewardType(NetworkWriter writer, ChaosRewardType value)
	{
		writer.WriteInt((int)value);
	}

	public static void _Write_Rarity(NetworkWriter writer, Rarity value)
	{
		NetworkWriterExtensions.WriteByte(writer, (byte)value);
	}

	public static void _Write_HeroLoadoutData(NetworkWriter writer, HeroLoadoutData value)
	{
		if (value == null)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		writer.WriteInt(value.skillQ);
		writer.WriteInt(value.skillR);
		writer.WriteInt(value.skillTrait);
		writer.WriteInt(value.skillMovement);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(writer, value.cDestruction);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(writer, value.cLife);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(writer, value.cImagination);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(writer, value.cFlexible);
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(NetworkWriter writer, List<LoadoutStarItem> value)
	{
		writer.WriteList(value);
	}

	public static void _Write_LoadoutStarItem(NetworkWriter writer, LoadoutStarItem value)
	{
		writer.WriteString(value.name);
		writer.WriteInt(value.level);
	}

	public static HeroLoadoutData _Read_HeroLoadoutData(NetworkReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		HeroLoadoutData heroLoadoutData = new HeroLoadoutData();
		heroLoadoutData.skillQ = reader.ReadInt();
		heroLoadoutData.skillR = reader.ReadInt();
		heroLoadoutData.skillTrait = reader.ReadInt();
		heroLoadoutData.skillMovement = reader.ReadInt();
		heroLoadoutData.cDestruction = _Read_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(reader);
		heroLoadoutData.cLife = _Read_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(reader);
		heroLoadoutData.cImagination = _Read_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(reader);
		heroLoadoutData.cFlexible = _Read_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(reader);
		return heroLoadoutData;
	}

	public static List<LoadoutStarItem> _Read_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E(NetworkReader reader)
	{
		return reader.ReadList<LoadoutStarItem>();
	}

	public static LoadoutStarItem _Read_LoadoutStarItem(NetworkReader reader)
	{
		return new LoadoutStarItem
		{
			name = reader.ReadString(),
			level = reader.ReadInt()
		};
	}

	public static PlayerStarItem _Read_PlayerStarItem(NetworkReader reader)
	{
		return new PlayerStarItem
		{
			type = reader.ReadString(),
			level = reader.ReadInt()
		};
	}

	public static void _Write_PlayerStarItem(NetworkWriter writer, PlayerStarItem value)
	{
		writer.WriteString(value.type);
		writer.WriteInt(value.level);
	}

	public static void _Write_System_002EString_005B_005D(NetworkWriter writer, string[] value)
	{
		writer.WriteArray(value);
	}

	public static string[] _Read_System_002EString_005B_005D(NetworkReader reader)
	{
		return reader.ReadArray<string>();
	}

	public static void _Write_InputMode(NetworkWriter writer, InputMode value)
	{
		writer.WriteInt((int)value);
	}

	public static InputMode _Read_InputMode(NetworkReader reader)
	{
		return (InputMode)reader.ReadInt();
	}

	public static void _Write_WorldMessageSetting(NetworkWriter writer, WorldMessageSetting value)
	{
		if (value == null)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		writer.WriteString(value.rawText);
		writer.WriteVector3(value.worldPos);
		writer.WriteColor(value.color);
		writer.WriteVector2Nullable(value.popOffset);
	}

	public static WorldMessageSetting _Read_WorldMessageSetting(NetworkReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		WorldMessageSetting worldMessageSetting = new WorldMessageSetting();
		worldMessageSetting.rawText = reader.ReadString();
		worldMessageSetting.worldPos = reader.ReadVector3();
		worldMessageSetting.color = reader.ReadColor();
		worldMessageSetting.popOffset = reader.ReadVector2Nullable();
		return worldMessageSetting;
	}

	public static void _Write_CenterMessageType(NetworkWriter writer, CenterMessageType value)
	{
		writer.WriteInt((int)value);
	}

	public static CenterMessageType _Read_CenterMessageType(NetworkReader reader)
	{
		return (CenterMessageType)reader.ReadInt();
	}

	public static void _Write_NetworkedOnScreenTimerHandle(NetworkWriter writer, NetworkedOnScreenTimerHandle value)
	{
		if (value == null)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		writer.WriteString(value.localeKey);
		writer.WriteString(value.skillKey);
		writer.WriteInt(value._id);
	}

	public static NetworkedOnScreenTimerHandle _Read_NetworkedOnScreenTimerHandle(NetworkReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		NetworkedOnScreenTimerHandle networkedOnScreenTimerHandle = new NetworkedOnScreenTimerHandle();
		networkedOnScreenTimerHandle.localeKey = reader.ReadString();
		networkedOnScreenTimerHandle.skillKey = reader.ReadString();
		networkedOnScreenTimerHandle._id = reader.ReadInt();
		return networkedOnScreenTimerHandle;
	}

	public static void _Write_SampleCastInfoContext(NetworkWriter writer, SampleCastInfoContext value)
	{
		writer.WriteCastMethodData(value.castMethod);
		_Write_AbilityTargetValidator(writer, value.targetValidator);
		writer.WriteNetworkBehaviour(value.trigger);
		writer.WriteBool(value.showCastIndicator);
		writer.WriteBool(value.isInitialInfoSet);
		_Write_CastInfo(writer, value.currentInfo);
		_Write_SampleCastInfoContext_002FCastOnButtonType(writer, value.castOnButton);
		writer.WriteFloat(value.angleSpeedLimit);
	}

	public static void _Write_AbilityTargetValidator(NetworkWriter writer, AbilityTargetValidator value)
	{
		if (value == null)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		_Write_EntityRelation(writer, value.targets);
	}

	public static void _Write_EntityRelation(NetworkWriter writer, EntityRelation value)
	{
		NetworkWriterExtensions.WriteByte(writer, (byte)value);
	}

	public static void _Write_SampleCastInfoContext_002FCastOnButtonType(NetworkWriter writer, SampleCastInfoContext.CastOnButtonType value)
	{
		writer.WriteInt((int)value);
	}

	public static SampleCastInfoContext _Read_SampleCastInfoContext(NetworkReader reader)
	{
		return new SampleCastInfoContext
		{
			castMethod = reader.ReadWriteCastMethodData(),
			targetValidator = _Read_AbilityTargetValidator(reader),
			trigger = reader.ReadNetworkBehaviour<AbilityTrigger>(),
			showCastIndicator = reader.ReadBool(),
			isInitialInfoSet = reader.ReadBool(),
			currentInfo = _Read_CastInfo(reader),
			castOnButton = _Read_SampleCastInfoContext_002FCastOnButtonType(reader),
			angleSpeedLimit = reader.ReadFloat()
		};
	}

	public static AbilityTargetValidator _Read_AbilityTargetValidator(NetworkReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		AbilityTargetValidator abilityTargetValidator = new AbilityTargetValidator();
		abilityTargetValidator.targets = _Read_EntityRelation(reader);
		return abilityTargetValidator;
	}

	public static EntityRelation _Read_EntityRelation(NetworkReader reader)
	{
		return (EntityRelation)NetworkReaderExtensions.ReadByte(reader);
	}

	public static SampleCastInfoContext.CastOnButtonType _Read_SampleCastInfoContext_002FCastOnButtonType(NetworkReader reader)
	{
		return (SampleCastInfoContext.CastOnButtonType)reader.ReadInt();
	}

	public static void _Write_Steamworks_002ECSteamID(NetworkWriter writer, CSteamID value)
	{
		writer.WriteULong(value.m_SteamID);
	}

	public static CSteamID _Read_Steamworks_002ECSteamID(NetworkReader reader)
	{
		return new CSteamID
		{
			m_SteamID = reader.ReadULong()
		};
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E(NetworkWriter writer, List<string> value)
	{
		writer.WriteList(value);
	}

	public static List<string> _Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E(NetworkReader reader)
	{
		return reader.ReadList<string>();
	}

	public static void _Write_DewPlayer_002FRole(NetworkWriter writer, DewPlayer.Role value)
	{
		NetworkWriterExtensions.WriteByte(writer, (byte)value);
	}

	public static DewPlayer.Role _Read_DewPlayer_002FRole(NetworkReader reader)
	{
		return (DewPlayer.Role)NetworkReaderExtensions.ReadByte(reader);
	}

	public static void _Write_HatredStrengthType(NetworkWriter writer, HatredStrengthType value)
	{
		writer.WriteInt((int)value);
	}

	public static HatredStrengthType _Read_HatredStrengthType(NetworkReader reader)
	{
		return (HatredStrengthType)reader.ReadInt();
	}

	public static void _Write_EventInfoSkillUse(NetworkWriter writer, EventInfoSkillUse value)
	{
		_Write_HeroSkillLocation(writer, value.type);
		writer.WriteNetworkBehaviour(value.skill);
	}

	public static void _Write_HeroSkillLocation(NetworkWriter writer, HeroSkillLocation value)
	{
		writer.WriteInt((int)value);
	}

	public static EventInfoSkillUse _Read_EventInfoSkillUse(NetworkReader reader)
	{
		return new EventInfoSkillUse
		{
			type = _Read_HeroSkillLocation(reader),
			skill = reader.ReadNetworkBehaviour<SkillTrigger>()
		};
	}

	public static HeroSkillLocation _Read_HeroSkillLocation(NetworkReader reader)
	{
		return (HeroSkillLocation)reader.ReadInt();
	}

	public static void _Write_DescriptionTags(NetworkWriter writer, DescriptionTags value)
	{
		writer.WriteInt((int)value);
	}

	public static DescriptionTags _Read_DescriptionTags(NetworkReader reader)
	{
		return (DescriptionTags)reader.ReadInt();
	}

	public static SyncedNetworkBehaviour _Read_SyncedNetworkBehaviour(NetworkReader reader)
	{
		return new SyncedNetworkBehaviour
		{
			netId = reader.ReadUInt(),
			componentIndex = NetworkReaderExtensions.ReadByte(reader)
		};
	}

	public static void _Write_SyncedNetworkBehaviour(NetworkWriter writer, SyncedNetworkBehaviour value)
	{
		writer.WriteUInt(value.netId);
		NetworkWriterExtensions.WriteByte(writer, value.componentIndex);
	}

	public static void _Write_EntityAnimation_002FReplaceableAnimationType(NetworkWriter writer, EntityAnimation.ReplaceableAnimationType value)
	{
		writer.WriteInt((int)value);
	}

	public static EntityAnimation.ReplaceableAnimationType _Read_EntityAnimation_002FReplaceableAnimationType(NetworkReader reader)
	{
		return (EntityAnimation.ReplaceableAnimationType)reader.ReadInt();
	}

	public static void _Write_EntityControl_002FBlockableAction(NetworkWriter writer, EntityControl.BlockableAction value)
	{
		writer.WriteInt((int)value);
	}

	public static EntityControl.BlockableAction _Read_EntityControl_002FBlockableAction(NetworkReader reader)
	{
		return (EntityControl.BlockableAction)reader.ReadInt();
	}

	public static void _Write_EntityControl_002FPositionSyncData(NetworkWriter writer, EntityControl.PositionSyncData value)
	{
		writer.WriteDouble(value.timestamp);
		writer.WriteVector3(value.position);
		writer.WriteVector3(value.velocity);
		writer.WriteFloat(value.desiredAngle);
	}

	public static EntityControl.PositionSyncData _Read_EntityControl_002FPositionSyncData(NetworkReader reader)
	{
		return new EntityControl.PositionSyncData
		{
			timestamp = reader.ReadDouble(),
			position = reader.ReadVector3(),
			velocity = reader.ReadVector3(),
			desiredAngle = reader.ReadFloat()
		};
	}

	public static void _Write_EventInfoStatusEffect(NetworkWriter writer, EventInfoStatusEffect value)
	{
		writer.WriteNetworkBehaviour(value.victim);
		writer.WriteNetworkBehaviour(value.effect);
	}

	public static EventInfoStatusEffect _Read_EventInfoStatusEffect(NetworkReader reader)
	{
		return new EventInfoStatusEffect
		{
			victim = reader.ReadNetworkBehaviour<Entity>(),
			effect = reader.ReadNetworkBehaviour<StatusEffect>()
		};
	}

	public static void _Write_BaseStats(NetworkWriter writer, BaseStats value)
	{
		writer.WriteFloat(value.attackDamage);
		writer.WriteFloat(value.abilityPower);
		writer.WriteFloat(value.maxHealth);
		writer.WriteFloat(value.maxMana);
		writer.WriteFloat(value.healthRegen);
		writer.WriteFloat(value.manaRegen);
		writer.WriteFloat(value.critAmp);
		writer.WriteFloat(value.critChance);
		writer.WriteFloat(value.abilityHaste);
		writer.WriteFloat(value.tenacity);
		writer.WriteFloat(value.fireEffectAmp);
		writer.WriteFloat(value.coldEffectAmp);
		writer.WriteFloat(value.lightEffectAmp);
		writer.WriteFloat(value.darkEffectAmp);
		writer.WriteFloat(value.armor);
	}

	public static void _Write_BonusStats(NetworkWriter writer, BonusStats value)
	{
		writer.WriteFloat(value.attackDamageFlat);
		writer.WriteFloat(value.attackDamagePercentage);
		writer.WriteFloat(value.abilityPowerFlat);
		writer.WriteFloat(value.abilityPowerPercentage);
		writer.WriteFloat(value.maxHealthFlat);
		writer.WriteFloat(value.maxHealthPercentage);
		writer.WriteFloat(value.maxManaFlat);
		writer.WriteFloat(value.maxManaPercentage);
		writer.WriteFloat(value.healthRegenFlat);
		writer.WriteFloat(value.healthRegenPercentage);
		writer.WriteFloat(value.manaRegenFlat);
		writer.WriteFloat(value.manaRegenPercentage);
		writer.WriteFloat(value.attackSpeedPercentage);
		writer.WriteFloat(value.critAmpFlat);
		writer.WriteFloat(value.critAmpPercentage);
		writer.WriteFloat(value.critChanceFlat);
		writer.WriteFloat(value.critChancePercentage);
		writer.WriteFloat(value.abilityHasteFlat);
		writer.WriteFloat(value.abilityHastePercentage);
		writer.WriteFloat(value.tenacityFlat);
		writer.WriteFloat(value.tenacityPercentage);
		writer.WriteFloat(value.movementSpeedPercentage);
		writer.WriteFloat(value.fireEffectAmpFlat);
		writer.WriteFloat(value.coldEffectAmpFlat);
		writer.WriteFloat(value.lightEffectAmpFlat);
		writer.WriteFloat(value.darkEffectAmpFlat);
		writer.WriteFloat(value.armorFlat);
		writer.WriteFloat(value.armorPercentage);
	}

	public static void _Write_BasicEffectMask(NetworkWriter writer, BasicEffectMask value)
	{
		writer.WriteUInt((uint)value);
	}

	public static BaseStats _Read_BaseStats(NetworkReader reader)
	{
		return new BaseStats
		{
			attackDamage = reader.ReadFloat(),
			abilityPower = reader.ReadFloat(),
			maxHealth = reader.ReadFloat(),
			maxMana = reader.ReadFloat(),
			healthRegen = reader.ReadFloat(),
			manaRegen = reader.ReadFloat(),
			critAmp = reader.ReadFloat(),
			critChance = reader.ReadFloat(),
			abilityHaste = reader.ReadFloat(),
			tenacity = reader.ReadFloat(),
			fireEffectAmp = reader.ReadFloat(),
			coldEffectAmp = reader.ReadFloat(),
			lightEffectAmp = reader.ReadFloat(),
			darkEffectAmp = reader.ReadFloat(),
			armor = reader.ReadFloat()
		};
	}

	public static BonusStats _Read_BonusStats(NetworkReader reader)
	{
		return new BonusStats
		{
			attackDamageFlat = reader.ReadFloat(),
			attackDamagePercentage = reader.ReadFloat(),
			abilityPowerFlat = reader.ReadFloat(),
			abilityPowerPercentage = reader.ReadFloat(),
			maxHealthFlat = reader.ReadFloat(),
			maxHealthPercentage = reader.ReadFloat(),
			maxManaFlat = reader.ReadFloat(),
			maxManaPercentage = reader.ReadFloat(),
			healthRegenFlat = reader.ReadFloat(),
			healthRegenPercentage = reader.ReadFloat(),
			manaRegenFlat = reader.ReadFloat(),
			manaRegenPercentage = reader.ReadFloat(),
			attackSpeedPercentage = reader.ReadFloat(),
			critAmpFlat = reader.ReadFloat(),
			critAmpPercentage = reader.ReadFloat(),
			critChanceFlat = reader.ReadFloat(),
			critChancePercentage = reader.ReadFloat(),
			abilityHasteFlat = reader.ReadFloat(),
			abilityHastePercentage = reader.ReadFloat(),
			tenacityFlat = reader.ReadFloat(),
			tenacityPercentage = reader.ReadFloat(),
			movementSpeedPercentage = reader.ReadFloat(),
			fireEffectAmpFlat = reader.ReadFloat(),
			coldEffectAmpFlat = reader.ReadFloat(),
			lightEffectAmpFlat = reader.ReadFloat(),
			darkEffectAmpFlat = reader.ReadFloat(),
			armorFlat = reader.ReadFloat(),
			armorPercentage = reader.ReadFloat()
		};
	}

	public static BasicEffectMask _Read_BasicEffectMask(NetworkReader reader)
	{
		return (BasicEffectMask)reader.ReadUInt();
	}

	public static void _Write_GibInfo(NetworkWriter writer, GibInfo value)
	{
		writer.WriteVector3(value.velocity);
		writer.WriteVector3(value.normalizedCurrentDamage);
		writer.WriteFloat(value.yVelocity);
	}

	public static GibInfo _Read_GibInfo(NetworkReader reader)
	{
		return new GibInfo
		{
			velocity = reader.ReadVector3(),
			normalizedCurrentDamage = reader.ReadVector3(),
			yVelocity = reader.ReadFloat()
		};
	}

	public static void _Write_KnockUpStrength(NetworkWriter writer, KnockUpStrength value)
	{
		writer.WriteInt((int)value);
	}

	public static KnockUpStrength _Read_KnockUpStrength(NetworkReader reader)
	{
		return (KnockUpStrength)reader.ReadInt();
	}

	public static GemLocation _Read_GemLocation(NetworkReader reader)
	{
		return new GemLocation
		{
			skill = _Read_HeroSkillLocation(reader),
			index = reader.ReadInt()
		};
	}

	public static void _Write_GemLocation(NetworkWriter writer, GemLocation value)
	{
		_Write_HeroSkillLocation(writer, value.skill);
		writer.WriteInt(value.index);
	}

	public static void _Write_ChatManager_002FMessage(NetworkWriter writer, ChatManager.Message value)
	{
		_Write_ChatManager_002FMessageType(writer, value.type);
		writer.WriteString(value.content);
		_Write_System_002EString_005B_005D(writer, value.args);
		writer.WriteString(value.itemType);
		writer.WriteInt(value.itemLevel);
		_Write_Cost(writer, value.itemPrice);
		writer.WriteString(value.itemCustomData);
	}

	public static void _Write_ChatManager_002FMessageType(NetworkWriter writer, ChatManager.MessageType value)
	{
		NetworkWriterExtensions.WriteByte(writer, (byte)value);
	}

	public static ChatManager.Message _Read_ChatManager_002FMessage(NetworkReader reader)
	{
		return new ChatManager.Message
		{
			type = _Read_ChatManager_002FMessageType(reader),
			content = reader.ReadString(),
			args = _Read_System_002EString_005B_005D(reader),
			itemType = reader.ReadString(),
			itemLevel = reader.ReadInt(),
			itemPrice = _Read_Cost(reader),
			itemCustomData = reader.ReadString()
		};
	}

	public static ChatManager.MessageType _Read_ChatManager_002FMessageType(NetworkReader reader)
	{
		return (ChatManager.MessageType)NetworkReaderExtensions.ReadByte(reader);
	}

	public static void _Write_EventInfoHeal(NetworkWriter writer, EventInfoHeal value)
	{
		writer.WriteNetworkBehaviour(value.actor);
		writer.WriteNetworkBehaviour(value.target);
		_Write_FinalHealData(writer, value.heal);
		writer.WriteFloat(value.amount);
		writer.WriteFloat(value.discardedAmount);
		writer.WriteBool(value.isCrit);
		writer.WriteBool(value.canMerge);
		_Write_ReactionChain(writer, value.chain);
	}

	public static void _Write_FinalHealData(NetworkWriter writer, FinalHealData value)
	{
		writer.WriteFloat(value.amount);
		writer.WriteFloat(value.discardedAmount);
		writer.WriteBool(value.isCrit);
		_Write_ActorFlags(writer, value._flags);
	}

	public static void _Write_ActorFlags(NetworkWriter writer, ActorFlags value)
	{
	}

	public static void _Write_ReactionChain(NetworkWriter writer, ReactionChain value)
	{
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CActor_003E(writer, value._actors);
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CActor_003E(NetworkWriter writer, List<Actor> value)
	{
		writer.WriteList(value);
	}

	public static EventInfoHeal _Read_EventInfoHeal(NetworkReader reader)
	{
		return new EventInfoHeal
		{
			actor = reader.ReadNetworkBehaviour<Actor>(),
			target = reader.ReadNetworkBehaviour<Entity>(),
			heal = _Read_FinalHealData(reader),
			amount = reader.ReadFloat(),
			discardedAmount = reader.ReadFloat(),
			isCrit = reader.ReadBool(),
			canMerge = reader.ReadBool(),
			chain = _Read_ReactionChain(reader)
		};
	}

	public static FinalHealData _Read_FinalHealData(NetworkReader reader)
	{
		return new FinalHealData
		{
			amount = reader.ReadFloat(),
			discardedAmount = reader.ReadFloat(),
			isCrit = reader.ReadBool(),
			_flags = _Read_ActorFlags(reader)
		};
	}

	public static ActorFlags _Read_ActorFlags(NetworkReader reader)
	{
		return default(ActorFlags);
	}

	public static ReactionChain _Read_ReactionChain(NetworkReader reader)
	{
		return new ReactionChain
		{
			_actors = _Read_System_002ECollections_002EGeneric_002EList_00601_003CActor_003E(reader)
		};
	}

	public static List<Actor> _Read_System_002ECollections_002EGeneric_002EList_00601_003CActor_003E(NetworkReader reader)
	{
		return reader.ReadList<Actor>();
	}

	public static void _Write_EventInfoDamage(NetworkWriter writer, EventInfoDamage value)
	{
		writer.WriteNetworkBehaviour(value.actor);
		writer.WriteNetworkBehaviour(value.victim);
		_Write_FinalDamageData(writer, value.damage);
	}

	public static void _Write_FinalDamageData(NetworkWriter writer, FinalDamageData value)
	{
		writer.WriteFloat(value.amount);
		writer.WriteFloat(value.discardedAmount);
		writer.WriteFloat(value.procCoefficient);
		_Write_AttackEffectType(writer, value.attackEffectType);
		writer.WriteFloat(value.attackEffectStrength);
		writer.WriteNullableElementalType(value.elemental);
		writer.WriteVector3Nullable(value.direction);
		_Write_DamageData_002FSourceType(writer, value.type);
		_Write_DamageAttribute(writer, value.attributes);
		writer.WriteIntNullable(value.overrideElementalStacks);
		_Write_ActorFlags(writer, value._modifyFlags);
	}

	public static void _Write_AttackEffectType(NetworkWriter writer, AttackEffectType value)
	{
		writer.WriteInt((int)value);
	}

	public static void _Write_DamageData_002FSourceType(NetworkWriter writer, DamageData.SourceType value)
	{
		writer.WriteInt((int)value);
	}

	public static void _Write_DamageAttribute(NetworkWriter writer, DamageAttribute value)
	{
		writer.WriteLong((long)value);
	}

	public static EventInfoDamage _Read_EventInfoDamage(NetworkReader reader)
	{
		return new EventInfoDamage
		{
			actor = reader.ReadNetworkBehaviour<Actor>(),
			victim = reader.ReadNetworkBehaviour<Entity>(),
			damage = _Read_FinalDamageData(reader)
		};
	}

	public static FinalDamageData _Read_FinalDamageData(NetworkReader reader)
	{
		return new FinalDamageData
		{
			amount = reader.ReadFloat(),
			discardedAmount = reader.ReadFloat(),
			procCoefficient = reader.ReadFloat(),
			attackEffectType = _Read_AttackEffectType(reader),
			attackEffectStrength = reader.ReadFloat(),
			elemental = reader.ReadNullableElementalType(),
			direction = reader.ReadVector3Nullable(),
			type = _Read_DamageData_002FSourceType(reader),
			attributes = _Read_DamageAttribute(reader),
			overrideElementalStacks = reader.ReadIntNullable(),
			_modifyFlags = _Read_ActorFlags(reader)
		};
	}

	public static AttackEffectType _Read_AttackEffectType(NetworkReader reader)
	{
		return (AttackEffectType)reader.ReadInt();
	}

	public static DamageData.SourceType _Read_DamageData_002FSourceType(NetworkReader reader)
	{
		return (DamageData.SourceType)reader.ReadInt();
	}

	public static DamageAttribute _Read_DamageAttribute(NetworkReader reader)
	{
		return (DamageAttribute)reader.ReadLong();
	}

	public static void _Write_EventInfoSpentMana(NetworkWriter writer, EventInfoSpentMana value)
	{
		writer.WriteNetworkBehaviour(value.actor);
		writer.WriteNetworkBehaviour(value.entity);
		writer.WriteFloat(value.amount);
	}

	public static EventInfoSpentMana _Read_EventInfoSpentMana(NetworkReader reader)
	{
		return new EventInfoSpentMana
		{
			actor = reader.ReadNetworkBehaviour<Actor>(),
			entity = reader.ReadNetworkBehaviour<Entity>(),
			amount = reader.ReadFloat()
		};
	}

	public static void _Write_EventInfoAttackMissed(NetworkWriter writer, EventInfoAttackMissed value)
	{
		writer.WriteNetworkBehaviour(value.actor);
		writer.WriteNetworkBehaviour(value.attacker);
		writer.WriteNetworkBehaviour(value.victim);
		writer.WriteBool(value.isCrit);
	}

	public static EventInfoAttackMissed _Read_EventInfoAttackMissed(NetworkReader reader)
	{
		return new EventInfoAttackMissed
		{
			actor = reader.ReadNetworkBehaviour<Actor>(),
			attacker = reader.ReadNetworkBehaviour<Entity>(),
			victim = reader.ReadNetworkBehaviour<Entity>(),
			isCrit = reader.ReadBool()
		};
	}

	public static void _Write_EventInfoDamageNegatedByImmunity(NetworkWriter writer, EventInfoDamageNegatedByImmunity value)
	{
		writer.WriteNetworkBehaviour(value.actor);
		writer.WriteBasicEffect(value.effect);
		writer.WriteNetworkBehaviour(value.victim);
		_Write_FinalDamageData(writer, value.data);
	}

	public static EventInfoDamageNegatedByImmunity _Read_EventInfoDamageNegatedByImmunity(NetworkReader reader)
	{
		return new EventInfoDamageNegatedByImmunity
		{
			actor = reader.ReadNetworkBehaviour<Actor>(),
			effect = reader.ReadBasicEffect(),
			victim = reader.ReadNetworkBehaviour<Entity>(),
			data = _Read_FinalDamageData(reader)
		};
	}

	public static void _Write_EventInfoAttackHit(NetworkWriter writer, EventInfoAttackHit value)
	{
		writer.WriteNetworkBehaviour(value.actor);
		writer.WriteNetworkBehaviour(value.attacker);
		writer.WriteNetworkBehaviour(value.victim);
		writer.WriteBool(value.isCrit);
		writer.WriteFloat(value.strength);
	}

	public static EventInfoAttackHit _Read_EventInfoAttackHit(NetworkReader reader)
	{
		return new EventInfoAttackHit
		{
			actor = reader.ReadNetworkBehaviour<Actor>(),
			attacker = reader.ReadNetworkBehaviour<Entity>(),
			victim = reader.ReadNetworkBehaviour<Entity>(),
			isCrit = reader.ReadBool(),
			strength = reader.ReadFloat()
		};
	}

	public static void _Write_EventInfoApplyElemental(NetworkWriter writer, EventInfoApplyElemental value)
	{
		writer.WriteNetworkBehaviour(value.actor);
		writer.WriteNetworkBehaviour(value.victim);
		_Write_ElementalType(writer, value.type);
		writer.WriteInt(value.addedStack);
	}

	public static void _Write_ElementalType(NetworkWriter writer, ElementalType value)
	{
		writer.WriteInt((int)value);
	}

	public static EventInfoApplyElemental _Read_EventInfoApplyElemental(NetworkReader reader)
	{
		return new EventInfoApplyElemental
		{
			actor = reader.ReadNetworkBehaviour<Actor>(),
			victim = reader.ReadNetworkBehaviour<Entity>(),
			type = _Read_ElementalType(reader),
			addedStack = reader.ReadInt()
		};
	}

	public static ElementalType _Read_ElementalType(NetworkReader reader)
	{
		return (ElementalType)reader.ReadInt();
	}

	public static void _Write_EventInfoCast(NetworkWriter writer, EventInfoCast value)
	{
		writer.WriteNetworkBehaviour(value.instance);
		writer.WriteNetworkBehaviour(value.trigger);
		writer.WriteInt(value.configIndex);
		_Write_CastInfo(writer, value.info);
	}

	public static EventInfoCast _Read_EventInfoCast(NetworkReader reader)
	{
		return new EventInfoCast
		{
			instance = reader.ReadNetworkBehaviour<AbilityInstance>(),
			trigger = reader.ReadNetworkBehaviour<AbilityTrigger>(),
			configIndex = reader.ReadInt(),
			info = _Read_CastInfo(reader)
		};
	}

	public static void _Write_DewConversationSettings(NetworkWriter writer, DewConversationSettings value)
	{
		if (value == null)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		writer.WriteString(value.startConversationKey);
		writer.WriteNetworkBehaviour(value.player);
		_Write_Entity_005B_005D(writer, value.speakers);
		writer.WriteBool(value.rotateTowardsCenter);
		_Write_ConversationVisibility(writer, value.visibility);
		writer.WriteDictionaryStringString(value.variables);
		writer.WriteInt(value._seed);
	}

	public static void _Write_Entity_005B_005D(NetworkWriter writer, Entity[] value)
	{
		writer.WriteArray(value);
	}

	public static void _Write_ConversationVisibility(NetworkWriter writer, ConversationVisibility value)
	{
		writer.WriteInt((int)value);
	}

	public static DewConversationSettings _Read_DewConversationSettings(NetworkReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		DewConversationSettings dewConversationSettings = new DewConversationSettings();
		dewConversationSettings.startConversationKey = reader.ReadString();
		dewConversationSettings.player = reader.ReadNetworkBehaviour<DewPlayer>();
		dewConversationSettings.speakers = _Read_Entity_005B_005D(reader);
		dewConversationSettings.rotateTowardsCenter = reader.ReadBool();
		dewConversationSettings.visibility = _Read_ConversationVisibility(reader);
		dewConversationSettings.variables = reader.ReadDictionaryStringString();
		dewConversationSettings._seed = reader.ReadInt();
		return dewConversationSettings;
	}

	public static Entity[] _Read_Entity_005B_005D(NetworkReader reader)
	{
		return reader.ReadArray<Entity>();
	}

	public static ConversationVisibility _Read_ConversationVisibility(NetworkReader reader)
	{
		return (ConversationVisibility)reader.ReadInt();
	}

	public static void _Write_System_002EInt32_005B_005D(NetworkWriter writer, int[] value)
	{
		writer.WriteArray(value);
	}

	public static int[] _Read_System_002EInt32_005B_005D(NetworkReader reader)
	{
		return reader.ReadArray<int>();
	}

	public static void _Write_DewGameResult(NetworkWriter writer, DewGameResult value)
	{
		if (value == null)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		_Write_DewGameResult_002FResultType(writer, value.result);
		writer.WriteLong(value.startTimestamp);
		writer.WriteInt(value.elapsedGameTimeSeconds);
		writer.WriteInt(value.visitedWorlds);
		writer.WriteInt(value.visitedLocations);
		writer.WriteString(value.difficulty);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FPlayerData_003E(writer, value.players);
	}

	public static void _Write_DewGameResult_002FResultType(NetworkWriter writer, DewGameResult.ResultType value)
	{
		writer.WriteInt((int)value);
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FPlayerData_003E(NetworkWriter writer, List<DewGameResult.PlayerData> value)
	{
		writer.WriteList(value);
	}

	public static void _Write_DewGameResult_002FPlayerData(NetworkWriter writer, DewGameResult.PlayerData value)
	{
		if (value == null)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		writer.WriteBool(value.isLocalPlayer);
		writer.WriteUInt(value.playerNetId);
		writer.WriteString(value.playerProfileName);
		writer.WriteString(value.heroType);
		writer.WriteFloat(value.maxHealth);
		writer.WriteFloat(value.attackDamage);
		writer.WriteFloat(value.abilityPower);
		writer.WriteFloat(value.skillHaste);
		writer.WriteFloat(value.attackSpeed);
		writer.WriteFloat(value.fireAmp);
		writer.WriteFloat(value.critChance);
		writer.WriteInt(value.level);
		writer.WriteInt(value.kills);
		writer.WriteInt(value.heroicBossKills);
		writer.WriteInt(value.miniBossKills);
		writer.WriteInt(value.hunterKills);
		writer.WriteInt(value.totalGoldIncome);
		writer.WriteInt(value.totalDreamDustIncome);
		writer.WriteInt(value.totalStardustIncome);
		writer.WriteInt(value.deaths);
		writer.WriteInt(value.combatTime);
		writer.WriteFloat(value.dealtDamageToEnemies);
		writer.WriteFloat(value.maxDealtSingleDamageToEnemy);
		writer.WriteFloat(value.healToSelf);
		writer.WriteFloat(value.healToOthers);
		writer.WriteFloat(value.receivedDamage);
		writer.WriteString(value.causeOfDeathActor);
		writer.WriteString(value.causeOfDeathEntity);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CPlayerStarItem_003E(writer, value.stars);
		writer.WriteDictionaryStringString(value.capturedStarTooltipFields);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FSkillData_003E(writer, value.skills);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FGemData_003E(writer, value.gems);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EInt32_003E(writer, value.maxGemCounts);
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CPlayerStarItem_003E(NetworkWriter writer, List<PlayerStarItem> value)
	{
		writer.WriteList(value);
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FSkillData_003E(NetworkWriter writer, List<DewGameResult.SkillData> value)
	{
		writer.WriteList(value);
	}

	public static void _Write_DewGameResult_002FSkillData(NetworkWriter writer, DewGameResult.SkillData value)
	{
		writer.WriteUInt(value.netId);
		_Write_HeroSkillLocation(writer, value.loc);
		_Write_SkillType(writer, value.type);
		writer.WriteString(value.name);
		writer.WriteInt(value.level);
		writer.WriteInt(value.maxCharges);
		writer.WriteFloat(value.cooldownTime);
		writer.WriteDictionaryStringString(value.capturedTooltipFields);
	}

	public static void _Write_SkillType(NetworkWriter writer, SkillType value)
	{
		writer.WriteInt((int)value);
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FGemData_003E(NetworkWriter writer, List<DewGameResult.GemData> value)
	{
		writer.WriteList(value);
	}

	public static void _Write_DewGameResult_002FGemData(NetworkWriter writer, DewGameResult.GemData value)
	{
		writer.WriteUInt(value.netId);
		_Write_GemLocation(writer, value.location);
		writer.WriteString(value.name);
		writer.WriteInt(value.quality);
		writer.WriteDictionaryStringString(value.capturedTooltipFields);
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EInt32_003E(NetworkWriter writer, List<int> value)
	{
		writer.WriteList(value);
	}

	public static DewGameResult _Read_DewGameResult(NetworkReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		DewGameResult dewGameResult = new DewGameResult();
		dewGameResult.result = _Read_DewGameResult_002FResultType(reader);
		dewGameResult.startTimestamp = reader.ReadLong();
		dewGameResult.elapsedGameTimeSeconds = reader.ReadInt();
		dewGameResult.visitedWorlds = reader.ReadInt();
		dewGameResult.visitedLocations = reader.ReadInt();
		dewGameResult.difficulty = reader.ReadString();
		dewGameResult.players = _Read_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FPlayerData_003E(reader);
		return dewGameResult;
	}

	public static DewGameResult.ResultType _Read_DewGameResult_002FResultType(NetworkReader reader)
	{
		return (DewGameResult.ResultType)reader.ReadInt();
	}

	public static List<DewGameResult.PlayerData> _Read_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FPlayerData_003E(NetworkReader reader)
	{
		return reader.ReadList<DewGameResult.PlayerData>();
	}

	public static DewGameResult.PlayerData _Read_DewGameResult_002FPlayerData(NetworkReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		DewGameResult.PlayerData playerData = new DewGameResult.PlayerData();
		playerData.isLocalPlayer = reader.ReadBool();
		playerData.playerNetId = reader.ReadUInt();
		playerData.playerProfileName = reader.ReadString();
		playerData.heroType = reader.ReadString();
		playerData.maxHealth = reader.ReadFloat();
		playerData.attackDamage = reader.ReadFloat();
		playerData.abilityPower = reader.ReadFloat();
		playerData.skillHaste = reader.ReadFloat();
		playerData.attackSpeed = reader.ReadFloat();
		playerData.fireAmp = reader.ReadFloat();
		playerData.critChance = reader.ReadFloat();
		playerData.level = reader.ReadInt();
		playerData.kills = reader.ReadInt();
		playerData.heroicBossKills = reader.ReadInt();
		playerData.miniBossKills = reader.ReadInt();
		playerData.hunterKills = reader.ReadInt();
		playerData.totalGoldIncome = reader.ReadInt();
		playerData.totalDreamDustIncome = reader.ReadInt();
		playerData.totalStardustIncome = reader.ReadInt();
		playerData.deaths = reader.ReadInt();
		playerData.combatTime = reader.ReadInt();
		playerData.dealtDamageToEnemies = reader.ReadFloat();
		playerData.maxDealtSingleDamageToEnemy = reader.ReadFloat();
		playerData.healToSelf = reader.ReadFloat();
		playerData.healToOthers = reader.ReadFloat();
		playerData.receivedDamage = reader.ReadFloat();
		playerData.causeOfDeathActor = reader.ReadString();
		playerData.causeOfDeathEntity = reader.ReadString();
		playerData.stars = _Read_System_002ECollections_002EGeneric_002EList_00601_003CPlayerStarItem_003E(reader);
		playerData.capturedStarTooltipFields = reader.ReadDictionaryStringString();
		playerData.skills = _Read_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FSkillData_003E(reader);
		playerData.gems = _Read_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FGemData_003E(reader);
		playerData.maxGemCounts = _Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EInt32_003E(reader);
		return playerData;
	}

	public static List<PlayerStarItem> _Read_System_002ECollections_002EGeneric_002EList_00601_003CPlayerStarItem_003E(NetworkReader reader)
	{
		return reader.ReadList<PlayerStarItem>();
	}

	public static List<DewGameResult.SkillData> _Read_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FSkillData_003E(NetworkReader reader)
	{
		return reader.ReadList<DewGameResult.SkillData>();
	}

	public static DewGameResult.SkillData _Read_DewGameResult_002FSkillData(NetworkReader reader)
	{
		return new DewGameResult.SkillData
		{
			netId = reader.ReadUInt(),
			loc = _Read_HeroSkillLocation(reader),
			type = _Read_SkillType(reader),
			name = reader.ReadString(),
			level = reader.ReadInt(),
			maxCharges = reader.ReadInt(),
			cooldownTime = reader.ReadFloat(),
			capturedTooltipFields = reader.ReadDictionaryStringString()
		};
	}

	public static SkillType _Read_SkillType(NetworkReader reader)
	{
		return (SkillType)reader.ReadInt();
	}

	public static List<DewGameResult.GemData> _Read_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FGemData_003E(NetworkReader reader)
	{
		return reader.ReadList<DewGameResult.GemData>();
	}

	public static DewGameResult.GemData _Read_DewGameResult_002FGemData(NetworkReader reader)
	{
		return new DewGameResult.GemData
		{
			netId = reader.ReadUInt(),
			location = _Read_GemLocation(reader),
			name = reader.ReadString(),
			quality = reader.ReadInt(),
			capturedTooltipFields = reader.ReadDictionaryStringString()
		};
	}

	public static List<int> _Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EInt32_003E(NetworkReader reader)
	{
		return reader.ReadList<int>();
	}

	public static void _Write_PingManager_002FPing(NetworkWriter writer, PingManager.Ping value)
	{
		writer.WriteNetworkBehaviour(value.sender);
		_Write_PingManager_002FPingType(writer, value.type);
		writer.WriteNetworkBehaviour(value.target);
		writer.WriteVector3(value.position);
		writer.WriteInt(value.itemIndex);
	}

	public static void _Write_PingManager_002FPingType(NetworkWriter writer, PingManager.PingType value)
	{
		NetworkWriterExtensions.WriteByte(writer, (byte)value);
	}

	public static PingManager.Ping _Read_PingManager_002FPing(NetworkReader reader)
	{
		return new PingManager.Ping
		{
			sender = reader.ReadNetworkBehaviour<DewPlayer>(),
			type = _Read_PingManager_002FPingType(reader),
			target = reader.ReadNetworkBehaviour(),
			position = reader.ReadVector3(),
			itemIndex = reader.ReadInt()
		};
	}

	public static PingManager.PingType _Read_PingManager_002FPingType(NetworkReader reader)
	{
		return (PingManager.PingType)NetworkReaderExtensions.ReadByte(reader);
	}

	public static void _Write_QuestFailReason(NetworkWriter writer, QuestFailReason value)
	{
		writer.WriteInt((int)value);
	}

	public static QuestFailReason _Read_QuestFailReason(NetworkReader reader)
	{
		return (QuestFailReason)reader.ReadInt();
	}

	public static WorldNodeData _Read_WorldNodeData(NetworkReader reader)
	{
		return new WorldNodeData
		{
			type = _Read_WorldNodeType(reader),
			status = _Read_WorldNodeStatus(reader),
			room = reader.ReadString(),
			roomRotIndex = reader.ReadInt(),
			modifiers = _Read_System_002ECollections_002EGeneric_002EList_00601_003CModifierData_003E(reader),
			position = reader.ReadVector2()
		};
	}

	public static WorldNodeType _Read_WorldNodeType(NetworkReader reader)
	{
		return (WorldNodeType)reader.ReadInt();
	}

	public static WorldNodeStatus _Read_WorldNodeStatus(NetworkReader reader)
	{
		return (WorldNodeStatus)reader.ReadInt();
	}

	public static List<ModifierData> _Read_System_002ECollections_002EGeneric_002EList_00601_003CModifierData_003E(NetworkReader reader)
	{
		return reader.ReadList<ModifierData>();
	}

	public static ModifierData _Read_ModifierData(NetworkReader reader)
	{
		return new ModifierData
		{
			id = reader.ReadInt(),
			type = reader.ReadString(),
			clientData = reader.ReadString()
		};
	}

	public static void _Write_WorldNodeData(NetworkWriter writer, WorldNodeData value)
	{
		_Write_WorldNodeType(writer, value.type);
		_Write_WorldNodeStatus(writer, value.status);
		writer.WriteString(value.room);
		writer.WriteInt(value.roomRotIndex);
		_Write_System_002ECollections_002EGeneric_002EList_00601_003CModifierData_003E(writer, value.modifiers);
		writer.WriteVector2(value.position);
	}

	public static void _Write_WorldNodeType(NetworkWriter writer, WorldNodeType value)
	{
		writer.WriteInt((int)value);
	}

	public static void _Write_WorldNodeStatus(NetworkWriter writer, WorldNodeStatus value)
	{
		writer.WriteInt((int)value);
	}

	public static void _Write_System_002ECollections_002EGeneric_002EList_00601_003CModifierData_003E(NetworkWriter writer, List<ModifierData> value)
	{
		writer.WriteList(value);
	}

	public static void _Write_ModifierData(NetworkWriter writer, ModifierData value)
	{
		writer.WriteInt(value.id);
		writer.WriteString(value.type);
		writer.WriteString(value.clientData);
	}

	public static HunterStatus _Read_HunterStatus(NetworkReader reader)
	{
		return (HunterStatus)reader.ReadInt();
	}

	public static void _Write_HunterStatus(NetworkWriter writer, HunterStatus value)
	{
		writer.WriteInt((int)value);
	}

	public static void _Write_EventInfoLoadRoom(NetworkWriter writer, EventInfoLoadRoom value)
	{
		writer.WriteInt(value.fromIndex);
		writer.WriteInt(value.toIndex);
		writer.WriteBool(value.isSidetrackTransition);
	}

	public static EventInfoLoadRoom _Read_EventInfoLoadRoom(NetworkReader reader)
	{
		return new EventInfoLoadRoom
		{
			fromIndex = reader.ReadInt(),
			toIndex = reader.ReadInt(),
			isSidetrackTransition = reader.ReadBool()
		};
	}

	public static void _Write_VoteType(NetworkWriter writer, VoteType value)
	{
		writer.WriteInt((int)value);
	}

	public static VoteType _Read_VoteType(NetworkReader reader)
	{
		return (VoteType)reader.ReadInt();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void InitReadWriters()
	{
		Writer<byte>.write = NetworkWriterExtensions.WriteByte;
		Writer<byte?>.write = NetworkWriterExtensions.WriteByteNullable;
		Writer<sbyte>.write = NetworkWriterExtensions.WriteSByte;
		Writer<sbyte?>.write = NetworkWriterExtensions.WriteSByteNullable;
		Writer<char>.write = NetworkWriterExtensions.WriteChar;
		Writer<char?>.write = NetworkWriterExtensions.WriteCharNullable;
		Writer<bool>.write = NetworkWriterExtensions.WriteBool;
		Writer<bool?>.write = NetworkWriterExtensions.WriteBoolNullable;
		Writer<short>.write = NetworkWriterExtensions.WriteShort;
		Writer<short?>.write = NetworkWriterExtensions.WriteShortNullable;
		Writer<ushort>.write = NetworkWriterExtensions.WriteUShort;
		Writer<ushort?>.write = NetworkWriterExtensions.WriteUShortNullable;
		Writer<int>.write = NetworkWriterExtensions.WriteInt;
		Writer<int?>.write = NetworkWriterExtensions.WriteIntNullable;
		Writer<uint>.write = NetworkWriterExtensions.WriteUInt;
		Writer<uint?>.write = NetworkWriterExtensions.WriteUIntNullable;
		Writer<long>.write = NetworkWriterExtensions.WriteLong;
		Writer<long?>.write = NetworkWriterExtensions.WriteLongNullable;
		Writer<ulong>.write = NetworkWriterExtensions.WriteULong;
		Writer<ulong?>.write = NetworkWriterExtensions.WriteULongNullable;
		Writer<float>.write = NetworkWriterExtensions.WriteFloat;
		Writer<float?>.write = NetworkReaderWriterExtensions.WriteNullableFloat;
		Writer<double>.write = NetworkWriterExtensions.WriteDouble;
		Writer<double?>.write = NetworkWriterExtensions.WriteDoubleNullable;
		Writer<decimal>.write = NetworkWriterExtensions.WriteDecimal;
		Writer<decimal?>.write = NetworkWriterExtensions.WriteDecimalNullable;
		Writer<string>.write = NetworkWriterExtensions.WriteString;
		Writer<ArraySegment<byte>>.write = NetworkWriterExtensions.WriteBytesAndSizeSegment;
		Writer<byte[]>.write = NetworkWriterExtensions.WriteBytesAndSize;
		Writer<Vector2>.write = NetworkWriterExtensions.WriteVector2;
		Writer<Vector2?>.write = NetworkWriterExtensions.WriteVector2Nullable;
		Writer<Vector3>.write = NetworkWriterExtensions.WriteVector3;
		Writer<Vector3?>.write = NetworkWriterExtensions.WriteVector3Nullable;
		Writer<Vector4>.write = NetworkWriterExtensions.WriteVector4;
		Writer<Vector4?>.write = NetworkWriterExtensions.WriteVector4Nullable;
		Writer<Vector2Int>.write = NetworkWriterExtensions.WriteVector2Int;
		Writer<Vector2Int?>.write = NetworkWriterExtensions.WriteVector2IntNullable;
		Writer<Vector3Int>.write = NetworkWriterExtensions.WriteVector3Int;
		Writer<Vector3Int?>.write = NetworkWriterExtensions.WriteVector3IntNullable;
		Writer<Color>.write = NetworkWriterExtensions.WriteColor;
		Writer<Color?>.write = NetworkWriterExtensions.WriteColorNullable;
		Writer<Color32>.write = NetworkWriterExtensions.WriteColor32;
		Writer<Color32?>.write = NetworkWriterExtensions.WriteColor32Nullable;
		Writer<Quaternion>.write = NetworkWriterExtensions.WriteQuaternion;
		Writer<Quaternion?>.write = NetworkWriterExtensions.WriteQuaternionNullable;
		Writer<Rect>.write = NetworkWriterExtensions.WriteRect;
		Writer<Rect?>.write = NetworkWriterExtensions.WriteRectNullable;
		Writer<Plane>.write = NetworkWriterExtensions.WritePlane;
		Writer<Plane?>.write = NetworkWriterExtensions.WritePlaneNullable;
		Writer<Ray>.write = NetworkWriterExtensions.WriteRay;
		Writer<Ray?>.write = NetworkWriterExtensions.WriteRayNullable;
		Writer<Matrix4x4>.write = NetworkWriterExtensions.WriteMatrix4x4;
		Writer<Matrix4x4?>.write = NetworkWriterExtensions.WriteMatrix4x4Nullable;
		Writer<Guid>.write = NetworkWriterExtensions.WriteGuid;
		Writer<Guid?>.write = NetworkWriterExtensions.WriteGuidNullable;
		Writer<NetworkIdentity>.write = NetworkWriterExtensions.WriteNetworkIdentity;
		Writer<NetworkBehaviour>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<Transform>.write = NetworkWriterExtensions.WriteTransform;
		Writer<GameObject>.write = NetworkWriterExtensions.WriteGameObject;
		Writer<Uri>.write = NetworkWriterExtensions.WriteUri;
		Writer<Texture2D>.write = NetworkWriterExtensions.WriteTexture2D;
		Writer<Sprite>.write = NetworkWriterExtensions.WriteSprite;
		Writer<DateTime>.write = NetworkWriterExtensions.WriteDateTime;
		Writer<DateTime?>.write = NetworkWriterExtensions.WriteDateTimeNullable;
		Writer<TimeSnapshotMessage>.write = _Write_Mirror_002ETimeSnapshotMessage;
		Writer<ReadyMessage>.write = _Write_Mirror_002EReadyMessage;
		Writer<NotReadyMessage>.write = _Write_Mirror_002ENotReadyMessage;
		Writer<AddPlayerMessage>.write = _Write_Mirror_002EAddPlayerMessage;
		Writer<SceneMessage>.write = _Write_Mirror_002ESceneMessage;
		Writer<SceneOperation>.write = _Write_Mirror_002ESceneOperation;
		Writer<CommandMessage>.write = _Write_Mirror_002ECommandMessage;
		Writer<RpcMessage>.write = _Write_Mirror_002ERpcMessage;
		Writer<RpcBufferMessage>.write = _Write_Mirror_002ERpcBufferMessage;
		Writer<SpawnMessage>.write = _Write_Mirror_002ESpawnMessage;
		Writer<ChangeOwnerMessage>.write = _Write_Mirror_002EChangeOwnerMessage;
		Writer<ObjectSpawnStartedMessage>.write = _Write_Mirror_002EObjectSpawnStartedMessage;
		Writer<ObjectSpawnFinishedMessage>.write = _Write_Mirror_002EObjectSpawnFinishedMessage;
		Writer<ObjectDestroyMessage>.write = _Write_Mirror_002EObjectDestroyMessage;
		Writer<ObjectHideMessage>.write = _Write_Mirror_002EObjectHideMessage;
		Writer<EntityStateMessage>.write = _Write_Mirror_002EEntityStateMessage;
		Writer<NetworkPingMessage>.write = _Write_Mirror_002ENetworkPingMessage;
		Writer<NetworkPongMessage>.write = _Write_Mirror_002ENetworkPongMessage;
		Writer<SampleCastInfoContext?>.write = SampleCastInfoContextSerialization.WriteSampleCastInfoContext;
		Writer<CastMethodData>.write = CastMethodDataSerialization.WriteCastMethodData;
		Writer<ElementalType?>.write = ElementalTypeSerialization.WriteNullableElementalType;
		Writer<BasicEffect>.write = NetworkReaderWriterExtensions.WriteBasicEffect;
		Writer<IInteractable>.write = NetworkReaderWriterExtensions.WriteIInteractable;
		Writer<Type>.write = NetworkReaderWriterExtensions.WriteType;
		Writer<DewAnimationClip>.write = NetworkReaderWriterExtensions.WriteDewAnimationClip;
		Writer<DewAudioClip>.write = NetworkReaderWriterExtensions.WriteDewAudioClip;
		Writer<AnimationClip>.write = NetworkReaderWriterExtensions.WriteAnimationClip;
		Writer<DewDifficultySettings>.write = NetworkReaderWriterExtensions.WriteDewDifficultySettings;
		Writer<Zone>.write = NetworkReaderWriterExtensions.WriteZone;
		Writer<Key?>.write = NetworkReaderWriterExtensions.WriteNullableKey;
		Writer<GamepadButton?>.write = NetworkReaderWriterExtensions.WriteNullableGamepadButton;
		Writer<KeyCode?>.write = NetworkReaderWriterExtensions.WriteNullableKeyCode;
		Writer<IItem>.write = NetworkReaderWriterExtensions.WriteIHoldableInHand;
		Writer<Displacement>.write = NetworkReaderWriterExtensions.WriteDisplacement;
		Writer<Dictionary<string, string>>.write = NetworkReaderWriterExtensions.WriteDictionaryStringString;
		Writer<CounterBool>.write = NetworkReaderWriterExtensions.WriteCounterBool;
		Writer<DewEffect.PlayEffectMessage>.write = _Write_DewEffect_002FPlayEffectMessage;
		Writer<DewEffect.PlayPositionedEffectMessage>.write = _Write_DewEffect_002FPlayPositionedEffectMessage;
		Writer<DewEffect.PlayAttachedEffectMessage>.write = _Write_DewEffect_002FPlayAttachedEffectMessage;
		Writer<Entity>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<DewEffect.PlayCastEffectMessage>.write = _Write_DewEffect_002FPlayCastEffectMessage;
		Writer<CastInfo>.write = _Write_CastInfo;
		Writer<CastMethodType>.write = _Write_CastMethodType;
		Writer<DewEffect.StopEffectMessage>.write = _Write_DewEffect_002FStopEffectMessage;
		Writer<DewNetworkManager.ChangeSceneMessage>.write = _Write_DewNetworkManager_002FChangeSceneMessage;
		Writer<DewNetworkManager.SetLoadingStatusMessage>.write = _Write_DewNetworkManager_002FSetLoadingStatusMessage;
		Writer<DewNetworkManager.SessionEndedMessage>.write = _Write_DewNetworkManager_002FSessionEndedMessage;
		Writer<DewNetworkManager.SessionRestartingMessage>.write = _Write_DewNetworkManager_002FSessionRestartingMessage;
		Writer<InGameAnalyticsManager.DisableAnalyticsMessage>.write = _Write_InGameAnalyticsManager_002FDisableAnalyticsMessage;
		Writer<Projectile.EntityHit>.write = _Write_Projectile_002FEntityHit;
		Writer<Projectile.ProjectileMode>.write = _Write_Projectile_002FProjectileMode;
		Writer<EventInfoKill>.write = _Write_EventInfoKill;
		Writer<Actor>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<Monster.MonsterType>.write = _Write_Monster_002FMonsterType;
		Writer<MerchandiseData[]>.write = _Write_MerchandiseData_005B_005D;
		Writer<MerchandiseData>.write = _Write_MerchandiseData;
		Writer<MerchandiseType>.write = _Write_MerchandiseType;
		Writer<Cost>.write = _Write_Cost;
		Writer<AbilityTrigger.ConfigSyncData[]>.write = _Write_AbilityTrigger_002FConfigSyncData_005B_005D;
		Writer<AbilityTrigger.ConfigSyncData>.write = _Write_AbilityTrigger_002FConfigSyncData;
		Writer<QuestProgressType>.write = _Write_QuestProgressType;
		Writer<ChoiceShrineItem>.write = _Write_ChoiceShrineItem;
		Writer<DewPlayer>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<ChaosReward[]>.write = _Write_ChaosReward_005B_005D;
		Writer<ChaosReward>.write = _Write_ChaosReward;
		Writer<ChaosRewardType>.write = _Write_ChaosRewardType;
		Writer<Rarity>.write = _Write_Rarity;
		Writer<HeroLoadoutData>.write = _Write_HeroLoadoutData;
		Writer<List<LoadoutStarItem>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E;
		Writer<LoadoutStarItem>.write = _Write_LoadoutStarItem;
		Writer<PlayerStarItem>.write = _Write_PlayerStarItem;
		Writer<string[]>.write = _Write_System_002EString_005B_005D;
		Writer<InputMode>.write = _Write_InputMode;
		Writer<WorldMessageSetting>.write = _Write_WorldMessageSetting;
		Writer<CenterMessageType>.write = _Write_CenterMessageType;
		Writer<NetworkedOnScreenTimerHandle>.write = _Write_NetworkedOnScreenTimerHandle;
		Writer<SampleCastInfoContext>.write = _Write_SampleCastInfoContext;
		Writer<AbilityTargetValidator>.write = _Write_AbilityTargetValidator;
		Writer<EntityRelation>.write = _Write_EntityRelation;
		Writer<AbilityTrigger>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<SampleCastInfoContext.CastOnButtonType>.write = _Write_SampleCastInfoContext_002FCastOnButtonType;
		Writer<CSteamID>.write = _Write_Steamworks_002ECSteamID;
		Writer<List<string>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E;
		Writer<DewPlayer.Role>.write = _Write_DewPlayer_002FRole;
		Writer<HatredStrengthType>.write = _Write_HatredStrengthType;
		Writer<EventInfoSkillUse>.write = _Write_EventInfoSkillUse;
		Writer<HeroSkillLocation>.write = _Write_HeroSkillLocation;
		Writer<SkillTrigger>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<DescriptionTags>.write = _Write_DescriptionTags;
		Writer<SyncedNetworkBehaviour>.write = _Write_SyncedNetworkBehaviour;
		Writer<EntityAnimation.ReplaceableAnimationType>.write = _Write_EntityAnimation_002FReplaceableAnimationType;
		Writer<EntityControl.BlockableAction>.write = _Write_EntityControl_002FBlockableAction;
		Writer<EntityControl.PositionSyncData>.write = _Write_EntityControl_002FPositionSyncData;
		Writer<StatusEffect>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<EventInfoStatusEffect>.write = _Write_EventInfoStatusEffect;
		Writer<BaseStats>.write = _Write_BaseStats;
		Writer<BonusStats>.write = _Write_BonusStats;
		Writer<BasicEffectMask>.write = _Write_BasicEffectMask;
		Writer<GibInfo>.write = _Write_GibInfo;
		Writer<KnockUpStrength>.write = _Write_KnockUpStrength;
		Writer<Room_Waypoint>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<GemLocation>.write = _Write_GemLocation;
		Writer<Gem>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<ChatManager.Message>.write = _Write_ChatManager_002FMessage;
		Writer<ChatManager.MessageType>.write = _Write_ChatManager_002FMessageType;
		Writer<EventInfoHeal>.write = _Write_EventInfoHeal;
		Writer<FinalHealData>.write = _Write_FinalHealData;
		Writer<ActorFlags>.write = _Write_ActorFlags;
		Writer<ReactionChain>.write = _Write_ReactionChain;
		Writer<List<Actor>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CActor_003E;
		Writer<EventInfoDamage>.write = _Write_EventInfoDamage;
		Writer<FinalDamageData>.write = _Write_FinalDamageData;
		Writer<AttackEffectType>.write = _Write_AttackEffectType;
		Writer<DamageData.SourceType>.write = _Write_DamageData_002FSourceType;
		Writer<DamageAttribute>.write = _Write_DamageAttribute;
		Writer<EventInfoSpentMana>.write = _Write_EventInfoSpentMana;
		Writer<EventInfoAttackMissed>.write = _Write_EventInfoAttackMissed;
		Writer<EventInfoDamageNegatedByImmunity>.write = _Write_EventInfoDamageNegatedByImmunity;
		Writer<EventInfoAttackHit>.write = _Write_EventInfoAttackHit;
		Writer<Hero>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<EventInfoApplyElemental>.write = _Write_EventInfoApplyElemental;
		Writer<ElementalType>.write = _Write_ElementalType;
		Writer<EventInfoCast>.write = _Write_EventInfoCast;
		Writer<AbilityInstance>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<DewConversationSettings>.write = _Write_DewConversationSettings;
		Writer<Entity[]>.write = _Write_Entity_005B_005D;
		Writer<ConversationVisibility>.write = _Write_ConversationVisibility;
		Writer<int[]>.write = _Write_System_002EInt32_005B_005D;
		Writer<DewGameResult>.write = _Write_DewGameResult;
		Writer<DewGameResult.ResultType>.write = _Write_DewGameResult_002FResultType;
		Writer<List<DewGameResult.PlayerData>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FPlayerData_003E;
		Writer<DewGameResult.PlayerData>.write = _Write_DewGameResult_002FPlayerData;
		Writer<List<PlayerStarItem>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CPlayerStarItem_003E;
		Writer<List<DewGameResult.SkillData>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FSkillData_003E;
		Writer<DewGameResult.SkillData>.write = _Write_DewGameResult_002FSkillData;
		Writer<SkillType>.write = _Write_SkillType;
		Writer<List<DewGameResult.GemData>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FGemData_003E;
		Writer<DewGameResult.GemData>.write = _Write_DewGameResult_002FGemData;
		Writer<List<int>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EInt32_003E;
		Writer<PingManager.Ping>.write = _Write_PingManager_002FPing;
		Writer<PingManager.PingType>.write = _Write_PingManager_002FPingType;
		Writer<DewQuest>.write = NetworkWriterExtensions.WriteNetworkBehaviour;
		Writer<QuestFailReason>.write = _Write_QuestFailReason;
		Writer<WorldNodeData>.write = _Write_WorldNodeData;
		Writer<WorldNodeType>.write = _Write_WorldNodeType;
		Writer<WorldNodeStatus>.write = _Write_WorldNodeStatus;
		Writer<List<ModifierData>>.write = _Write_System_002ECollections_002EGeneric_002EList_00601_003CModifierData_003E;
		Writer<ModifierData>.write = _Write_ModifierData;
		Writer<HunterStatus>.write = _Write_HunterStatus;
		Writer<EventInfoLoadRoom>.write = _Write_EventInfoLoadRoom;
		Writer<VoteType>.write = _Write_VoteType;
		Reader<byte>.read = NetworkReaderExtensions.ReadByte;
		Reader<byte?>.read = NetworkReaderExtensions.ReadByteNullable;
		Reader<sbyte>.read = NetworkReaderExtensions.ReadSByte;
		Reader<sbyte?>.read = NetworkReaderExtensions.ReadSByteNullable;
		Reader<char>.read = NetworkReaderExtensions.ReadChar;
		Reader<char?>.read = NetworkReaderExtensions.ReadCharNullable;
		Reader<bool>.read = NetworkReaderExtensions.ReadBool;
		Reader<bool?>.read = NetworkReaderExtensions.ReadBoolNullable;
		Reader<short>.read = NetworkReaderExtensions.ReadShort;
		Reader<short?>.read = NetworkReaderExtensions.ReadShortNullable;
		Reader<ushort>.read = NetworkReaderExtensions.ReadUShort;
		Reader<ushort?>.read = NetworkReaderExtensions.ReadUShortNullable;
		Reader<int>.read = NetworkReaderExtensions.ReadInt;
		Reader<int?>.read = NetworkReaderExtensions.ReadIntNullable;
		Reader<uint>.read = NetworkReaderExtensions.ReadUInt;
		Reader<uint?>.read = NetworkReaderExtensions.ReadUIntNullable;
		Reader<long>.read = NetworkReaderExtensions.ReadLong;
		Reader<long?>.read = NetworkReaderExtensions.ReadLongNullable;
		Reader<ulong>.read = NetworkReaderExtensions.ReadULong;
		Reader<ulong?>.read = NetworkReaderExtensions.ReadULongNullable;
		Reader<float>.read = NetworkReaderExtensions.ReadFloat;
		Reader<float?>.read = NetworkReaderWriterExtensions.ReadNullableFloat;
		Reader<double>.read = NetworkReaderExtensions.ReadDouble;
		Reader<double?>.read = NetworkReaderExtensions.ReadDoubleNullable;
		Reader<decimal>.read = NetworkReaderExtensions.ReadDecimal;
		Reader<decimal?>.read = NetworkReaderExtensions.ReadDecimalNullable;
		Reader<string>.read = NetworkReaderExtensions.ReadString;
		Reader<byte[]>.read = NetworkReaderExtensions.ReadBytesAndSize;
		Reader<ArraySegment<byte>>.read = NetworkReaderExtensions.ReadBytesAndSizeSegment;
		Reader<Vector2>.read = NetworkReaderExtensions.ReadVector2;
		Reader<Vector2?>.read = NetworkReaderExtensions.ReadVector2Nullable;
		Reader<Vector3>.read = NetworkReaderExtensions.ReadVector3;
		Reader<Vector3?>.read = NetworkReaderExtensions.ReadVector3Nullable;
		Reader<Vector4>.read = NetworkReaderExtensions.ReadVector4;
		Reader<Vector4?>.read = NetworkReaderExtensions.ReadVector4Nullable;
		Reader<Vector2Int>.read = NetworkReaderExtensions.ReadVector2Int;
		Reader<Vector2Int?>.read = NetworkReaderExtensions.ReadVector2IntNullable;
		Reader<Vector3Int>.read = NetworkReaderExtensions.ReadVector3Int;
		Reader<Vector3Int?>.read = NetworkReaderExtensions.ReadVector3IntNullable;
		Reader<Color>.read = NetworkReaderExtensions.ReadColor;
		Reader<Color?>.read = NetworkReaderExtensions.ReadColorNullable;
		Reader<Color32>.read = NetworkReaderExtensions.ReadColor32;
		Reader<Color32?>.read = NetworkReaderExtensions.ReadColor32Nullable;
		Reader<Quaternion>.read = NetworkReaderExtensions.ReadQuaternion;
		Reader<Quaternion?>.read = NetworkReaderExtensions.ReadQuaternionNullable;
		Reader<Rect>.read = NetworkReaderExtensions.ReadRect;
		Reader<Rect?>.read = NetworkReaderExtensions.ReadRectNullable;
		Reader<Plane>.read = NetworkReaderExtensions.ReadPlane;
		Reader<Plane?>.read = NetworkReaderExtensions.ReadPlaneNullable;
		Reader<Ray>.read = NetworkReaderExtensions.ReadRay;
		Reader<Ray?>.read = NetworkReaderExtensions.ReadRayNullable;
		Reader<Matrix4x4>.read = NetworkReaderExtensions.ReadMatrix4x4;
		Reader<Matrix4x4?>.read = NetworkReaderExtensions.ReadMatrix4x4Nullable;
		Reader<Guid>.read = NetworkReaderExtensions.ReadGuid;
		Reader<Guid?>.read = NetworkReaderExtensions.ReadGuidNullable;
		Reader<NetworkIdentity>.read = NetworkReaderExtensions.ReadNetworkIdentity;
		Reader<NetworkBehaviour>.read = NetworkReaderExtensions.ReadNetworkBehaviour;
		Reader<NetworkBehaviourSyncVar>.read = NetworkReaderExtensions.ReadNetworkBehaviourSyncVar;
		Reader<Transform>.read = NetworkReaderExtensions.ReadTransform;
		Reader<GameObject>.read = NetworkReaderExtensions.ReadGameObject;
		Reader<Uri>.read = NetworkReaderExtensions.ReadUri;
		Reader<Texture2D>.read = NetworkReaderExtensions.ReadTexture2D;
		Reader<Sprite>.read = NetworkReaderExtensions.ReadSprite;
		Reader<DateTime>.read = NetworkReaderExtensions.ReadDateTime;
		Reader<DateTime?>.read = NetworkReaderExtensions.ReadDateTimeNullable;
		Reader<TimeSnapshotMessage>.read = _Read_Mirror_002ETimeSnapshotMessage;
		Reader<ReadyMessage>.read = _Read_Mirror_002EReadyMessage;
		Reader<NotReadyMessage>.read = _Read_Mirror_002ENotReadyMessage;
		Reader<AddPlayerMessage>.read = _Read_Mirror_002EAddPlayerMessage;
		Reader<SceneMessage>.read = _Read_Mirror_002ESceneMessage;
		Reader<SceneOperation>.read = _Read_Mirror_002ESceneOperation;
		Reader<CommandMessage>.read = _Read_Mirror_002ECommandMessage;
		Reader<RpcMessage>.read = _Read_Mirror_002ERpcMessage;
		Reader<RpcBufferMessage>.read = _Read_Mirror_002ERpcBufferMessage;
		Reader<SpawnMessage>.read = _Read_Mirror_002ESpawnMessage;
		Reader<ChangeOwnerMessage>.read = _Read_Mirror_002EChangeOwnerMessage;
		Reader<ObjectSpawnStartedMessage>.read = _Read_Mirror_002EObjectSpawnStartedMessage;
		Reader<ObjectSpawnFinishedMessage>.read = _Read_Mirror_002EObjectSpawnFinishedMessage;
		Reader<ObjectDestroyMessage>.read = _Read_Mirror_002EObjectDestroyMessage;
		Reader<ObjectHideMessage>.read = _Read_Mirror_002EObjectHideMessage;
		Reader<EntityStateMessage>.read = _Read_Mirror_002EEntityStateMessage;
		Reader<NetworkPingMessage>.read = _Read_Mirror_002ENetworkPingMessage;
		Reader<NetworkPongMessage>.read = _Read_Mirror_002ENetworkPongMessage;
		Reader<SampleCastInfoContext?>.read = SampleCastInfoContextSerialization.ReadSampleCastInfoContext;
		Reader<CastMethodData>.read = CastMethodDataSerialization.ReadWriteCastMethodData;
		Reader<ElementalType?>.read = ElementalTypeSerialization.ReadNullableElementalType;
		Reader<BasicEffect>.read = NetworkReaderWriterExtensions.ReadBasicEffect;
		Reader<IInteractable>.read = NetworkReaderWriterExtensions.ReadIInteractable;
		Reader<Type>.read = NetworkReaderWriterExtensions.ReadType;
		Reader<DewAnimationClip>.read = NetworkReaderWriterExtensions.ReadDewAnimationClip;
		Reader<DewAudioClip>.read = NetworkReaderWriterExtensions.ReadDewAudioClip;
		Reader<AnimationClip>.read = NetworkReaderWriterExtensions.ReadAnimationClip;
		Reader<DewDifficultySettings>.read = NetworkReaderWriterExtensions.ReadDewDifficultySettings;
		Reader<Zone>.read = NetworkReaderWriterExtensions.ReadZone;
		Reader<Key?>.read = NetworkReaderWriterExtensions.ReadNullableKey;
		Reader<GamepadButton?>.read = NetworkReaderWriterExtensions.ReadNullableGamepadButton;
		Reader<KeyCode?>.read = NetworkReaderWriterExtensions.ReadNullableKeyCode;
		Reader<IItem>.read = NetworkReaderWriterExtensions.ReadIHoldableInHand;
		Reader<Displacement>.read = NetworkReaderWriterExtensions.ReadDisplacement;
		Reader<Dictionary<string, string>>.read = NetworkReaderWriterExtensions.ReadDictionaryStringString;
		Reader<CounterBool>.read = NetworkReaderWriterExtensions.ReadCounterBool;
		Reader<DewEffect.PlayEffectMessage>.read = _Read_DewEffect_002FPlayEffectMessage;
		Reader<DewEffect.PlayPositionedEffectMessage>.read = _Read_DewEffect_002FPlayPositionedEffectMessage;
		Reader<DewEffect.PlayAttachedEffectMessage>.read = _Read_DewEffect_002FPlayAttachedEffectMessage;
		Reader<Entity>.read = NetworkReaderExtensions.ReadNetworkBehaviour<Entity>;
		Reader<DewEffect.PlayCastEffectMessage>.read = _Read_DewEffect_002FPlayCastEffectMessage;
		Reader<CastInfo>.read = _Read_CastInfo;
		Reader<CastMethodType>.read = _Read_CastMethodType;
		Reader<DewEffect.StopEffectMessage>.read = _Read_DewEffect_002FStopEffectMessage;
		Reader<DewNetworkManager.ChangeSceneMessage>.read = _Read_DewNetworkManager_002FChangeSceneMessage;
		Reader<DewNetworkManager.SetLoadingStatusMessage>.read = _Read_DewNetworkManager_002FSetLoadingStatusMessage;
		Reader<DewNetworkManager.SessionEndedMessage>.read = _Read_DewNetworkManager_002FSessionEndedMessage;
		Reader<DewNetworkManager.SessionRestartingMessage>.read = _Read_DewNetworkManager_002FSessionRestartingMessage;
		Reader<InGameAnalyticsManager.DisableAnalyticsMessage>.read = _Read_InGameAnalyticsManager_002FDisableAnalyticsMessage;
		Reader<Projectile.EntityHit>.read = _Read_Projectile_002FEntityHit;
		Reader<Projectile.ProjectileMode>.read = _Read_Projectile_002FProjectileMode;
		Reader<EventInfoKill>.read = _Read_EventInfoKill;
		Reader<Actor>.read = NetworkReaderExtensions.ReadNetworkBehaviour<Actor>;
		Reader<Monster.MonsterType>.read = _Read_Monster_002FMonsterType;
		Reader<MerchandiseData[]>.read = _Read_MerchandiseData_005B_005D;
		Reader<MerchandiseData>.read = _Read_MerchandiseData;
		Reader<MerchandiseType>.read = _Read_MerchandiseType;
		Reader<Cost>.read = _Read_Cost;
		Reader<AbilityTrigger.ConfigSyncData[]>.read = _Read_AbilityTrigger_002FConfigSyncData_005B_005D;
		Reader<AbilityTrigger.ConfigSyncData>.read = _Read_AbilityTrigger_002FConfigSyncData;
		Reader<QuestProgressType>.read = _Read_QuestProgressType;
		Reader<ChoiceShrineItem>.read = _Read_ChoiceShrineItem;
		Reader<DewPlayer>.read = NetworkReaderExtensions.ReadNetworkBehaviour<DewPlayer>;
		Reader<ChaosReward[]>.read = _Read_ChaosReward_005B_005D;
		Reader<ChaosReward>.read = _Read_ChaosReward;
		Reader<ChaosRewardType>.read = _Read_ChaosRewardType;
		Reader<Rarity>.read = _Read_Rarity;
		Reader<HeroLoadoutData>.read = _Read_HeroLoadoutData;
		Reader<List<LoadoutStarItem>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CLoadoutStarItem_003E;
		Reader<LoadoutStarItem>.read = _Read_LoadoutStarItem;
		Reader<PlayerStarItem>.read = _Read_PlayerStarItem;
		Reader<string[]>.read = _Read_System_002EString_005B_005D;
		Reader<InputMode>.read = _Read_InputMode;
		Reader<WorldMessageSetting>.read = _Read_WorldMessageSetting;
		Reader<CenterMessageType>.read = _Read_CenterMessageType;
		Reader<NetworkedOnScreenTimerHandle>.read = _Read_NetworkedOnScreenTimerHandle;
		Reader<SampleCastInfoContext>.read = _Read_SampleCastInfoContext;
		Reader<AbilityTargetValidator>.read = _Read_AbilityTargetValidator;
		Reader<EntityRelation>.read = _Read_EntityRelation;
		Reader<AbilityTrigger>.read = NetworkReaderExtensions.ReadNetworkBehaviour<AbilityTrigger>;
		Reader<SampleCastInfoContext.CastOnButtonType>.read = _Read_SampleCastInfoContext_002FCastOnButtonType;
		Reader<CSteamID>.read = _Read_Steamworks_002ECSteamID;
		Reader<List<string>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E;
		Reader<DewPlayer.Role>.read = _Read_DewPlayer_002FRole;
		Reader<HatredStrengthType>.read = _Read_HatredStrengthType;
		Reader<EventInfoSkillUse>.read = _Read_EventInfoSkillUse;
		Reader<HeroSkillLocation>.read = _Read_HeroSkillLocation;
		Reader<SkillTrigger>.read = NetworkReaderExtensions.ReadNetworkBehaviour<SkillTrigger>;
		Reader<DescriptionTags>.read = _Read_DescriptionTags;
		Reader<SyncedNetworkBehaviour>.read = _Read_SyncedNetworkBehaviour;
		Reader<EntityAnimation.ReplaceableAnimationType>.read = _Read_EntityAnimation_002FReplaceableAnimationType;
		Reader<EntityControl.BlockableAction>.read = _Read_EntityControl_002FBlockableAction;
		Reader<EntityControl.PositionSyncData>.read = _Read_EntityControl_002FPositionSyncData;
		Reader<StatusEffect>.read = NetworkReaderExtensions.ReadNetworkBehaviour<StatusEffect>;
		Reader<EventInfoStatusEffect>.read = _Read_EventInfoStatusEffect;
		Reader<BaseStats>.read = _Read_BaseStats;
		Reader<BonusStats>.read = _Read_BonusStats;
		Reader<BasicEffectMask>.read = _Read_BasicEffectMask;
		Reader<GibInfo>.read = _Read_GibInfo;
		Reader<KnockUpStrength>.read = _Read_KnockUpStrength;
		Reader<Room_Waypoint>.read = NetworkReaderExtensions.ReadNetworkBehaviour<Room_Waypoint>;
		Reader<GemLocation>.read = _Read_GemLocation;
		Reader<Gem>.read = NetworkReaderExtensions.ReadNetworkBehaviour<Gem>;
		Reader<ChatManager.Message>.read = _Read_ChatManager_002FMessage;
		Reader<ChatManager.MessageType>.read = _Read_ChatManager_002FMessageType;
		Reader<EventInfoHeal>.read = _Read_EventInfoHeal;
		Reader<FinalHealData>.read = _Read_FinalHealData;
		Reader<ActorFlags>.read = _Read_ActorFlags;
		Reader<ReactionChain>.read = _Read_ReactionChain;
		Reader<List<Actor>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CActor_003E;
		Reader<EventInfoDamage>.read = _Read_EventInfoDamage;
		Reader<FinalDamageData>.read = _Read_FinalDamageData;
		Reader<AttackEffectType>.read = _Read_AttackEffectType;
		Reader<DamageData.SourceType>.read = _Read_DamageData_002FSourceType;
		Reader<DamageAttribute>.read = _Read_DamageAttribute;
		Reader<EventInfoSpentMana>.read = _Read_EventInfoSpentMana;
		Reader<EventInfoAttackMissed>.read = _Read_EventInfoAttackMissed;
		Reader<EventInfoDamageNegatedByImmunity>.read = _Read_EventInfoDamageNegatedByImmunity;
		Reader<EventInfoAttackHit>.read = _Read_EventInfoAttackHit;
		Reader<Hero>.read = NetworkReaderExtensions.ReadNetworkBehaviour<Hero>;
		Reader<EventInfoApplyElemental>.read = _Read_EventInfoApplyElemental;
		Reader<ElementalType>.read = _Read_ElementalType;
		Reader<EventInfoCast>.read = _Read_EventInfoCast;
		Reader<AbilityInstance>.read = NetworkReaderExtensions.ReadNetworkBehaviour<AbilityInstance>;
		Reader<DewConversationSettings>.read = _Read_DewConversationSettings;
		Reader<Entity[]>.read = _Read_Entity_005B_005D;
		Reader<ConversationVisibility>.read = _Read_ConversationVisibility;
		Reader<int[]>.read = _Read_System_002EInt32_005B_005D;
		Reader<DewGameResult>.read = _Read_DewGameResult;
		Reader<DewGameResult.ResultType>.read = _Read_DewGameResult_002FResultType;
		Reader<List<DewGameResult.PlayerData>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FPlayerData_003E;
		Reader<DewGameResult.PlayerData>.read = _Read_DewGameResult_002FPlayerData;
		Reader<List<PlayerStarItem>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CPlayerStarItem_003E;
		Reader<List<DewGameResult.SkillData>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FSkillData_003E;
		Reader<DewGameResult.SkillData>.read = _Read_DewGameResult_002FSkillData;
		Reader<SkillType>.read = _Read_SkillType;
		Reader<List<DewGameResult.GemData>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CDewGameResult_002FGemData_003E;
		Reader<DewGameResult.GemData>.read = _Read_DewGameResult_002FGemData;
		Reader<List<int>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EInt32_003E;
		Reader<PingManager.Ping>.read = _Read_PingManager_002FPing;
		Reader<PingManager.PingType>.read = _Read_PingManager_002FPingType;
		Reader<DewQuest>.read = NetworkReaderExtensions.ReadNetworkBehaviour<DewQuest>;
		Reader<QuestFailReason>.read = _Read_QuestFailReason;
		Reader<WorldNodeData>.read = _Read_WorldNodeData;
		Reader<WorldNodeType>.read = _Read_WorldNodeType;
		Reader<WorldNodeStatus>.read = _Read_WorldNodeStatus;
		Reader<List<ModifierData>>.read = _Read_System_002ECollections_002EGeneric_002EList_00601_003CModifierData_003E;
		Reader<ModifierData>.read = _Read_ModifierData;
		Reader<HunterStatus>.read = _Read_HunterStatus;
		Reader<EventInfoLoadRoom>.read = _Read_EventInfoLoadRoom;
		Reader<VoteType>.read = _Read_VoteType;
	}
}
