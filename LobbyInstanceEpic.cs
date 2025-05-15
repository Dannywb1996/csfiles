using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using EpicTransport;
using UnityEngine;

public class LobbyInstanceEpic : LobbyInstance
{
	public LobbyDetails details;

	public List<EpicLobbyMember> lobbyMembers = new List<EpicLobbyMember>();

	private string _id;

	private string _name;

	private string _difficulty;

	private bool _hasGameStarted;

	internal int _currentPlayers;

	private int _maxPlayers;

	private string _version;

	private string _hostAddress;

	private bool _isInviteOnly;

	private string _shortCode;

	public override string id => _id;

	public override string name => _name;

	public override string difficulty => _difficulty;

	public override bool hasGameStarted => _hasGameStarted;

	public override int currentPlayers => _currentPlayers;

	public override int maxPlayers => _maxPlayers;

	public override string version => _version;

	public override string hostAddress => _hostAddress;

	public override bool isInviteOnly => _isInviteOnly;

	public override string shortCode => _shortCode;

	public override LobbyConnectionQuality connectionQuality => LobbyConnectionQuality.Unknown;

	public LobbyInstanceEpic ApplyFromHandle(LobbyDetails h)
	{
		details = h;
		try
		{
			LobbyDetailsCopyInfoOptions opt = default(LobbyDetailsCopyInfoOptions);
			h.CopyInfo(ref opt, out var info);
			if (info.HasValue)
			{
				_id = info.Value.LobbyId;
				_maxPlayers = (int)info.Value.MaxMembers;
				isLobbyLeader = info.Value.LobbyOwnerUserId == EOSSDKComponent.LocalUserProductId;
			}
			_name = h.GetAttributeString("name").Trim();
			_version = h.GetAttributeString("version");
			_difficulty = h.GetAttributeString("difficulty");
			_hasGameStarted = h.GetAttributeBool("hasGameStarted");
			_hostAddress = h.GetAttributeString("hostAddress");
			_shortCode = h.GetAttributeString("shortCode");
			LobbyDetailsGetMemberCountOptions opt2 = default(LobbyDetailsGetMemberCountOptions);
			_currentPlayers = (int)h.GetMemberCount(ref opt2);
			_isInviteOnly = h.GetAttributeBool("isInviteOnly");
			UpdateMemberList();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return this;
	}

	public void UpdateMemberList()
	{
		lobbyMembers.Clear();
		LobbyDetailsGetMemberCountOptions o0 = default(LobbyDetailsGetMemberCountOptions);
		uint maxCount = details.GetMemberCount(ref o0);
		for (uint i = 0u; i < maxCount; i++)
		{
			LobbyDetailsGetMemberByIndexOptions o1 = new LobbyDetailsGetMemberByIndexOptions
			{
				MemberIndex = i
			};
			ProductUserId member = details.GetMemberByIndex(ref o1);
			if (!(member == null))
			{
				lobbyMembers.Add(new EpicLobbyMember
				{
					handle = member
				});
			}
		}
		_currentPlayers = (int)maxCount;
	}
}
