using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyServiceUGS : LobbyServiceProvider
{
	private LobbyInstanceUnity _currentLobby;

	private float _lastGetJoinedLobbiesAsyncTime = float.NegativeInfinity;

	public string playerId => AuthenticationService.Instance.PlayerId;

	public override LobbyInstance currentLobby => _currentLobby;

	public override async UniTask CreateLobby()
	{
		await ManagerBase<UGSManager>.instance.EnsureReady();
		await LeaveLobby();
		CreateLobbyOptions options = new CreateLobbyOptions();
		options.IsPrivate = GetInitialAttr_isInviteOnly();
		options.Data = new Dictionary<string, DataObject>();
		AddData(options.Data, "version", GetInitialAttr_version());
		AddData(options.Data, "hostAddress", GetInitialAttr_hostAddress());
		AddData(options.Data, "hasGameStarted", GetInitialAttr_hasGameStarted());
		AddData(options.Data, "difficulty", GetInitialAttr_difficulty());
		Debug.Log("Creating UGS lobby");
		ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.CreatingLobby);
		Lobby ugsLobby = await LobbyService.Instance.CreateLobbyAsync(GetInitialAttr_name(), GetInitialAttr_maxPlayers(), options);
		_currentLobby = new LobbyInstanceUnity
		{
			internalInstance = ugsLobby,
			isLobbyLeader = true
		};
		Debug.Log($"Created UGS Lobby: {currentLobby}");
		try
		{
			await RegisterCallbacksToCurrentLobby();
		}
		catch (Exception)
		{
			Debug.Log("UGS callback register failed");
			await LeaveLobby();
			throw;
		}
		InvokeOnCurrentLobbyChanged();
		StopAllCoroutines();
		StartCoroutine(Heartbeat());
		IEnumerator Heartbeat()
		{
			while (currentLobby != null && currentLobby.isLobbyLeader)
			{
				LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.id);
				yield return new WaitForSecondsRealtime(15f);
			}
		}
	}

	public override async UniTask JoinLobby(object lobby)
	{
		if (!(lobby is string id))
		{
			throw new ArgumentException("lobby");
		}
		try
		{
			await ManagerBase<UGSManager>.instance.EnsureReady();
			await LeaveLobby();
			string id2 = id.Trim();
			ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.ConnectingToLobby);
			Lobby ugs;
			if (id2.Length < 8)
			{
				string code = id2.Replace(" ", "");
				ugs = await LobbyService.Instance.JoinLobbyByCodeAsync(code);
			}
			else
			{
				ugs = await LobbyService.Instance.JoinLobbyByIdAsync(id2);
			}
			_currentLobby = new LobbyInstanceUnity
			{
				isLobbyLeader = false,
				internalInstance = ugs
			};
			InvokeOnCurrentLobbyChanged();
			ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.PreparingLobbyClient);
			try
			{
				await RegisterCallbacksToCurrentLobby();
			}
			catch (Exception)
			{
				Debug.Log("UGS callback register failed");
				await LeaveLobby();
				throw;
			}
		}
		catch (Exception ex2)
		{
			if (ex2.ToString().Contains("HTTP/1.1 404"))
			{
				throw new DewException(DewExceptionType.LobbyNotFound);
			}
			throw;
		}
	}

	public override async UniTask LeaveLobby()
	{
		await ManagerBase<UGSManager>.instance.EnsureReady();
		ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.CheckingLobbyStatus);
		if (currentLobby == null)
		{
			if (Time.realtimeSinceStartup - _lastGetJoinedLobbiesAsyncTime < 31f)
			{
				Debug.Log("Force leave UGS lobby is rate limited. Skipping...");
				return;
			}
			try
			{
				_lastGetJoinedLobbiesAsyncTime = Time.realtimeSinceStartup;
				Debug.Log("Getting previous UGS lobbies");
				List<string> lobbyIds = await LobbyService.Instance.GetJoinedLobbiesAsync();
				if (lobbyIds.Count > 0)
				{
					Debug.Log($"Found {lobbyIds.Count} leftover connected UGS lobbies");
					ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.LeavingPreviousLobby);
					foreach (string id in lobbyIds)
					{
						Debug.Log("Leaving UGS lobby: " + id);
						await LobbyService.Instance.RemovePlayerAsync(id, playerId);
					}
				}
				else
				{
					Debug.Log("No previous UGS lobbies were found");
				}
				return;
			}
			catch (Exception message)
			{
				Debug.Log("Leave UGS lobby failed");
				Debug.Log(message);
				return;
			}
		}
		try
		{
			if (currentLobby.isLobbyLeader)
			{
				Debug.Log("Deleting UGS lobby");
				ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.CleaningUpPreviousLobby);
				await LobbyService.Instance.DeleteLobbyAsync(currentLobby.id);
			}
			else
			{
				Debug.Log("Leaving UGS lobby");
				ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.LeavingPreviousLobby);
				await LobbyService.Instance.RemovePlayerAsync(currentLobby.id, playerId);
			}
		}
		catch (Exception message2)
		{
			Debug.Log("Leave lobby failed");
			Debug.Log(message2);
		}
		Debug.Log("UGS Lobby left; LeaveLobby()");
		_currentLobby = null;
		InvokeOnCurrentLobbyChanged();
	}

	public override async UniTask HandleUserLeavingGame(string id)
	{
	}

	public override async UniTask GetLobbies(Action<LobbySearchResult> onUpdated, object continuationToken = null)
	{
		await ManagerBase<UGSManager>.instance.EnsureReady();
		QueryLobbiesOptions options = new QueryLobbiesOptions
		{
			Filters = new List<QueryFilter>
			{
				GetEqualQuery("hasGameStarted", "0"),
				GetEqualQuery("version", Dew.GetCurrentMultiplayerCompatibilityVersion())
			},
			ContinuationToken = (string)continuationToken
		};
		QueryResponse result = await Lobbies.Instance.QueryLobbiesAsync(options);
		List<LobbyInstance> list = new List<LobbyInstance>();
		foreach (Lobby l in result.Results)
		{
			list.Add(new LobbyInstanceUnity
			{
				isLobbyLeader = false,
				internalInstance = l
			});
		}
		LobbySearchResult res = new LobbySearchResult
		{
			lobbies = list,
			continuationToken = result.ContinuationToken
		};
		onUpdated?.Invoke(res);
	}

	public override async UniTask SetLobbyAttribute(string key, object value)
	{
		if (_currentLobby != null && _currentLobby.isLobbyLeader)
		{
			UpdateLobbyOptions options = new UpdateLobbyOptions
			{
				Data = new Dictionary<string, DataObject>()
			};
			AddData(value: (!(value is int i)) ? ((!(value is float f)) ? ((!(value is double d)) ? ((value is bool) ? (((bool)value) ? "1" : "0") : ((!(value is string str)) ? value.ToString() : str)) : d.ToString(CultureInfo.InvariantCulture)) : f.ToString(CultureInfo.InvariantCulture)) : i.ToString(CultureInfo.InvariantCulture), dict: options.Data, key: key);
			await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.internalInstance.Id, options);
			_currentLobby.internalInstance.Data[key] = options.Data[key];
			InvokeOnCurrentLobbyChanged();
		}
	}

	private async UniTask RegisterCallbacksToCurrentLobby()
	{
		if (currentLobby == null)
		{
			return;
		}
		Debug.Log("Registering callbacks");
		ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.PreparingLobbyClient);
		LobbyEventCallbacks callbacks = new LobbyEventCallbacks();
		callbacks.LobbyChanged += delegate(ILobbyChanges changes)
		{
			if (changes.LobbyDeleted)
			{
				if (currentLobby != null)
				{
					Debug.Log("UGS Lobby left; Lobby has been deleted");
					_currentLobby = null;
					InvokeOnCurrentLobbyChanged();
				}
			}
			else
			{
				changes.ApplyToLobby(_currentLobby.internalInstance);
			}
		};
		callbacks.DataChanged += delegate(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> values)
		{
			if (currentLobby == null)
			{
				return;
			}
			Lobby internalInstance = _currentLobby.internalInstance;
			foreach (KeyValuePair<string, ChangedOrRemovedLobbyValue<DataObject>> current in values)
			{
				switch (current.Value.ChangeType)
				{
				case LobbyValueChangeType.Changed:
					internalInstance.Data[current.Key] = current.Value.Value;
					break;
				case LobbyValueChangeType.Removed:
					internalInstance.Data.Remove(current.Key);
					break;
				case LobbyValueChangeType.Added:
					internalInstance.Data[current.Key] = current.Value.Value;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				case LobbyValueChangeType.Unchanged:
					break;
				}
			}
		};
		callbacks.KickedFromLobby += delegate
		{
			if (currentLobby != null)
			{
				Debug.Log("UGS Lobby left; Kicked from Lobby");
				_currentLobby = null;
				InvokeOnCurrentLobbyChanged();
			}
		};
		await Lobbies.Instance.SubscribeToLobbyEventsAsync(_currentLobby.internalInstance.Id, callbacks).AsUniTask().Timeout(TimeSpan.FromSeconds(8.0));
	}

	private void AddData(Dictionary<string, DataObject> dict, string key, string value)
	{
		dict.Add(key, GetDataObject(key, value));
	}

	private void AddData(Dictionary<string, DataObject> dict, string key, bool value)
	{
		dict.Add(key, GetDataObject(key, value ? "1" : "0"));
	}

	private DataObject GetDataObject(string key, string value)
	{
		DataObject.VisibilityOptions vis = GetVisibilityOption(key);
		DataObject.IndexOptions? index = GetIndexOption(key);
		if (!index.HasValue)
		{
			return new DataObject(vis, value);
		}
		return new DataObject(vis, value, index.Value);
	}

	private QueryFilter GetEqualQuery(string key, string value)
	{
		return new QueryFilter(GetFieldOption(key), value, QueryFilter.OpOptions.EQ);
	}

	private DataObject.VisibilityOptions GetVisibilityOption(string key)
	{
		switch (key)
		{
		case "difficulty":
		case "version":
		case "hasGameStarted":
			return DataObject.VisibilityOptions.Public;
		case "hostAddress":
			return DataObject.VisibilityOptions.Member;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private DataObject.IndexOptions? GetIndexOption(string key)
	{
		return key switch
		{
			"difficulty" => DataObject.IndexOptions.S2, 
			"version" => DataObject.IndexOptions.S1, 
			"hasGameStarted" => DataObject.IndexOptions.N1, 
			"hostAddress" => null, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private QueryFilter.FieldOptions GetFieldOption(string key)
	{
		DataObject.IndexOptions? index = GetIndexOption(key);
		if (!index.HasValue)
		{
			throw new ArgumentOutOfRangeException();
		}
		return index.Value switch
		{
			DataObject.IndexOptions.S1 => QueryFilter.FieldOptions.S1, 
			DataObject.IndexOptions.S2 => QueryFilter.FieldOptions.S2, 
			DataObject.IndexOptions.S3 => QueryFilter.FieldOptions.S3, 
			DataObject.IndexOptions.S4 => QueryFilter.FieldOptions.S4, 
			DataObject.IndexOptions.S5 => QueryFilter.FieldOptions.S5, 
			DataObject.IndexOptions.N1 => QueryFilter.FieldOptions.N1, 
			DataObject.IndexOptions.N2 => QueryFilter.FieldOptions.N2, 
			DataObject.IndexOptions.N3 => QueryFilter.FieldOptions.N3, 
			DataObject.IndexOptions.N4 => QueryFilter.FieldOptions.N4, 
			DataObject.IndexOptions.N5 => QueryFilter.FieldOptions.N5, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
