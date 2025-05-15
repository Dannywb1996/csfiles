using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using EpicTransport;
using UnityEngine;

public class LobbyServiceEOS : LobbyServiceProvider
{
	public const int ShortLobbyIdLength = 8;

	public readonly string[] AttributeKeys = new string[7] { "version", "hasGameStarted", "isInviteOnly", "name", "difficulty", "hostAddress", "shortCode" };

	private LobbyInstanceEpic _currentLobby;

	private ulong _lobbyMemberStatusNotifyId;

	private ulong _lobbyAttributeUpdateNotifyId;

	private bool _didWaitForSteam;

	public override LobbyInstance currentLobby => _currentLobby;

	private async void Start()
	{
		await UniTask.WaitWhile(() => !EOSSDKComponent.Initialized);
		AddNotifyLobbyMemberStatusReceivedOptions addNotifyLobbyMemberStatusReceivedOptions = default(AddNotifyLobbyMemberStatusReceivedOptions);
		_lobbyMemberStatusNotifyId = EOSSDKComponent.GetLobbyInterface().AddNotifyLobbyMemberStatusReceived(ref addNotifyLobbyMemberStatusReceivedOptions, null, delegate(ref LobbyMemberStatusReceivedCallbackInfo callback)
		{
			if (_currentLobby != null)
			{
				_currentLobby.UpdateMemberList();
			}
			if (callback.CurrentStatus == LobbyMemberStatus.Closed)
			{
				LeaveLobby();
			}
		});
		AddNotifyLobbyUpdateReceivedOptions addNotifyLobbyUpdateReceivedOptions = default(AddNotifyLobbyUpdateReceivedOptions);
		_lobbyAttributeUpdateNotifyId = EOSSDKComponent.GetLobbyInterface().AddNotifyLobbyUpdateReceived(ref addNotifyLobbyUpdateReceivedOptions, null, delegate
		{
			if (_currentLobby != null)
			{
				SetCurrentLobbyData(new LobbyInstanceEpic().ApplyFromHandle(_currentLobby.details));
			}
		});
		Debug.Log("Added notifications to EOS LobbyInterface");
	}

	private void OnDestroy()
	{
		if (!(EOSSDKComponent.Instance == null) && !(EOSSDKComponent.Instance.EOS == null))
		{
			EOSSDKComponent.GetLobbyInterface().RemoveNotifyLobbyMemberStatusReceived(_lobbyMemberStatusNotifyId);
			EOSSDKComponent.GetLobbyInterface().RemoveNotifyLobbyUpdateReceived(_lobbyAttributeUpdateNotifyId);
		}
	}

	private async UniTask EnsureInitialized()
	{
		if (EOSSDKComponent.IsConnecting || !EOSSDKComponent.Initialized)
		{
			Debug.Log("Waiting for EOS");
			await UniTask.WaitWhile(() => EOSSDKComponent.IsConnecting).Timeout(TimeSpan.FromSeconds(10.0));
			if (!EOSSDKComponent.Initialized)
			{
				throw new Exception("EOS is not available");
			}
		}
	}

