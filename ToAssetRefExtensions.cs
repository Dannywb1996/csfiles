using UnityEngine;
using UnityEngine.AddressableAssets;

public static class ToAssetRefExtensions
{
	public static AssetReferenceGameObject ToAssetRef(this Object obj)
	{
		return new AssetReferenceGameObject(DewResources.database.typeToGuid[obj.GetType()]);
	}
}
