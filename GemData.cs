using System;
using System.Collections.Generic;

namespace DewInternal;

[Serializable]
public class GemData
{
	public string name;

	public string template;

	public List<LocaleNode> description;

	public string shortDescription;

	public string memory;
}
