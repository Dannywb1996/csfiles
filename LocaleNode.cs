using System;

namespace DewInternal;

[Serializable]
public class LocaleNode
{
	public static readonly string[] ValidTags = new string[4] { "equipped", "/equipped", "ingame", "/ingame" };

	public LocaleNodeType type;

	public string textData;

	public ExpressionData expressionData;
}
