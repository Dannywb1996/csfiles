using Mirror;

public static class SampleCastInfoContextSerialization
{
	public static void WriteSampleCastInfoContext(this NetworkWriter writer, SampleCastInfoContext? value)
	{
		if (!value.HasValue)
		{
			writer.WriteBool(value: false);
			return;
		}
		writer.WriteBool(value: true);
		writer.Write(value.Value);
	}

	public static SampleCastInfoContext? ReadSampleCastInfoContext(this NetworkReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return reader.Read<SampleCastInfoContext>();
	}
}
