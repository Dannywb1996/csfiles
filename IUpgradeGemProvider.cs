public interface IUpgradeGemProvider
{
	int GetDreamDustCost(Gem target);

	int GetAddedQuality();

	bool RequestGemUpgrade(Gem target);
}
