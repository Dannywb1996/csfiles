using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[Serializable]
public class PCBind : ICloneable
{
	public List<Key> modifiers = new List<Key>();

	public Key key;

	public MouseButton mouse;

	public object Clone()
	{
		PCBind obj = (PCBind)MemberwiseClone();
		obj.modifiers = new List<Key>(modifiers);
		return obj;
	}

	public PCBind()
	{
	}

	public PCBind(Key key, params Key[] modifiers)
	{
		this.key = key;
		this.modifiers.AddRange(modifiers);
	}

	public PCBind(MouseButton mouse, params Key[] modifiers)
	{
		this.mouse = mouse;
		this.modifiers.AddRange(modifiers);
	}

	public static implicit operator PCBind(Key key)
	{
		return new PCBind
		{
			key = key
		};
	}

	public static implicit operator PCBind(MouseButton mouse)
	{
		return new PCBind
		{
			mouse = mouse
		};
	}
}
