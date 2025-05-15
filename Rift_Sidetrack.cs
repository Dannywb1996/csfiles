using System.Collections.Generic;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

[DewResourceLink(ResourceLinkBy.Name)]
public class Rift_Sidetrack : Rift
{
	public Color mainColor;

	public float spawnChance = 0.01f;

	public bool oncePerLoop;

	public string[] allowedZones;

	public bool useWildcardPattern;

	public string wildcardPattern;

	public List<string> manualRoomNames = new List<string>();

	public new static Rift_Sidetrack instance { get; private set; }

	public bool isValid
	{
		get
		{
			if (!useWildcardPattern)
			{
				return manualRoomNames.Count > 0;
			}
			return !string.IsNullOrEmpty(wildcardPattern);
		}
	}

	private string GetZoneValidationMessage(string[] obj)
	{
		if (obj == null)
		{
			return null;
		}
		foreach (string o in obj)
		{
			if (!DewResources.database.nameToGuid.ContainsKey(o))
			{
				return "Zone of name '" + o + "' not found";
			}
		}
		return null;
	}

	protected override void Awake()
	{
		base.Awake();
		instance = this;
	}

	internal List<string> GetRoomNames()
	{
		if (!useWildcardPattern)
		{
			return manualRoomNames;
		}
		List<string> list = new List<string>();
		foreach (string n in DewResources.database.sceneNames)
		{
			if (n.EqualsWildcard(wildcardPattern))
			{
				list.Add(n);
			}
		}
		return list;
	}

	protected override bool OnInteractRift(Hero hero)
	{
		TpcPromptConfirmation(hero.owner, NetworkedManagerBase<ZoneManager>.instance.isSidetracking);
		return false;
	}

	protected virtual string GetConfirmMessage()
	{
		return null;
	}

	[TargetRpc]
	private void TpcPromptConfirmation(NetworkConnectionToClient target, bool isReturning)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(isReturning);
		SendTargetRPCInternal(target, "System.Void Rift_Sidetrack::TpcPromptConfirmation(Mirror.NetworkConnectionToClient,System.Boolean)", 590471613, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Command(requiresAuthority = false)]
	private void CmdConfirmMove(NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendCommandInternal("System.Void Rift_Sidetrack::CmdConfirmMove(Mirror.NetworkConnectionToClient)", -1791713517, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	public virtual void TravelImmediately()
	{
		if (NetworkedManagerBase<ZoneManager>.instance.isSidetracking)
		{
			NetworkedManagerBase<ZoneManager>.instance.ReturnFromSidetracking();
			return;
		}
		List<string> roomNames = GetRoomNames();
		NetworkedManagerBase<ZoneManager>.instance.LoadSidetrackRoom(roomNames[Random.Range(0, roomNames.Count)]);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcPromptConfirmation__NetworkConnectionToClient__Boolean(NetworkConnectionToClient target, bool isReturning)
	{
		if (NetworkedManagerBase<ZoneManager>.instance.isVoting)
		{
			InGameUIManager.instance.ShowCenterMessage(CenterMessageType.Error, "InGame_Message_AlreadyVotingForTravel");
			return;
		}
		string text = GetConfirmMessage();
		if (text == null)
		{
			text = DewLocalization.GetUIValue(isReturning ? "InGame_Message_ReturnToPreviousWorld" : "InGame_Message_TravelToOtherworld");
		}
		ManagerBase<MessageManager>.instance.ShowMessage(new DewMessageSettings
		{
			rawContent = text,
			buttons = (DewMessageSettings.ButtonType.Yes | DewMessageSettings.ButtonType.No),
			defaultButton = DewMessageSettings.ButtonType.No,
			onClose = delegate(DewMessageSettings.ButtonType b)
			{
				if (b == DewMessageSettings.ButtonType.Yes)
				{
					NetworkedManagerBase<ZoneManager>.instance.TravelWithValidationAndConfirmation(delegate
					{
						CmdConfirmMove();
					}, ignoreSidetrack: true);
				}
			},
			validator = () => this != null && CanInteract(DewPlayer.local.hero) && InGameUIManager.ValidateInGameActionMessage()
		});
	}

	protected static void InvokeUserCode_TpcPromptConfirmation__NetworkConnectionToClient__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcPromptConfirmation called on server.");
		}
		else
		{
			((Rift_Sidetrack)obj).UserCode_TpcPromptConfirmation__NetworkConnectionToClient__Boolean((NetworkConnectionToClient)NetworkClient.connection, reader.ReadBool());
		}
	}

	protected void UserCode_CmdConfirmMove__NetworkConnectionToClient(NetworkConnectionToClient sender)
	{
		DewPlayer player = sender.GetPlayer();
		if (!(player == null) && !NetworkedManagerBase<ZoneManager>.instance.isVoting)
		{
			FxPlayNetworked(fxActivate);
			if (NetworkedManagerBase<ZoneManager>.instance.ShouldVoteOnTravel())
			{
				NetworkedManagerBase<ZoneManager>.instance.StartVoteSidetrack(player, this);
			}
			else
			{
				TravelImmediately();
			}
		}
	}

	protected static void InvokeUserCode_CmdConfirmMove__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdConfirmMove called on client.");
		}
		else
		{
			((Rift_Sidetrack)obj).UserCode_CmdConfirmMove__NetworkConnectionToClient(senderConnection);
		}
	}

	static Rift_Sidetrack()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(Rift_Sidetrack), "System.Void Rift_Sidetrack::CmdConfirmMove(Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdConfirmMove__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterRpc(typeof(Rift_Sidetrack), "System.Void Rift_Sidetrack::TpcPromptConfirmation(Mirror.NetworkConnectionToClient,System.Boolean)", InvokeUserCode_TpcPromptConfirmation__NetworkConnectionToClient__Boolean);
	}
}
