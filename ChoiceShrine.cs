using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public abstract class ChoiceShrine : Shrine
{
	protected readonly SyncList<ChoiceShrineItem> _choices = new SyncList<ChoiceShrineItem>();

	public int itemCount = 3;

	public IReadOnlyList<ChoiceShrineItem> choices => _choices;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			SetupSelections();
		}
	}

	protected abstract void SetupSelections();

	protected override bool OnUse(Entity entity)
	{
		TpcOpenSelection(entity.owner.connectionToClient);
		return false;
	}

	[TargetRpc]
	private void TpcOpenSelection(NetworkConnectionToClient target)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendTargetRPCInternal(target, "System.Void ChoiceShrine::TpcOpenSelection(Mirror.NetworkConnectionToClient)", 1393396993, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Command(requiresAuthority = false)]
	public void CmdChoose(int index, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(index);
		SendCommandInternal("System.Void ChoiceShrine::CmdChoose(System.Int32,Mirror.NetworkConnectionToClient)", -1078923672, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	protected ChoiceShrine()
	{
		InitSyncObject(_choices);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcOpenSelection__NetworkConnectionToClient(NetworkConnectionToClient target)
	{
		ManagerBase<FloatingWindowManager>.instance.SetTarget(this);
	}

	protected static void InvokeUserCode_TpcOpenSelection__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcOpenSelection called on server.");
		}
		else
		{
			((ChoiceShrine)obj).UserCode_TpcOpenSelection__NetworkConnectionToClient((NetworkConnectionToClient)NetworkClient.connection);
		}
	}

	protected void UserCode_CmdChoose__Int32__NetworkConnectionToClient(int index, NetworkConnectionToClient sender)
	{
		if (index < 0 || index >= _choices.Count)
		{
			return;
		}
		DewPlayer player = sender.GetPlayer();
		Hero hero;
		if (!(player == null))
		{
			hero = player.hero;
			if (!(hero == null) && CanAfford(hero) == AffordType.Yes && base.currentUseCount < maxUseCount)
			{
				StartCoroutine(Routine());
			}
		}
		IEnumerator Routine()
		{
			Vector3 pos = GetRandomSpawnPosition(hero.position);
			DoPostUseRoutines(hero);
			yield return new WaitForSeconds(0.5f);
			ChoiceShrineItem itemData = _choices[index];
			Object itemPrefab = DewResources.GetByShortTypeName(itemData.typeName);
			if (itemPrefab is SkillTrigger skill)
			{
				Dew.CreateSkillTrigger(skill, pos, itemData.level, player);
			}
			else if (itemPrefab is Gem gem)
			{
				Dew.CreateGem(gem, pos, itemData.level, player);
			}
		}
	}

	protected static void InvokeUserCode_CmdChoose__Int32__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChoose called on client.");
		}
		else
		{
			((ChoiceShrine)obj).UserCode_CmdChoose__Int32__NetworkConnectionToClient(reader.ReadInt(), senderConnection);
		}
	}

	static ChoiceShrine()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(ChoiceShrine), "System.Void ChoiceShrine::CmdChoose(System.Int32,Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdChoose__Int32__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterRpc(typeof(ChoiceShrine), "System.Void ChoiceShrine::TpcOpenSelection(Mirror.NetworkConnectionToClient)", InvokeUserCode_TpcOpenSelection__NetworkConnectionToClient);
	}
}
