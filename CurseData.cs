using System;
using System.Collections.Generic;

namespace DewInternal;

[Serializable]
public class CurseData
{
	public string name;

	public List<LocaleNode> description;

	public List<LocaleNode> shortDesc;
}
