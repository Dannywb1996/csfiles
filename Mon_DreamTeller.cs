using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Mon_DreamTeller : Monster, IInteractable, IForceHeroicHealthbar
{
	public GameObject fxExplode;

	public float showStoryDelay;

	private bool _isEligibleForMoreTalks;

	protected override DewPlayer defaultOwner => DewPlayer.environment;

	public Transform interactPivot => base.transform;

	public bool canInteractWithMouse => false;

	public float focusDistance => 4.5f;

	public int priority
	{
		get
		{
			if (NetworkedManagerBase<QuestManager>.instance.currentArtifact == null)
			{
				return 100;
			}
			return 10;
		}
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		if (base.isServer)
		{
			CreateStatusEffect<Se_OntologicalShield>(this, new CastInfo(this));
			_isEligibleForMoreTalks = UnityEngine.Random.value < 0.25f;
		}
	}

	public bool CanInteract(Entity entity)
	{
		return !base.Status.isInConversation;
	}

	public void OnInteract(Entity entity, bool alt)
	{
		if (!alt && entity.isOwned && !base.Status.isInConversation)
		{
			int unlocked = DewSave.profile.artifacts.Count((KeyValuePair<string, DewProfile.UnlockData> p) => p.Value.status == UnlockStatus.Complete);
			CmdTalkWithDreamTeller(entity, DewSave.profile.didMeetDreamTeller, unlocked >= 3);
			DewSave.profile.didMeetDreamTeller = true;
		}
	}

	[Command(requiresAuthority = false)]
	private void CmdTalkWithDreamTeller(Entity entity, bool didMeetDreamTeller, bool canDoMoreTalks, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(entity);
		writer.WriteBool(didMeetDreamTeller);
		writer.WriteBool(canDoMoreTalks);
		SendCommandInternal("System.Void Mon_DreamTeller::CmdTalkWithDreamTeller(Entity,System.Boolean,System.Boolean,Mirror.NetworkConnectionToClient)", 964231878, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	private void GiveArtifact(Entity from)
	{
		if (NetworkedManagerBase<QuestManager>.instance.currentArtifact != null)
		{
			Artifact artifact = DewResources.GetByShortTypeName<Artifact>(NetworkedManagerBase<QuestManager>.instance.currentArtifact);
			NetworkedManagerBase<QuestManager>.instance.RemoveArtifact();
			NetworkedManagerBase<ConversationManager>.instance.StartConversation(new DewConversationSettings
			{
				player = from.owner,
				speakers = new Entity[2] { this, from },
				visibility = ConversationVisibility.Everyone,
				rotateTowardsCenter = true,
				startConversationKey = "DreamTeller.Thanks*",
				onStop = delegate
				{
					OpenArtifact(from, artifact);
				},
				variables = new Dictionary<string, string> { 
				{
					"artifact",
					artifact.GetType().Name
				} }
			});
		}
	}

	[Server]
	private void OpenArtifact(Entity from, Artifact artifact)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Mon_DreamTeller::OpenArtifact(Entity,Artifact)' called when server was not active");
			return;
		}
		Vector3 pos = base.position;
		base.Control.StartDaze(4f);
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			FxPlayNetworked(fxExplode, this);
			if (from != null && from.owner != null && from.owner.isHumanPlayer)
			{
				TpcDisableCharacterControls(from.owner);
			}
			yield return new WaitForSeconds(showStoryDelay);
			if (from != null && from.owner != null && from.owner.isHumanPlayer)
			{
				TpcEnableCharacterControls(from.owner);
			}
			NetworkedManagerBase<QuestManager>.instance.DiscoverArtifactAndShowStory(artifact.GetType().Name);
			yield return new WaitForSeconds(1f);
			NetworkedManagerBase<PickupManager>.instance.DropStarDust(artifact.grantedStardust, pos);
		}
	}

	[TargetRpc]
	private void TpcDisableCharacterControls(NetworkConnectionToClient target)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendTargetRPCInternal(target, "System.Void Mon_DreamTeller::TpcDisableCharacterControls(Mirror.NetworkConnectionToClient)", 1498787282, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[TargetRpc]
	private void TpcEnableCharacterControls(NetworkConnectionToClient target)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendTargetRPCInternal(target, "System.Void Mon_DreamTeller::TpcEnableCharacterControls(Mirror.NetworkConnectionToClient)", 39458877, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	public override bool ShouldBeSaved()
	{
		return false;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdTalkWithDreamTeller__Entity__Boolean__Boolean__NetworkConnectionToClient(Entity entity, bool didMeetDreamTeller, bool canDoMoreTalks, NetworkConnectionToClient sender)
	{
		if (sender == null || sender.GetPlayer() == null || entity.owner != sender.GetPlayer() || base.Status.isInConversation)
		{
			return;
		}
		if (NetworkedManagerBase<QuestManager>.instance.currentArtifact != null)
		{
			NetworkedManagerBase<ConversationManager>.instance.StartConversation(new DewConversationSettings
			{
				player = entity.owner,
				speakers = new Entity[2] { this, entity },
				visibility = ConversationVisibility.Everyone,
				rotateTowardsCenter = true,
				startConversationKey = (didMeetDreamTeller ? "DreamTeller.Intro*" : "DreamTeller.FirstTimeIntro"),
				callFunctions = new Dictionary<string, Action> { 
				{
					"GiveArtifact",
					delegate
					{
						GiveArtifact(entity);
					}
				} }
			});
		}
		else if (canDoMoreTalks && _isEligibleForMoreTalks)
		{
			NetworkedManagerBase<ConversationManager>.instance.StartConversation(new DewConversationSettings
			{
				player = entity.owner,
				speakers = new Entity[2] { this, entity },
				visibility = ConversationVisibility.Everyone,
				rotateTowardsCenter = true,
				startConversationKey = "DreamTeller.TalkAboutSelfChoice"
			});
			_isEligibleForMoreTalks = false;
		}
		else
		{
			NetworkedManagerBase<ConversationManager>.instance.StartConversation(new DewConversationSettings
			{
				player = entity.owner,
				speakers = new Entity[2] { this, entity },
				visibility = ConversationVisibility.Everyone,
				rotateTowardsCenter = true,
				startConversationKey = "DreamTeller.ChitChat*"
			});
		}
	}

	protected static void InvokeUserCode_CmdTalkWithDreamTeller__Entity__Boolean__Boolean__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTalkWithDreamTeller called on client.");
		}
		else
		{
			((Mon_DreamTeller)obj).UserCode_CmdTalkWithDreamTeller__Entity__Boolean__Boolean__NetworkConnectionToClient(reader.ReadNetworkBehaviour<Entity>(), reader.ReadBool(), reader.ReadBool(), senderConnection);
		}
	}

	protected void UserCode_TpcDisableCharacterControls__NetworkConnectionToClient(NetworkConnectionToClient target)
	{
		ManagerBase<ControlManager>.instance.DisableCharacterControls();
	}

	protected static void InvokeUserCode_TpcDisableCharacterControls__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcDisableCharacterControls called on server.");
		}
		else
		{
			((Mon_DreamTeller)obj).UserCode_TpcDisableCharacterControls__NetworkConnectionToClient((NetworkConnectionToClient)NetworkClient.connection);
		}
	}

	protected void UserCode_TpcEnableCharacterControls__NetworkConnectionToClient(NetworkConnectionToClient target)
	{
		ManagerBase<ControlManager>.instance.EnableCharacterControls();
	}

	protected static void InvokeUserCode_TpcEnableCharacterControls__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcEnableCharacterControls called on server.");
		}
		else
		{
			((Mon_DreamTeller)obj).UserCode_TpcEnableCharacterControls__NetworkConnectionToClient((NetworkConnectionToClient)NetworkClient.connection);
		}
	}

	static Mon_DreamTeller()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(Mon_DreamTeller), "System.Void Mon_DreamTeller::CmdTalkWithDreamTeller(Entity,System.Boolean,System.Boolean,Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdTalkWithDreamTeller__Entity__Boolean__Boolean__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterRpc(typeof(Mon_DreamTeller), "System.Void Mon_DreamTeller::TpcDisableCharacterControls(Mirror.NetworkConnectionToClient)", InvokeUserCode_TpcDisableCharacterControls__NetworkConnectionToClient);
		RemoteProcedureCalls.RegisterRpc(typeof(Mon_DreamTeller), "System.Void Mon_DreamTeller::TpcEnableCharacterControls(Mirror.NetworkConnectionToClient)", InvokeUserCode_TpcEnableCharacterControls__NetworkConnectionToClient);
	}
}
