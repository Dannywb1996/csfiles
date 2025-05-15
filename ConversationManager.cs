using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DewInternal;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class ConversationManager : NetworkedManagerBase<ConversationManager>
{
	private struct ActionResult
	{
		public string nextConversation;

		public int nextLineIndex;
	}

	public SafeAction<uint> ClientEvent_OnStartConversation;

	public SafeAction<uint> ClientEvent_OnConversationLineRequestedCompletion;

	public SafeAction<uint, ShownConversation> ClientEvent_OnConversationShowLineAndRequestUserInput;

	public SafeAction<uint> ClientEvent_OnStopConversation;

	public readonly Dictionary<uint, DewConversationSettings> convSettings = new Dictionary<uint, DewConversationSettings>();

	private uint _nextConversationId = 1u;

	private readonly Dictionary<uint, DewConversationExecutionContext> _convContexts = new Dictionary<uint, DewConversationExecutionContext>();

	private bool _didStartNewConversation;

	public bool hasOngoingLocalConversation => ongoingLocalConversation != null;

	public DewConversationSettings ongoingLocalConversation { get; private set; }

	public override void OnStartServer()
	{
		base.OnStartServer();
		DewNetworkManager dewNetworkManager = DewNetworkManager.instance;
		dewNetworkManager.onHumanPlayerRemove = (Action<DewPlayer>)Delegate.Combine(dewNetworkManager.onHumanPlayerRemove, (Action<DewPlayer>)delegate(DewPlayer removed)
		{
			uint[] array = convSettings.Keys.ToArray();
			foreach (uint num in array)
			{
				if (!(convSettings[num].player != removed))
				{
					StopConversation(num);
				}
			}
		});
	}

	private bool ValidateConversationSettings(DewConversationSettings s)
	{
		string og = s.startConversationKey;
		s.startConversationKey = ResolveConversationKey(og);
		if (s.startConversationKey == null)
		{
			Debug.LogWarning("Conversation '" + og + "' not found");
			return false;
		}
		if (s.player == null || !s.player.isHumanPlayer)
		{
			Debug.LogWarning("Invalid player for conversation '" + s.startConversationKey + "'");
			return false;
		}
		return true;
	}

	private string ResolveConversationKey(string patternKey, string fromKey = null)
	{
		string baseScope = ((fromKey != null && fromKey.Contains(".")) ? fromKey.Substring(0, fromKey.LastIndexOf(".", StringComparison.InvariantCulture)) : null);
		List<string> list = new List<string>();
		if (baseScope != null)
		{
			if (DewLocalization.GetConversationData(baseScope + "." + patternKey) != null)
			{
				return baseScope + "." + patternKey;
			}
			foreach (string k in DewLocalization.data.conversations.Keys)
			{
				if (k.EqualsWildcard(baseScope + "." + patternKey))
				{
					list.Add(k);
				}
			}
			if (list.Count > 0)
			{
				return list[UnityEngine.Random.Range(0, list.Count)];
			}
			list.Clear();
		}
		if (DewLocalization.GetConversationData(patternKey) != null)
		{
			return patternKey;
		}
		foreach (string k2 in DewLocalization.data.conversations.Keys)
		{
			if (k2.EqualsWildcard(patternKey))
			{
				list.Add(k2);
			}
		}
		if (list.Count > 0)
		{
			return list[UnityEngine.Random.Range(0, list.Count)];
		}
		Debug.LogWarning("Failed to resolve conversation key: " + patternKey + " (Scope: " + baseScope + ")");
		return null;
	}

	[Server]
	public uint StartConversation(DewConversationSettings s)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.UInt32 ConversationManager::StartConversation(DewConversationSettings)' called when server was not active");
			return default(uint);
		}
		if (!ValidateConversationSettings(s))
		{
			return 0u;
		}
		s._seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		uint id = _nextConversationId;
		_nextConversationId++;
		DewConversationExecutionContext conv = new DewConversationExecutionContext();
		_convContexts.Add(id, conv);
		conv.coroutine = StartCoroutine(ConversationRoutine_Imp(id, s, conv));
		return id;
	}

	public IEnumerator StartConversationRoutine(DewConversationSettings s)
	{
		return new WaitForPromise(delegate(Action resolve, Action<Exception> reject)
		{
			DewConversationSettings dewConversationSettings = s;
			dewConversationSettings.onStop = (Action)Delegate.Combine(dewConversationSettings.onStop, resolve);
			StartConversation(s);
		});
	}

	private IEnumerator ConversationRoutine_Imp(uint id, DewConversationSettings s, DewConversationExecutionContext context)
	{
		uint[] array = convSettings.Keys.ToArray();
		foreach (uint k in array)
		{
			DewConversationSettings c = convSettings[k];
			if (c.player == s.player || s.speakers.Any((Entity value) => c.speakers.Contains(value)))
			{
				StopConversation(k);
			}
		}
		Entity[] speakers = s.speakers;
		foreach (Entity spk in speakers)
		{
			NetworkedManagerBase<ActorManager>.instance.serverActor.CreateStatusEffect<Se_InConversation>(spk, new CastInfo(spk));
		}
		if (s.rotateTowardsCenter)
		{
			Vector3 center = Vector3.zero;
			int count = 0;
			speakers = s.speakers;
			foreach (Entity spk2 in speakers)
			{
				if (!spk2.IsNullInactiveDeadOrKnockedOut())
				{
					center += spk2.agentPosition;
					count++;
				}
			}
			center /= (float)count;
			speakers = s.speakers;
			foreach (Entity spk3 in speakers)
			{
				if (!spk3.IsNullInactiveDeadOrKnockedOut())
				{
					spk3.Control.RotateTowards(center, immediately: false);
				}
			}
		}
		AddConversationAndInvokeEvents(id, s);
		context.currentLineIndex = 0;
		context.currentKey = s.startConversationKey;
		context.currentData = DewLocalization.GetConversationData(context.currentKey);
		List<int> choices = new List<int>();
		while (context.currentData != null && context.currentLineIndex < context.currentData.lines.Length && context.currentLineIndex >= 0 && !s.speakers.Any((Entity ent) => ent.IsNullInactiveDeadOrKnockedOut()))
		{
			LineData currentLine = context.currentData.lines[context.currentLineIndex];
			if (currentLine.type == LineType.Say)
			{
				choices.Clear();
				for (int i2 = context.currentLineIndex + 1; i2 < context.currentData.lines.Length && context.currentData.lines[i2].type == LineType.Choice; i2++)
				{
					choices.Add(i2);
				}
				if (choices.Count > 0)
				{
					yield return ShowLineWithChoicesAndWaitForChoice(id, context.currentKey, context.currentLineIndex, choices);
					context.currentLineIndex = context.userInput;
				}
				else
				{
					yield return ShowLineAndWaitForAdvance(id, context.currentKey, context.currentLineIndex);
					context.currentLineIndex++;
				}
				continue;
			}
			if (currentLine.type == LineType.Choice || currentLine.type == LineType.Action)
			{
				ActionResult res = new ActionResult
				{
					nextLineIndex = -1
				};
				ExecuteActionString(id, currentLine.actionString, ref res);
				yield return null;
				if (res.nextConversation != null)
				{
					context.currentLineIndex = 0;
					context.currentKey = ResolveConversationKey(res.nextConversation, context.currentKey);
					context.currentData = DewLocalization.GetConversationData(context.currentKey);
				}
				else if (res.nextLineIndex >= 0)
				{
					context.currentLineIndex = res.nextLineIndex;
				}
				else if (currentLine.type == LineType.Choice)
				{
					context.currentLineIndex++;
					while (context.currentLineIndex < context.currentData.lines.Length && context.currentData.lines[context.currentLineIndex].type == LineType.Choice)
					{
						context.currentLineIndex++;
					}
				}
				else
				{
					context.currentLineIndex++;
				}
				continue;
			}
			throw new ArgumentOutOfRangeException();
		}
		StopConversation(id);
	}

	[Server]
	private void AddConversationAndInvokeEvents(uint id, DewConversationSettings s)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ConversationManager::AddConversationAndInvokeEvents(System.UInt32,DewConversationSettings)' called when server was not active");
			return;
		}
		convSettings.Add(id, s);
		RpcAddConversationAndInvokeEvents(id, s);
	}

	[ClientRpc]
	private void RpcAddConversationAndInvokeEvents(uint id, DewConversationSettings s)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteUInt(id);
		GeneratedNetworkCode._Write_DewConversationSettings(writer, s);
		SendRPCInternal("System.Void ConversationManager::RpcAddConversationAndInvokeEvents(System.UInt32,DewConversationSettings)", -594341626, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void AddConversationAndInvokeEvents_Imp(uint id, DewConversationSettings s)
	{
		if (convSettings.TryGetValue(id, out var existingSettings))
		{
			s = existingSettings;
		}
		else
		{
			convSettings.Add(id, s);
		}
		if (s.player.isLocalPlayer && !hasOngoingLocalConversation)
		{
			ongoingLocalConversation = s;
			ManagerBase<ControlManager>.instance.DisableCharacterControls();
		}
		try
		{
			ClientEvent_OnStartConversation?.Invoke(id);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private IEnumerator ShowLineAndWaitForAdvance(uint id, string currentKey, int currentLine)
	{
		DewConversationExecutionContext context = _convContexts[id];
		context.waitingForUserInput = true;
		context.userInput = -1;
		RpcShowLineAndRequestUserInput(id, currentKey, currentLine, null);
		yield return new WaitWhile(() => context.waitingForUserInput);
	}

	private IEnumerator ShowLineWithChoicesAndWaitForChoice(uint id, string currentKey, int currentLine, List<int> choices)
	{
		DewConversationExecutionContext context = _convContexts[id];
		context.waitingForUserInput = true;
		context.userInput = -1;
		RpcShowLineAndRequestUserInput(id, currentKey, currentLine, choices.ToArray());
		yield return new WaitWhile(() => context.waitingForUserInput);
		if (!choices.Contains(context.userInput))
		{
			context.userInput = choices[0];
		}
	}

	[Server]
	public void StopConversation(uint id)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ConversationManager::StopConversation(System.UInt32)' called when server was not active");
			return;
		}
		if (!_convContexts.ContainsKey(id))
		{
			Debug.LogWarning($"Tried to stop conversation with non-existent id: {id}");
			return;
		}
		StopCoroutine(_convContexts[id].coroutine);
		_convContexts.Remove(id);
		Entity[] speakers = convSettings[id].speakers;
		foreach (Entity s in speakers)
		{
			if (!s.IsNullInactiveDeadOrKnockedOut() && s.Status.TryGetStatusEffect<Se_InConversation>(out var se))
			{
				se.Destroy();
			}
		}
		RpcStopConversation(id);
	}

	[ClientRpc]
	private void RpcStopConversation(uint id)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteUInt(id);
		SendRPCInternal("System.Void ConversationManager::RpcStopConversation(System.UInt32)", 324059125, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Command(requiresAuthority = false)]
	public void CmdRequestLineCompletion(uint id, int lineIndex, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteUInt(id);
		writer.WriteInt(lineIndex);
		SendCommandInternal("System.Void ConversationManager::CmdRequestLineCompletion(System.UInt32,System.Int32,Mirror.NetworkConnectionToClient)", 1106884890, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcCompleteLine(uint id)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteUInt(id);
		SendRPCInternal("System.Void ConversationManager::RpcCompleteLine(System.UInt32)", 1426307149, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcShowLineAndRequestUserInput(uint id, string key, int lineIndex, int[] choices)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteUInt(id);
		writer.WriteString(key);
		writer.WriteInt(lineIndex);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, choices);
		SendRPCInternal("System.Void ConversationManager::RpcShowLineAndRequestUserInput(System.UInt32,System.String,System.Int32,System.Int32[])", 270071100, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Command(requiresAuthority = false)]
	public void CmdDoUserInputOnConversation(uint id, int lineIndexFrom, int userInput, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteUInt(id);
		writer.WriteInt(lineIndexFrom);
		writer.WriteInt(userInput);
		SendCommandInternal("System.Void ConversationManager::CmdDoUserInputOnConversation(System.UInt32,System.Int32,System.Int32,Mirror.NetworkConnectionToClient)", -416962420, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	private void ExecuteActionString(uint convId, string actionString, ref ActionResult result)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ConversationManager::ExecuteActionString(System.UInt32,System.String,ConversationManager/ActionResult&)' called when server was not active");
		}
		else
		{
			if (string.IsNullOrWhiteSpace(actionString))
			{
				return;
			}
			actionString = actionString.Trim();
			if (!actionString.Contains(";"))
			{
				try
				{
					int spaceIndex = actionString.IndexOf(" ", StringComparison.InvariantCulture);
					string type = ((spaceIndex < 0) ? actionString : actionString.Substring(0, spaceIndex));
					string param = ((spaceIndex < 0) ? null : actionString.Substring(spaceIndex + 1));
					if (type.Equals("start", StringComparison.InvariantCultureIgnoreCase))
					{
						result = default(ActionResult);
						result.nextConversation = param;
					}
					else if (type.Equals("goto", StringComparison.InvariantCultureIgnoreCase))
					{
						result = default(ActionResult);
						result.nextLineIndex = int.Parse(param);
					}
					else if (type.Equals("start", StringComparison.InvariantCultureIgnoreCase))
					{
						result = default(ActionResult);
						result.nextConversation = param;
					}
					else
					{
						if (!type.Equals("call", StringComparison.InvariantCultureIgnoreCase))
						{
							throw new ArgumentOutOfRangeException();
						}
						if (param == null || convSettings[convId].callFunctions == null || !convSettings[convId].callFunctions.TryGetValue(param, out var func))
						{
							Debug.LogWarning("Call function not found: " + param);
						}
						else
						{
							func();
						}
					}
					return;
				}
				catch (Exception exception)
				{
					Debug.LogWarning("Exception occured while executing ActionString '" + actionString + "'");
					Debug.LogException(exception);
					result = default(ActionResult);
					result.nextLineIndex = -1;
					return;
				}
			}
			string[] array = actionString.Split(";");
			foreach (string s in array)
			{
				ExecuteActionString(convId, s, ref result);
			}
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcAddConversationAndInvokeEvents__UInt32__DewConversationSettings(uint id, DewConversationSettings s)
	{
		AddConversationAndInvokeEvents_Imp(id, s);
	}

	protected static void InvokeUserCode_RpcAddConversationAndInvokeEvents__UInt32__DewConversationSettings(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAddConversationAndInvokeEvents called on server.");
		}
		else
		{
			((ConversationManager)obj).UserCode_RpcAddConversationAndInvokeEvents__UInt32__DewConversationSettings(reader.ReadUInt(), GeneratedNetworkCode._Read_DewConversationSettings(reader));
		}
	}

	protected void UserCode_RpcStopConversation__UInt32(uint id)
	{
		if (!convSettings.ContainsKey(id))
		{
			return;
		}
		DewConversationSettings settings = convSettings[id];
		if (settings.isLocalAuthority && hasOngoingLocalConversation)
		{
			ongoingLocalConversation = null;
			ManagerBase<ControlManager>.instance.EnableCharacterControls();
		}
		try
		{
			settings.onStop?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		convSettings.Remove(id);
		try
		{
			ClientEvent_OnStopConversation?.Invoke(id);
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	protected static void InvokeUserCode_RpcStopConversation__UInt32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStopConversation called on server.");
		}
		else
		{
			((ConversationManager)obj).UserCode_RpcStopConversation__UInt32(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdRequestLineCompletion__UInt32__Int32__NetworkConnectionToClient(uint id, int lineIndex, NetworkConnectionToClient sender)
	{
		if (_convContexts.ContainsKey(id))
		{
			DewConversationExecutionContext context = _convContexts[id];
			DewConversationSettings settings = convSettings[id];
			if (!(sender.GetPlayer() != settings.player) && context.currentLineIndex == lineIndex)
			{
				RpcCompleteLine(id);
			}
		}
	}

	protected static void InvokeUserCode_CmdRequestLineCompletion__UInt32__Int32__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestLineCompletion called on client.");
		}
		else
		{
			((ConversationManager)obj).UserCode_CmdRequestLineCompletion__UInt32__Int32__NetworkConnectionToClient(reader.ReadUInt(), reader.ReadInt(), senderConnection);
		}
	}

	protected void UserCode_RpcCompleteLine__UInt32(uint id)
	{
		try
		{
			ClientEvent_OnConversationLineRequestedCompletion?.Invoke(id);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_RpcCompleteLine__UInt32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCompleteLine called on server.");
		}
		else
		{
			((ConversationManager)obj).UserCode_RpcCompleteLine__UInt32(reader.ReadUInt());
		}
	}

	protected void UserCode_RpcShowLineAndRequestUserInput__UInt32__String__Int32__Int32_005B_005D(uint id, string key, int lineIndex, int[] choices)
	{
		ClientEvent_OnConversationShowLineAndRequestUserInput?.Invoke(id, new ShownConversation
		{
			key = key,
			lineIndex = lineIndex,
			choices = choices
		});
	}

	protected static void InvokeUserCode_RpcShowLineAndRequestUserInput__UInt32__String__Int32__Int32_005B_005D(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowLineAndRequestUserInput called on server.");
		}
		else
		{
			((ConversationManager)obj).UserCode_RpcShowLineAndRequestUserInput__UInt32__String__Int32__Int32_005B_005D(reader.ReadUInt(), reader.ReadString(), reader.ReadInt(), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader));
		}
	}

	protected void UserCode_CmdDoUserInputOnConversation__UInt32__Int32__Int32__NetworkConnectionToClient(uint id, int lineIndexFrom, int userInput, NetworkConnectionToClient sender)
	{
		if (_convContexts.ContainsKey(id))
		{
			DewConversationSettings settings = convSettings[id];
			DewConversationExecutionContext context = _convContexts[id];
			if (!(sender.GetPlayer() != settings.player) && context.waitingForUserInput && context.currentLineIndex == lineIndexFrom)
			{
				context.userInput = userInput;
				context.waitingForUserInput = false;
			}
		}
	}

	protected static void InvokeUserCode_CmdDoUserInputOnConversation__UInt32__Int32__Int32__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDoUserInputOnConversation called on client.");
		}
		else
		{
			((ConversationManager)obj).UserCode_CmdDoUserInputOnConversation__UInt32__Int32__Int32__NetworkConnectionToClient(reader.ReadUInt(), reader.ReadInt(), reader.ReadInt(), senderConnection);
		}
	}

	static ConversationManager()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(ConversationManager), "System.Void ConversationManager::CmdRequestLineCompletion(System.UInt32,System.Int32,Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdRequestLineCompletion__UInt32__Int32__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterCommand(typeof(ConversationManager), "System.Void ConversationManager::CmdDoUserInputOnConversation(System.UInt32,System.Int32,System.Int32,Mirror.NetworkConnectionToClient)", InvokeUserCode_CmdDoUserInputOnConversation__UInt32__Int32__Int32__NetworkConnectionToClient, requiresAuthority: false);
		RemoteProcedureCalls.RegisterRpc(typeof(ConversationManager), "System.Void ConversationManager::RpcAddConversationAndInvokeEvents(System.UInt32,DewConversationSettings)", InvokeUserCode_RpcAddConversationAndInvokeEvents__UInt32__DewConversationSettings);
		RemoteProcedureCalls.RegisterRpc(typeof(ConversationManager), "System.Void ConversationManager::RpcStopConversation(System.UInt32)", InvokeUserCode_RpcStopConversation__UInt32);
		RemoteProcedureCalls.RegisterRpc(typeof(ConversationManager), "System.Void ConversationManager::RpcCompleteLine(System.UInt32)", InvokeUserCode_RpcCompleteLine__UInt32);
		RemoteProcedureCalls.RegisterRpc(typeof(ConversationManager), "System.Void ConversationManager::RpcShowLineAndRequestUserInput(System.UInt32,System.String,System.Int32,System.Int32[])", InvokeUserCode_RpcShowLineAndRequestUserInput__UInt32__String__Int32__Int32_005B_005D);
	}
}
