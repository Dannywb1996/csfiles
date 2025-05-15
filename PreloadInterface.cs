using System.Collections.Generic;

public class PreloadInterface
{
	internal HashSet<string> _guids = new HashSet<string>();

	public void AddGuid(string guid)
	{
		_guids.Add(guid);
	}

	public void AddFromMonsterPool(MonsterPool pool)
	{
		if (pool == null)
		{
			return;
		}
		foreach (MonsterPool.SpawnRuleEntry e in pool.entries)
		{
			if (!string.IsNullOrEmpty(e.monsterRef.AssetGUID))
			{
				if (DewResources.database.guidToType.TryGetValue(e.monsterRef.AssetGUID, out var monType))
				{
					AddType(monType.Name);
				}
				else
				{
					AddGuid(e.monsterRef.AssetGUID);
				}
			}
		}
	}

	public void AddType(string type, bool includeDependencies = true)
	{
		if (!DewResources.database.typeNameToGuid.TryGetValue(type, out var guid))
		{
			return;
		}
		AddGuid(guid);
		if (!includeDependencies)
		{
			return;
		}
		ListReturnHandle<string> handle;
		foreach (string dep in DewResources.GetAllDependencies(out handle, type))
		{
			if (DewResources.database.typeNameToGuid.TryGetValue(dep, out var guid2))
			{
				AddGuid(guid2);
			}
		}
		handle.Return();
	}

	public void KeepEverything()
	{
		foreach (string g in DewResources.loadedGuids)
		{
			AddGuid(g);
		}
	}
}
