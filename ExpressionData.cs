using System;
using System.Collections.Generic;

namespace DewInternal;

[Serializable]
public class ExpressionData
{
	public List<ExpressionChildNode> nodes;

	public string format;

	public string raw;
}
