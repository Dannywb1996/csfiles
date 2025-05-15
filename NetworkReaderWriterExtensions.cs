using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class NetworkReaderWriterExtensions
{
	private static class CommonReaderWriters
	{
		public static void WriteAsset<T>(NetworkWriter writer, T asset) where T : UnityEngine.Object
		{
			if (asset == null)
			{
				writer.WriteString("");
			}
			else
			{
				writer.WriteString(DewResources.GetGuidOfAsset(asset));
			}
		}

		public static T ReadAsset<T>(NetworkReader reader) where T : UnityEngine.Object
		{
			string guid = reader.ReadString();
			if (string.IsNullOrWhiteSpace(guid))
			{
				return null;
			}
			return DewResources.GetByGuid<T>(guid);
		}
	}

	public static void WriteBasicEffect(this NetworkWriter writer, BasicEffect eff)
	{
	}

	public static BasicEffect ReadBasicEffect(this NetworkReader reader)
	{
		return null;
	}

	public static void WriteIInteractable(this NetworkWriter writer, IInteractable interactable)
	{
		if (!(interactable is Component c))
		{
			writer.WriteNetworkIdentity(null);
		}
		else
		{
			writer.WriteNetworkIdentity(c.GetComponent<NetworkIdentity>());
		}
	}

	public static IInteractable ReadIInteractable(this NetworkReader reader)
	{
		NetworkIdentity netIdentity = reader.ReadNetworkIdentity();
		if (netIdentity == null)
		{
			return null;
		}
		return netIdentity.GetComponent<IInteractable>();
	}

	public static void WriteType(this NetworkWriter writer, Type type)
	{
		writer.WriteString(type.AssemblyQualifiedName);
	}

	public static Type ReadType(this NetworkReader reader)
	{
		return Type.GetType(reader.ReadString());
	}

	public static void WriteDewAnimationClip(this NetworkWriter writer, DewAnimationClip asset)
	{
		CommonReaderWriters.WriteAsset(writer, asset);
	}

	public static DewAnimationClip ReadDewAnimationClip(this NetworkReader reader)
	{
		return CommonReaderWriters.ReadAsset<DewAnimationClip>(reader);
	}

	public static void WriteDewAudioClip(this NetworkWriter writer, DewAudioClip asset)
	{
		CommonReaderWriters.WriteAsset(writer, asset);
	}

	public static DewAudioClip ReadDewAudioClip(this NetworkReader reader)
	{
		return CommonReaderWriters.ReadAsset<DewAudioClip>(reader);
	}

	public static void WriteAnimationClip(this NetworkWriter writer, AnimationClip asset)
	{
		CommonReaderWriters.WriteAsset(writer, asset);
	}

	public static AnimationClip ReadAnimationClip(this NetworkReader reader)
	{
		return CommonReaderWriters.ReadAsset<AnimationClip>(reader);
	}

	public static void WriteDewDifficultySettings(this NetworkWriter writer, DewDifficultySettings asset)
	{
		CommonReaderWriters.WriteAsset(writer, asset);
	}

	public static DewDifficultySettings ReadDewDifficultySettings(this NetworkReader reader)
	{
		return CommonReaderWriters.ReadAsset<DewDifficultySettings>(reader);
	}

	public static void WriteZone(this NetworkWriter writer, Zone asset)
	{
		CommonReaderWriters.WriteAsset(writer, asset);
	}

	public static Zone ReadZone(this NetworkReader reader)
	{
		return CommonReaderWriters.ReadAsset<Zone>(reader);
	}

	public static void WriteNullableFloat(this NetworkWriter writer, float? nullable)
	{
		if (!nullable.HasValue)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		writer.WriteFloat(nullable.Value);
	}

	public static float? ReadNullableFloat(this NetworkReader reader)
	{
		if (reader.ReadBool())
		{
			return reader.ReadFloat();
		}
		return null;
	}

	public static void WriteNullableKey(this NetworkWriter writer, Key? nullable)
	{
		if (!nullable.HasValue)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		writer.WriteInt((int)nullable.Value);
	}

	public static Key? ReadNullableKey(this NetworkReader reader)
	{
		if (reader.ReadBool())
		{
			return (Key)reader.ReadInt();
		}
		return null;
	}

	public static void WriteNullableGamepadButton(this NetworkWriter writer, GamepadButton? nullable)
	{
		if (!nullable.HasValue)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		writer.WriteInt((int)nullable.Value);
	}

	public static GamepadButton? ReadNullableGamepadButton(this NetworkReader reader)
	{
		if (reader.ReadBool())
		{
			return (GamepadButton)reader.ReadInt();
		}
		return null;
	}

	public static void WriteNullableKeyCode(this NetworkWriter writer, KeyCode? nullable)
	{
		if (!nullable.HasValue)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		writer.Write(nullable.Value);
	}

	public static KeyCode? ReadNullableKeyCode(this NetworkReader reader)
	{
		if (reader.ReadBool())
		{
			return reader.Read<KeyCode>();
		}
		return null;
	}

	public static void WriteIHoldableInHand(this NetworkWriter writer, IItem ownable)
	{
		writer.WriteNetworkBehaviour((NetworkBehaviour)ownable);
	}

	public static IItem ReadIHoldableInHand(this NetworkReader reader)
	{
		return reader.ReadNetworkBehaviour() as IItem;
	}

	public static void WriteDisplacement(this NetworkWriter writer, Displacement d)
	{
		if (d == null)
		{
			writer.WriteByte(0);
			return;
		}
		if (d is DispByTarget dt)
		{
			writer.WriteByte(1);
			writer.WriteNetworkBehaviour(dt.target);
			writer.WriteFloat(dt.goalDistance);
			writer.WriteFloat(dt.speed);
			writer.WriteFloat(dt.cancelTime);
			writer.WriteBool(d.isFriendly);
			writer.WriteBool(d.isCanceledByCC);
			writer.WriteBool(d.affectedByMovementSpeed);
			writer.WriteBool(d.rotateForward);
			writer.WriteBool(d.rotateSmoothly);
			return;
		}
		if (d is DispByDestination dd)
		{
			writer.WriteByte(2);
			writer.WriteByte((byte)dd.ease);
			writer.WriteVector3(dd.destination);
			writer.WriteFloat(dd.duration);
			writer.WriteBool(dd.canGoOverTerrain);
			writer.WriteBool(d.isFriendly);
			writer.WriteBool(d.isCanceledByCC);
			writer.WriteBool(d.affectedByMovementSpeed);
			writer.WriteBool(d.rotateForward);
			writer.WriteBool(d.rotateSmoothly);
			return;
		}
		throw new ArgumentOutOfRangeException("d");
	}

	public static Displacement ReadDisplacement(this NetworkReader reader)
	{
		return reader.ReadByte() switch
		{
			0 => null, 
			1 => new DispByTarget
			{
				target = (Entity)reader.ReadNetworkBehaviour(),
				goalDistance = reader.ReadFloat(),
				speed = reader.ReadFloat(),
				cancelTime = reader.ReadFloat(),
				isFriendly = reader.ReadBool(),
				isCanceledByCC = reader.ReadBool(),
				affectedByMovementSpeed = reader.ReadBool(),
				rotateForward = reader.ReadBool(),
				rotateSmoothly = reader.ReadBool()
			}, 
			2 => new DispByDestination
			{
				ease = (DewEase)reader.ReadByte(),
				destination = reader.ReadVector3(),
				duration = reader.ReadFloat(),
				canGoOverTerrain = reader.ReadBool(),
				isFriendly = reader.ReadBool(),
				isCanceledByCC = reader.ReadBool(),
				affectedByMovementSpeed = reader.ReadBool(),
				rotateForward = reader.ReadBool(),
				rotateSmoothly = reader.ReadBool()
			}, 
			_ => throw new ArgumentOutOfRangeException("type"), 
		};
	}

	public static void WriteDictionaryStringString(this NetworkWriter writer, Dictionary<string, string> dict)
	{
		if (dict == null)
		{
			writer.WriteInt(-1);
			return;
		}
		writer.WriteInt(dict.Count);
		foreach (KeyValuePair<string, string> p in dict)
		{
			writer.WriteString(p.Key);
			writer.WriteString(p.Value);
		}
	}

	public static Dictionary<string, string> ReadDictionaryStringString(this NetworkReader reader)
	{
		int count = reader.ReadInt();
		if (count < 0)
		{
			return null;
		}
		Dictionary<string, string> dict = new Dictionary<string, string>();
		for (int i = 0; i < count; i++)
		{
			dict.Add(reader.ReadString(), reader.ReadString());
		}
		return dict;
	}

	public static void WriteCounterBool(this NetworkWriter writer, CounterBool value)
	{
		writer.WriteInt(value.count);
	}

	public static CounterBool ReadCounterBool(this NetworkReader reader)
	{
		return new CounterBool(reader.ReadInt());
	}
}
