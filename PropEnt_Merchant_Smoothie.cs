using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PropEnt_Merchant_Smoothie : PropEnt_Merchant_Base
{
	public int treasuresCount = 3;

	public int souvenirsCount = 3;

	private List<Treasure> _treasures;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!base.isServer)
		{
			return;
		}
		_treasures = new List<Treasure>();
		foreach (Treasure t in DewResources.FindAllByTypeSubstring<Treasure>("Treasure_"))
		{
			if (Dew.IsTreasureIncludedInGame(t.GetType().Name))
			{
				_treasures.Add(t);
			}
		}
		Dew.CallDelayed(delegate
		{
			if (_merchandises.Count > 0)
			{
				return;
			}
			foreach (DewPlayer current in DewPlayer.humanPlayers)
			{
				PopulatePlayerMerchandises(current);
				remainingRefreshes[current.netId] = current.allowedShopRefreshes;
			}
		});
	}

	protected override void OnRefresh(DewPlayer player)
	{
		base.OnRefresh(player);
		PopulatePlayerMerchandises(player);
	}

	private void PopulatePlayerMerchandises(DewPlayer player)
	{
		MerchandiseData[] data = new MerchandiseData[treasuresCount + souvenirsCount];
		List<Treasure> treasures = _treasures.ToList();
		for (int i = treasures.Count - 1; i >= 0; i--)
		{
			treasures[i].merchant = this;
			treasures[i].player = player;
			treasures[i].hero = player.hero;
			if (!treasures[i].ShouldBeIncludedInPool())
			{
				treasures.RemoveAt(i);
			}
		}
		for (int j = 0; j < treasuresCount; j++)
		{
			Treasure t;
			if (treasures.Count == 0)
			{
				t = _treasures[UnityEngine.Random.Range(0, _treasures.Count)];
			}
			else
			{
				int index = UnityEngine.Random.Range(0, treasures.Count);
				t = treasures[index];
				treasures.RemoveAt(index);
			}
			try
			{
				t.OnAddMerchandise(out var mercPrice, out var customData);
				data[j] = new MerchandiseData
				{
					type = MerchandiseType.Treasure,
					itemName = t.GetType().Name,
					count = t.maxUse,
					price = mercPrice,
					customData = customData
				};
			}
			catch (Exception exception)
			{
				data[j].type = MerchandiseType.Empty;
				Debug.LogException(exception);
			}
		}
		for (int k = 0; k < souvenirsCount; k++)
		{
			data[treasuresCount + k].type = MerchandiseType.Empty;
		}
		_merchandises[player.netId] = data;
	}

	private void MirrorProcessed()
	{
	}
}
