using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class DewInputReadableTextExtensions
{
	public static string GetReadableText(this Key k)
	{
		if (k == Key.None)
		{
			return DewLocalization.GetUIValue("Key_None");
		}
		if (DewInput.KeyReadableTextBindings.TryGetValue(k, out var val))
		{
			return val;
		}
		return k.ToString();
	}

	public static string GetReadableText(this GamepadButton? b)
	{
		if (!b.HasValue)
		{
			return DewLocalization.GetUIValue("Key_None");
		}
		return b.Value.GetReadableText();
	}

	public static string GetReadableText(this GamepadButton b)
	{
		if (DewInput.GamepadButtonReadableTextBindings.TryGetValue((GamepadButtonEx)b, out var val))
		{
			return val;
		}
		return b.ToString();
	}

	public static string GetReadableText(this GamepadButtonEx? b)
	{
		if (!b.HasValue)
		{
			return DewLocalization.GetUIValue("Key_None");
		}
		return b.Value.GetReadableText();
	}

	public static string GetReadableText(this GamepadButtonEx b)
	{
		if (DewInput.GamepadButtonReadableTextBindings.TryGetValue(b, out var val))
		{
			return val;
		}
		return b.ToString();
	}

	public static string GetReadableText(this MouseButton b)
	{
		if (b == MouseButton.None)
		{
			return DewLocalization.GetUIValue("Key_None");
		}
		if (DewInput.MouseButtonReadableTextBindings.TryGetValue(b, out var val))
		{
			return val;
		}
		return b.ToString();
	}
}
