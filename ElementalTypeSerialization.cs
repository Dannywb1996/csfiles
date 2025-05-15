using Mirror;

public static class ElementalTypeSerialization
{
	public static void WriteNullableElementalType(this NetworkWriter writer, ElementalType? value)
	{
		writer.WriteByte(value.HasValue ? ((byte)value.Value) : byte.MaxValue);
	}

	public static ElementalType? ReadNullableElementalType(this NetworkReader reader)
	{
		byte b = reader.ReadByte();
		if (b == byte.MaxValue)
		{
			return null;
		}
		return (ElementalType)b;
	}
}
