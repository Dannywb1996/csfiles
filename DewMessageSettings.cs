using System;
using UnityEngine;

public class DewMessageSettings
{
	[Flags]
	public enum ButtonType
	{
		Ok = 1,
		Yes = 2,
		No = 4,
		Cancel = 8
	}

	public UnityEngine.Object owner;

	public string rawContent;

	public ButtonType buttons = ButtonType.Ok;

	public Action<ButtonType> onClose;

	public bool destructiveConfirm;

	public ButtonType defaultButton = ButtonType.Cancel;

	public Func<bool> validator;

	internal bool IsValid()
	{
		if ((object)owner == null || owner != null)
		{
			if (validator != null)
			{
				return validator();
			}
			return true;
		}
		return false;
	}
}
