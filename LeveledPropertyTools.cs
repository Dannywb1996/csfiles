using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

public static class LeveledPropertyTools
{
	public static void SetPropertyOrFieldValue(object target, string name, string value)
	{
		Type type = target.GetType();
		PropertyInfo property = type.GetProperty(name);
		FieldInfo field = type.GetField(name);
		if (property == null && field == null)
		{
			throw new Exception($"Field or property with name '{name}' not found on '{target}'");
		}
		if (property != null && field != null)
		{
			throw new Exception("How is this even possible lmao");
		}
		object parsedValue = TypeDescriptor.GetConverter((property != null) ? property.PropertyType : field.FieldType).ConvertFromString(value);
		if (property != null)
		{
			property.SetValue(target, parsedValue);
		}
		else
		{
			field.SetValue(target, parsedValue);
		}
	}

	public static float[] ParseFloatValues(string values)
	{
		string[] split = values.Split('/');
		float[] array = new float[split.Length];
		for (int i = 0; i < split.Length; i++)
		{
			array[i] = float.Parse(split[i], CultureInfo.InvariantCulture);
		}
		return array;
	}

	public static int[] ParseIntValues(string values)
	{
		string[] split = values.Split('/');
		int[] array = new int[split.Length];
		for (int i = 0; i < split.Length; i++)
		{
			array[i] = int.Parse(split[i], CultureInfo.InvariantCulture);
		}
		return array;
	}
}