	public override async UniTask CreateLobby()
	{
		await EnsureInitialized();
		await LeaveLobby();
		LobbyPermissionLevel vis = LobbyPermissionLevel.Publicadvertised;
		CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
		{
			LocalUserId = EOSSDKComponent.LocalUserProductId,
			MaxLobbyMembers = (uint)GetInitialAttr_maxPlayers(),
			PermissionLevel = vis,
			PresenceEnabled = false,
			BucketId = "default"
		};
		UniTaskCompletionSource task = new UniTaskCompletionSource();
		ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.CreatingLobby);
		EOSSDKComponent.GetLobbyInterface().CreateLobby(ref createLobbyOptions, null, delegate(ref CreateLobbyCallbackInfo callback)
		{
			try
			{
				List<Epic.OnlineServices.Lobby.Attribute> lobbyReturnData;
				LobbyModification modHandle;
				if (callback.ResultCode != Result.Success)
				{
					task.TrySetException(new EOSResultException(callback.ResultCode));
				}
				else
				{
					lobbyReturnData = new List<Epic.OnlineServices.Lobby.Attribute>();
					modHandle = new LobbyModification();
					AttributeData value = new AttributeData
					{
						Key = "default",
						Value = "default"
					};
					UpdateLobbyModificationOptions options = new UpdateLobbyModificationOptions
					{
						LobbyId = callback.LobbyId,
						LocalUserId = EOSSDKComponent.LocalUserProductId
					};
					EOSSDKComponent.GetLobbyInterface().UpdateLobbyModification(ref options, out modHandle);
					LobbyModificationAddAttributeOptions options2 = new LobbyModificationAddAttributeOptions
					{
						Attribute = value,
						Visibility = LobbyAttributeVisibility.Public
					};
					modHandle.AddAttribute(ref options2);
					AddAttr("version", GetInitialAttr_version());
					AddAttr("hostAddress", GetInitialAttr_hostAddress());
					AddAttr("hasGameStarted", GetInitialAttr_hasGameStarted());
					AddAttr("difficulty", GetInitialAttr_difficulty());
					AddAttr("maxPlayers", GetInitialAttr_maxPlayers());
					AddAttr("name", GetInitialAttr_name());
					AddAttr("isInviteOnly", GetInitialAttr_isInviteOnly());
					Utf8String lobbyId = callback.LobbyId;
					ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.PreparingLobby);
					UpdateLobbyOptions options3 = new UpdateLobbyOptions
					{
						LobbyModificationHandle = modHandle
					};
					EOSSDKComponent.GetLobbyInterface().UpdateLobby(ref options3, null, delegate(ref UpdateLobbyCallbackInfo updateCallback)
					{
						try
						{
							if (updateCallback.ResultCode != Result.Success)
							{
								task.TrySetException(new EOSResultException(updateCallback.ResultCode));
							}
							else
							{
								CopyLobbyDetailsHandleOptions options4 = new CopyLobbyDetailsHandleOptions
								{
									LobbyId = lobbyId,
									LocalUserId = EOSSDKComponent.LocalUserProductId
								};
								EOSSDKComponent.GetLobbyInterface().CopyLobbyDetailsHandle(ref options4, out var outLobbyDetailsHandle);
								LobbyInstanceEpic lobbyInstanceEpic = new LobbyInstanceEpic();
								lobbyInstanceEpic.ApplyFromHandle(outLobbyDetailsHandle);
								lobbyInstanceEpic.isLobbyLeader = true;
								SetCurrentLobbyData(lobbyInstanceEpic);
								SetLobbyShortId();
								task.TrySetResult();
							}
						}
						catch (Exception exception2)
						{
							task.TrySetException(exception2);
						}
					});
				}
				void AddAttr(string key, object o)
				{
					LobbyModificationAddAttributeOptions options4 = new LobbyModificationAddAttributeOptions
					{
						Attribute = new AttributeData
						{
							Key = key,
							Value = o.ToAttrDataValue()
						},
						Visibility = LobbyAttributeVisibility.Public
					};
					modHandle.AddAttribute(ref options4);
					lobbyReturnData.Add(new Epic.OnlineServices.Lobby.Attribute
					{
						Data = options4.Attribute,
						Visibility = LobbyAttributeVisibility.Public
					});
				}
			}
			catch (Exception exception)
			{
				task.TrySetException(exception);
			}
		});
		await task.Task;
		Debug.Log($"Created EOS Lobby: {currentLobby}");
		StopAllCoroutines();
		StartCoroutine(Heartbeat());
		IEnumerator Heartbeat()
		{
			while (currentLobby != null && currentLobby.isLobbyLeader)
			{
				SetLobbyAttribute("heartbeat", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
				yield return new WaitForSecondsRealtime(15f);
			}
		}
	}

	public override async UniTask JoinLobby(object lobby)
	{
		LobbyDetails details;
		if (lobby is LobbyDetails d)
		{
			details = d;
		}
		else
		{
			if (!(lobby is string id))
			{
				throw new DewException(DewExceptionType.LobbyNotFound);
			}
			details = await GetLobbyById(id);
		}
		await EnsureInitialized();
		await LeaveLobby();
		JoinLobbyOptions joinLobbyOptions = new JoinLobbyOptions
		{
			LobbyDetailsHandle = details,
			LocalUserId = EOSSDKComponent.LocalUserProductId,
			PresenceEnabled = false
		};
		ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.ConnectingToLobby);
		UniTaskCompletionSource task = new UniTaskCompletionSource();
		EOSSDKComponent.GetLobbyInterface().JoinLobby(ref joinLobbyOptions, null, delegate(ref JoinLobbyCallbackInfo callback)
		{
			try
			{
				if (callback.ResultCode != Result.Success)
				{
					task.TrySetException(new EOSResultException(callback.ResultCode));
				}
				else
				{
					CopyLobbyDetailsHandleOptions options = new CopyLobbyDetailsHandleOptions
					{
						LobbyId = callback.LobbyId,
						LocalUserId = EOSSDKComponent.LocalUserProductId
					};
					EOSSDKComponent.GetLobbyInterface().CopyLobbyDetailsHandle(ref options, out var outLobbyDetailsHandle);
					LobbyInstanceEpic lobbyInstanceEpic = new LobbyInstanceEpic();
					lobbyInstanceEpic.ApplyFromHandle(outLobbyDetailsHandle);
					lobbyInstanceEpic.isLobbyLeader = false;
					SetCurrentLobbyData(lobbyInstanceEpic);
					task.TrySetResult();
				}
			}
			catch (Exception exception)
			{
				task.TrySetException(exception);
			}
		});
		await task.Task;
	}

	public override async UniTask LeaveLobby()
	{
		if (currentLobby == null)
		{
			return;
		}
		await EnsureInitialized();
		UniTaskCompletionSource task = new UniTaskCompletionSource();
		if (currentLobby.isLobbyLeader)
		{
			Debug.Log("Deleting EOS lobby");
			ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.CleaningUpPreviousLobby);
			DestroyLobbyOptions destroyLobbyOptions = new DestroyLobbyOptions
			{
				LobbyId = currentLobby.id,
				LocalUserId = EOSSDKComponent.LocalUserProductId
			};
			EOSSDKComponent.GetLobbyInterface().DestroyLobby(ref destroyLobbyOptions, null, delegate(ref DestroyLobbyCallbackInfo callback)
			{
				if (callback.ResultCode != Result.Success)
				{
					task.TrySetException(new EOSResultException(callback.ResultCode));
				}
				else
				{
					task.TrySetResult();
				}
			});
			SetCurrentLobbyData(null);
		}
		else
		{
			Debug.Log("Leaving EOS lobby");
			ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.LeavingPreviousLobby);
			LeaveLobbyOptions leaveLobbyOptions = new LeaveLobbyOptions
			{
				LobbyId = currentLobby.id,
				LocalUserId = EOSSDKComponent.LocalUserProductId
			};
			EOSSDKComponent.GetLobbyInterface().LeaveLobby(ref leaveLobbyOptions, null, delegate(ref LeaveLobbyCallbackInfo callback)
			{
				if (callback.ResultCode != Result.Success && callback.ResultCode != Result.NotFound)
				{
					task.TrySetException(new EOSResultException(callback.ResultCode));
				}
				else
				{
					task.TrySetResult();
				}
			});
			SetCurrentLobbyData(null);
		}
		try
		{
			await task.Task;
		}
		catch (Exception message)
		{
			Debug.Log("Leave EOS lobby failed");
			Debug.Log(message);
		}
		Debug.Log("EOS Lobby left; LeaveLobby()");
	}

	public override async UniTask HandleUserLeavingGame(string id)
	{
		if (_currentLobby == null || _currentLobby.lobbyMembers.Find((EpicLobbyMember e) => e.handle.ToString() == id) == null)
		{
			return;
		}
		KickMemberOptions opt = new KickMemberOptions
		{
			LobbyId = _currentLobby.id,
			LocalUserId = EOSSDKComponent.LocalUserProductId,
			TargetUserId = ProductUserId.FromString(id)
		};
		Debug.Log("Kicking user " + id);
		EOSSDKComponent.GetLobbyInterface().KickMember(ref opt, null, delegate(ref KickMemberCallbackInfo data)
		{
			if (data.ResultCode != Result.Success)
			{
				Debug.Log("Failed to kick user " + id + " from EOS lobby: " + data.ResultCode);
			}
			else
			{
				Debug.Log("Kicked user " + id + " from EOS lobby");
			}
		});
	}

	public override async UniTask GetLobbies(Action<LobbySearchResult> onUpdated, object continuationToken = null)
	{
		await EnsureInitialized();
		uint maxResults = 100u;
		LobbySearchSetParameterOptions[] obj = new LobbySearchSetParameterOptions[4]
		{
			new LobbySearchSetParameterOptions
			{
				Parameter = new AttributeData
				{
					Key = "version",
					Value = Dew.GetCurrentMultiplayerCompatibilityVersion()
				},
				ComparisonOp = ComparisonOp.Equal
			},
			new LobbySearchSetParameterOptions
			{
				Parameter = new AttributeData
				{
					Key = "hasGameStarted",
					Value = false
				},
				ComparisonOp = ComparisonOp.Equal
			},
			new LobbySearchSetParameterOptions
			{
				Parameter = new AttributeData
				{
					Key = "isInviteOnly",
					Value = false
				},
				ComparisonOp = ComparisonOp.Equal
			},
			new LobbySearchSetParameterOptions
			{
				Parameter = new AttributeData
				{
					Key = "heartbeat",
					Value = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 30
				},
				ComparisonOp = ComparisonOp.Greaterthan
			}
		};
		LobbySearch search = new LobbySearch();
		CreateLobbySearchOptions createLobbySearchOptions = new CreateLobbySearchOptions
		{
			MaxResults = maxResults
		};
		EOSSDKComponent.GetLobbyInterface().CreateLobbySearch(ref createLobbySearchOptions, out search);
		LobbySearchSetParameterOptions[] array = obj;
		for (int i = 0; i < array.Length; i++)
		{
			LobbySearchSetParameterOptions option = array[i];
			search.SetParameter(ref option);
		}
		LobbySearchFindOptions findOptions = new LobbySearchFindOptions
		{
			LocalUserId = EOSSDKComponent.LocalUserProductId
		};
		UniTaskCompletionSource<LobbySearchResult> task = new UniTaskCompletionSource<LobbySearchResult>();
		search.Find(ref findOptions, null, delegate(ref LobbySearchFindCallbackInfo callback)
		{
			try
			{
				if (callback.ResultCode != Result.Success)
				{
					task.TrySetException(new EOSResultException(callback.ResultCode));
				}
				else
				{
					LobbySearchResult lobbySearchResult = new LobbySearchResult();
					LobbySearchGetSearchResultCountOptions options = default(LobbySearchGetSearchResultCountOptions);
					for (int j = 0; j < search.GetSearchResultCount(ref options); j++)
					{
						LobbySearchCopySearchResultByIndexOptions options2 = new LobbySearchCopySearchResultByIndexOptions
						{
							LobbyIndex = (uint)j
						};
						search.CopySearchResultByIndex(ref options2, out var outLobbyDetailsHandle);
						LobbyInstanceEpic lobbyInstanceEpic = new LobbyInstanceEpic().ApplyFromHandle(outLobbyDetailsHandle);
						if (lobbyInstanceEpic.currentPlayers < lobbyInstanceEpic.maxPlayers && lobbyInstanceEpic.currentPlayers > 0 && lobbyInstanceEpic.name.Length > 0)
						{
							LobbyDetailsGetLobbyOwnerOptions options3 = default(LobbyDetailsGetLobbyOwnerOptions);
							if (!(outLobbyDetailsHandle.GetLobbyOwner(ref options3) == EOSSDKComponent.LocalUserProductId))
							{
								lobbySearchResult.lobbies.Add(lobbyInstanceEpic);
							}
						}
					}
					task.TrySetResult(lobbySearchResult);
				}
			}
			catch (Exception exception)
			{
				task.TrySetException(exception);
			}
		});
		LobbySearchResult res = await task.Task.Timeout(TimeSpan.FromSeconds(15.0));
		onUpdated?.Invoke(res);
	}

	public override async UniTask SetLobbyAttribute(string key, object value)
	{
		if (currentLobby == null)
		{
			return;
		}
		UniTaskCompletionSource task = new UniTaskCompletionSource();
		LobbyModification modHandle = new LobbyModification();
		UpdateLobbyModificationOptions updateLobbyModificationOptions = new UpdateLobbyModificationOptions
		{
			LobbyId = currentLobby.id,
			LocalUserId = EOSSDKComponent.LocalUserProductId
		};
		EOSSDKComponent.GetLobbyInterface().UpdateLobbyModification(ref updateLobbyModificationOptions, out modHandle);
		LobbyModificationAddAttributeOptions options = new LobbyModificationAddAttributeOptions
		{
			Attribute = new AttributeData
			{
				Key = key,
				Value = value.ToAttrDataValue()
			},
			Visibility = LobbyAttributeVisibility.Public
		};
		modHandle.AddAttribute(ref options);
		UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
		{
			LobbyModificationHandle = modHandle
		};
		EOSSDKComponent.GetLobbyInterface().UpdateLobby(ref updateLobbyOptions, null, delegate(ref UpdateLobbyCallbackInfo callback)
		{
			if (callback.ResultCode != Result.Success)
			{
				task.TrySetException(new EOSResultException(callback.ResultCode));
			}
			else
			{
				SetCurrentLobbyData(new LobbyInstanceEpic().ApplyFromHandle(_currentLobby.details));
				task.TrySetResult();
			}
		});
		await task.Task;
	}

	private async UniTask SetLobbyShortId()
	{
		if (_currentLobby != null)
		{
			string hash = GenerateHash(currentLobby.id);
			await SetLobbyAttribute("shortCode", hash);
		}
		static string GenerateHash(string input)
		{
			char[] availableCharacters = "23456789abcdefghjklmnpqrstuvwxyz".ToCharArray();
			using MD5 md5 = MD5.Create();
			byte[] inputBytes = Encoding.UTF8.GetBytes(input);
			byte[] hashBytes = md5.ComputeHash(inputBytes);
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < 8; i++)
			{
				int index = hashBytes[i] % availableCharacters.Length;
				char character = availableCharacters[index];
				sb.Append(character);
			}
			return sb.ToString();
		}
	}

	private async UniTask<LobbyDetails> GetLobbyById(string id)
	{
		await EnsureInitialized();
		ManagerBase<TransitionManager>.instance.UpdateLoadingStatus(LoadingStatus.GettingLobbyInformation);
		CreateLobbySearchOptions opt = new CreateLobbySearchOptions
		{
			MaxResults = 1u
		};
		EOSSDKComponent.GetLobbyInterface().CreateLobbySearch(ref opt, out var search);
		string shortId = id.Trim().Replace(" ", "").ToLower();
		if (shortId.Length == 8)
		{
			Debug.Log("Starting lobby search via short code: " + shortId);
			LobbySearchSetParameterOptions param = new LobbySearchSetParameterOptions
			{
				Parameter = new AttributeData
				{
					Key = "shortCode",
					Value = shortId
				},
				ComparisonOp = ComparisonOp.Equal
			};
			search.SetParameter(ref param);
		}
		else
		{
			Debug.Log("Starting lobby search via id: " + id);
			LobbySearchSetLobbyIdOptions param2 = new LobbySearchSetLobbyIdOptions
			{
				LobbyId = id
			};
			search.SetLobbyId(ref param2);
		}
		UniTaskCompletionSource<LobbyDetails> task = new UniTaskCompletionSource<LobbyDetails>();
		LobbySearchFindOptions opt2 = new LobbySearchFindOptions
		{
			LocalUserId = EOSSDKComponent.LocalUserProductId
		};
		search.Find(ref opt2, null, delegate(ref LobbySearchFindCallbackInfo callback)
		{
			if (callback.ResultCode != Result.Success)
			{
				task.TrySetException(new EOSResultException(callback.ResultCode));
			}
			LobbySearchGetSearchResultCountOptions options = default(LobbySearchGetSearchResultCountOptions);
			if (search.GetSearchResultCount(ref options) == 0)
			{
				task.TrySetException(new EOSResultException(Result.NotFound));
			}
			else
			{
				LobbySearchCopySearchResultByIndexOptions options2 = new LobbySearchCopySearchResultByIndexOptions
				{
					LobbyIndex = 0u
				};
				search.CopySearchResultByIndex(ref options2, out var outLobbyDetailsHandle);
				task.TrySetResult(outLobbyDetailsHandle);
			}
		});
		return await task.Task;
	}

	private bool TryGetAttribute(List<Epic.OnlineServices.Lobby.Attribute> list, string key, out Epic.OnlineServices.Lobby.Attribute attr)
	{
		attr = list.Find((Epic.OnlineServices.Lobby.Attribute x) => x.Data.HasValue && x.Data.Value.Key == (Utf8String)key);
		return attr.Data.HasValue;
	}

	private void SetCurrentLobbyData(LobbyInstanceEpic data)
	{
		if (_currentLobby != data)
		{
			_currentLobby = data;
			InvokeOnCurrentLobbyChanged();
		}
	}
}
