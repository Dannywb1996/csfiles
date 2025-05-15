using System;

[Serializable]
public struct ModifierData
{
	public int id;

	public string type;

	public string clientData;

	public ModifierData(string type)
	{
		id = 0;
		this.type = type;
		clientData = "";
	}
}
