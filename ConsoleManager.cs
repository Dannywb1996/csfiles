using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using IngameDebugConsole;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConsoleManager : NetworkedManagerBase<ConsoleManager>
{
	public enum AutoExecKey
	{
		Global = 0,
		Network = 100,
		NetworkServer = 101,
		NetworkClient = 102,
		Game = 200,
		GameServer = 201,
		GameClient = 202
	}

	public struct ExecutionContext
	{
		public DewPlayer player;

		public Entity selection;

		public Vector3 cursorWorldPos;
	}

	public Entity localSelectedEntity;

	public ExecutionContext executionContext;

	public Material glDrawMaterial;

	[NonSerialized]
	public List<ConsoleBindItem> activeCommandBinds = new List<ConsoleBindItem>();

	public SafeAction<bool> ClientEvent_OnCheatEnabledChanged;

	[SyncVar(hook = "CheatEnabledChanged")]
	private bool _isCheatEnabled;

	private CanvasGroup _consoleWindowCanvas;

	private Camera _mainCamera;

	public bool isCheatEnabled => _isCheatEnabled;

	public bool isConsoleWindowOpen
	{
		get
		{
			if (_consoleWindowCanvas != null)
			{
				return _consoleWindowCanvas.alpha > 0.1f;
			}
			return false;
		}
	}

	public bool Network_isCheatEnabled
	{
		get
		{
			return _isCheatEnabled;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _isCheatEnabled, 1uL, CheatEnabledChanged);
		}
	}

	private void CheatEnabledChanged(bool oldVal, bool newVal)
	{
		try
		{
			ClientEvent_OnCheatEnabledChanged?.Invoke(newVal);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		if (newVal)
		{
			Debug.Log("Cheat is now on");
		}
		else
		{
			Debug.Log("Cheat is now off");
		}
		if (newVal && ManagerBase<InGameAnalyticsManager>.instance != null)
		{
			ManagerBase<InGameAnalyticsManager>.instance.DisableAnalyticsLocal();
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		_mainCamera = Camera.main;
		_consoleWindowCanvas = UnityEngine.Object.FindObjectOfType<DebugLogManager>(includeInactive: true).transform.Find("DebugLogWindow").GetComponent<CanvasGroup>();
		DebugLogConsole.ServerCommandHandler = delegate(string command)
		{
			if (NetworkedManagerBase<GameManager>.instance != null)
			{
				ExecuteServerCommand(command, localSelectedEntity, ControlManager.GetWorldPositionOnGroundOnCursor(forDirectionalAttacks: false));
			}
			else if (NetworkClient.active)
			{
				ExecuteServerCommand(command, localSelectedEntity, Vector3.zero);
			}
			else
			{
				Debug.Log("You're not connected to a game.");
			}
		};
		DebugLogConsole.GameContextValidator = () => NetworkedManagerBase<GameManager>.instance != null;
		DebugLogConsole.CheatContextValidator = () => NetworkedManagerBase<ConsoleManager>.instance != null && NetworkedManagerBase<ConsoleManager>.instance.isCheatEnabled;
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(DrawEntityBounds));
	}

	public override void OnLateStart()
	{
		base.OnLateStart();
		ExecuteAutoExec(AutoExecKey.Global);
	}

	public bool ShouldSkipAutoExec()
	{
		if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl))
		{
			return Input.GetKey(KeyCode.LeftAlt);
		}
		return false;
	}

	public void ExecuteAutoExec(AutoExecKey key)
	{
		if (ShouldSkipAutoExec())
		{
			Debug.Log("Ctrl+Alt+Shift detected, skipping auto-exec of " + key);
			return;
		}
		string[] commands = PlayerPrefs.GetString("AutoExec_" + key, "").Split('\n');
		string[] array = commands;
		foreach (string c in array)
		{
			if (!string.IsNullOrWhiteSpace(c.Trim()))
			{
				Debug.Log($"AutoExec for ({key}) will execute {c}");
			}
		}
		array = commands;
		foreach (string c2 in array)
		{
			if (!string.IsNullOrWhiteSpace(c2.Trim()))
			{
				DebugLogConsole.ExecuteCommand(c2);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(DrawEntityBounds));
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (ControlManager.IsInputFieldFocused())
		{
			return;
		}
		for (int i = 0; i < activeCommandBinds.Count; i++)
		{
			ConsoleBindItem c = activeCommandBinds[i];
			if ((c.type == ConsoleBindItemType.Down && Input.GetKeyDown(c.key)) || (c.type == ConsoleBindItemType.Up && Input.GetKeyUp(c.key)))
			{
				DebugLogConsole.ExecuteCommand(c.command);
			}
		}
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if ((localSelectedEntity == null || !localSelectedEntity.isActive) && DewPlayer.local != null && DewPlayer.local.hero != null)
		{
			localSelectedEntity = DewPlayer.local.hero;
		}
	}

	private void DrawEntityBounds(Camera cam)
	{
		if (_mainCamera == null)
		{
			_mainCamera = Camera.main;
		}
		if (cam != _mainCamera || _consoleWindowCanvas.alpha < 0.1f)
		{
			return;
		}
		Entity entUnderCursor = ControlManager.GetEntityOnCursor();
		foreach (Entity ent in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			Color color = Color.gray;
			if (ent == localSelectedEntity)
			{
				color = Color.green;
			}
			else
			{
				if (!(ent == entUnderCursor))
				{
					continue;
				}
				color = Color.gray;
			}
			GLDrawCircle(ent.position, ent.Control.outerRadius, color);
		}
	}

	public void OnGameAreaPointerDown(PointerEventData eventData)
	{
		if (NetworkedManagerBase<GameManager>.instance == null || !isConsoleWindowOpen)
		{
			return;
		}
		Entity entUnderCursor = ControlManager.GetEntityOnCursor();
		if (Input.GetKeyDown(KeyCode.Mouse0) && entUnderCursor != null)
		{
			localSelectedEntity = entUnderCursor;
			_consoleWindowCanvas.transform.Find("CommandInputField").GetComponent<InputField>().ActivateInputField();
		}
		if (Input.GetKeyDown(KeyCode.Mouse2))
		{
			if (localSelectedEntity == null)
			{
				localSelectedEntity = DewPlayer.local.hero;
			}
			DebugLogConsole.ExecuteCommand("teleport");
			_consoleWindowCanvas.transform.Find("CommandInputField").GetComponent<InputField>().ActivateInputField();
		}
	}

	private void GLDrawCircle(Vector3 pos, float radius, Color color)
	{
		GL.PushMatrix();
		glDrawMaterial.SetPass(0);
		GL.LoadOrtho();
		GL.Begin(1);
		GL.Color(color);
		for (int i = 0; i < 20; i++)
		{
			Vector3 p0 = pos + Vector3.right * Mathf.Cos(MathF.PI / 10f * (float)i) * radius + Vector3.forward * Mathf.Sin(MathF.PI / 10f * (float)i) * radius;
			Vector3 p1 = pos + Vector3.right * Mathf.Cos(MathF.PI / 10f * (float)(i + 1)) * radius + Vector3.forward * Mathf.Sin(MathF.PI / 10f * (float)(i + 1)) * radius;
			Line(p0, p1);
		}
		GL.End();
		GL.PopMatrix();
		void Line(Vector3 start, Vector3 end)
		{
			Vector3 v0 = _mainCamera.WorldToViewportPoint(start);
			Vector3 v1 = _mainCamera.WorldToViewportPoint(end);
			v0.z = 0f;
			v1.z = 0f;
			GL.Vertex(v0);
			GL.Vertex(v1);
		}
	}

	[Server]
	public void EnableCheats()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ConsoleManager::EnableCheats()' called when server was not active");
		}
		else
		{
			Network_isCheatEnabled = true;
		}
	}

	[Server]
	public void DisableCheats()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void ConsoleManager::DisableCheats()' called when server was not active");
		}
		else
		{
			Network_isCheatEnabled = false;
		}
	}

	[Command(requiresAuthority = false)]
	public void ExecuteServerCommand(string command, Entity selected, Vector3 cursorWorldPos, NetworkConnectionToClient sender = null)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(command);
		writer.WriteNetworkBehaviour(selected);
		writer.WriteVector3(cursorWorldPos);
		SendCommandInternal("System.Void ConsoleManager::ExecuteServerCommand(System.String,Entity,UnityEngine.Vector3,Mirror.NetworkConnectionToClient)", -505903775, writer, 0, requiresAuthority: false);
		NetworkWriterPool.Return(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_ExecuteServerCommand__String__Entity__Vector3__NetworkConnectionToClient(string command, Entity selected, Vector3 cursorWorldPos, NetworkConnectionToClient sender)
	{
		executionContext.player = sender.GetPlayer();
		executionContext.selection = selected;
		executionContext.cursorWorldPos = cursorWorldPos;
		Debug.Log(executionContext.player.name + ": " + command);
		try
		{
			DebugLogConsole.ExecuteCommand(command, isComingFromRemote: true);
		}
		catch (Exception ex)
		{
			Exception innerE = ex;
			while (innerE.InnerException != null)
			{
				innerE = innerE.InnerException;
			}
			if (executionContext.player != DewPlayer.local)
			{
				executionContext.player.SendLogWarning("Exception was thrown on server while executing above command");
				executionContext.player.SendLogWarning($"{ex}\n\n");
			}
			Debug.LogWarning("Exception was thrown while executing above command");
			Debug.LogException(ex);
		}
		executionContext = default(ExecutionContext);
	}

	protected static void InvokeUserCode_ExecuteServerCommand__String__Entity__Vector3__NetworkConnectionToClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command ExecuteServerCommand called on client.");
		}
		else
		{
			((ConsoleManager)obj).UserCode_ExecuteServerCommand__String__Entity__Vector3__NetworkConnectionToClient(reader.ReadString(), reader.ReadNetworkBehaviour<Entity>(), reader.ReadVector3(), senderConnection);
		}
	}

	static ConsoleManager()
	{
		RemoteProcedureCalls.RegisterCommand(typeof(ConsoleManager), "System.Void ConsoleManager::ExecuteServerCommand(System.String,Entity,UnityEngine.Vector3,Mirror.NetworkConnectionToClient)", InvokeUserCode_ExecuteServerCommand__String__Entity__Vector3__NetworkConnectionToClient, requiresAuthority: false);
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(_isCheatEnabled);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(_isCheatEnabled);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref _isCheatEnabled, CheatEnabledChanged, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _isCheatEnabled, CheatEnabledChanged, reader.ReadBool());
		}
	}
}
