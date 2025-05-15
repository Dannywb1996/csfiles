using Mirror;

public static class CastMethodDataSerialization
{
	public static void WriteCastMethodData(this NetworkWriter writer, CastMethodData value)
	{
		writer.Write((byte)value.type);
		writer.Write(value._angle);
		writer.Write(value._length);
		writer.Write(value._width);
		writer.Write(value._range);
		writer.Write(value._radius);
		writer.Write(value._isClamping);
	}

	public static CastMethodData ReadWriteCastMethodData(this NetworkReader reader)
	{
		return new CastMethodData
		{
			type = (CastMethodType)reader.ReadByte(),
			_angle = reader.ReadFloat(),
			_length = reader.ReadFloat(),
			_width = reader.ReadFloat(),
			_range = reader.ReadFloat(),
			_radius = reader.ReadFloat(),
			_isClamping = reader.ReadBool()
		};
	}
}
