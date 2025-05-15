public static class WorldNodeTypeExtensions
{
	public static bool IsNormalNode(this WorldNodeType type)
	{
		return type < WorldNodeType.Merchant;
	}
}
