using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomModifiers : RoomComponent
{
	[NonSerialized]
	public List<RoomModifierBase> modifierInstances = new List<RoomModifierBase>();

	private WorldNodeSaveData _currentSave;

	public override void OnRoomStartServer(WorldNodeSaveData save)
	{
		base.OnRoomStartServer(save);
		List<ModifierData> modifiers = NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers;
		for (int i = 0; i < modifiers.Count; i++)
		{
			HandleRuntimeAddition(modifiers[i].id);
		}
		StartCoroutine(ExecuteRoomOnceCoroutine());
	}

	internal void HandleRuntimeRemoval(int id)
	{
		int num = modifierInstances.FindIndex((RoomModifierBase m) => m.id == id);
		if (num < 0)
		{
			Debug.Log($"Runtime removal failed; Modifier instance with id {id} not found");
			return;
		}
		if (!modifierInstances[num].IsNullOrInactive())
		{
			modifierInstances[num].Destroy();
		}
		modifierInstances.RemoveAt(num);
	}

	internal void HandleRuntimeAddition(int id)
	{
		List<ModifierData> modifiers = NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers;
		int num = NetworkedManagerBase<ZoneManager>.instance.currentNode.modifiers.FindIndex((ModifierData m) => m.id == id);
		if (num < 0)
		{
			Debug.Log($"Runtime addition failed; Modifier instance with id {id} not found");
			return;
		}
		ModifierData modifierData = modifiers[num];
		RoomModifierBase byShortTypeName = DewResources.GetByShortTypeName<RoomModifierBase>(modifierData.type);
		bool isNewInstance = !NetworkedManagerBase<ZoneManager>.instance.modifierServerData[id].ContainsKey("didCreateInstance");
		RoomModifierBase item = Dew.CreateActor(byShortTypeName, Vector3.zero, null, null, delegate(RoomModifierBase mod)
		{
			mod.isNewInstance = isNewInstance;
			mod.id = id;
		});
		modifierInstances.Add(item);
		if (isNewInstance)
		{
			NetworkedManagerBase<ZoneManager>.instance.modifierServerData[id].Add("didCreateInstance", true);
		}
		Debug.Log("Added modifier of type " + modifierData.type);
	}

	private void MirrorProcessed()
	{
	}

	private IEnumerator ExecuteRoomOnceCoroutine()
	{
		while (!SingletonDewNetworkBehaviour<Room>.instance.isActive)
		{
			yield return new WaitForSeconds(0.1f);
		}
		AttrCustomizeManager.ExecuteRoomOnce();
	}
}
