public interface IItem
{
	Hero handOwner { get; set; }

	Entity owner { get; set; }

	DewPlayer tempOwner { get; set; }

	bool isLocked { get; }

	bool skipStartAnimation { get; }

	Rarity rarity { get; }

	ItemWorldModel worldModel { get; }

	bool IsLockedFor(DewPlayer player);
}
