using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Mirror;
using Mirror.RemoteCalls;
using Steamworks;
using UnityEngine;

[DewResourceLink(ResourceLinkBy.Type)]
public class DewPlayer : DewNetworkBehaviour
{
	private enum Role : byte
	{
		None,
		Environment,
		Creep
	}

	private static Texture2D DefaultAvatar;

	private static List<DewPlayer> _humanPlayers;

	public readonly SyncList<DewPlayer> allies = new SyncList<DewPlayer>();

	public readonly SyncList<DewPlayer> enemies = new SyncList<DewPlayer>();

	public readonly SyncList<DewPlayer> neutrals = new SyncList<DewPlayer>();

	[NonSerialized]
	public List<string> lucidDreams = new List<string>();

	[SyncVar]
	[SerializeField]
	private Role _role;

	[SyncVar(hook = "OnPlayerNameChanged")]
	private string _playerName = "Dreamer";

	[SyncVar(hook = "OnEquippedNametagChanged")]
	private string _equippedNametag;

	public SafeAction<string, string> ClientEvent_OnEquippedNametagChanged;

	[SyncVar]
	private bool _isHostPlayer;

	[SyncVar]
	private Texture2D _avatar;

	[SyncVar(hook = "SelectedEntityChanged")]
	private Entity _controllingEntity;

	[CompilerGenerated]
	[SyncVar(hook = "OnHeroChanged")]
	private Hero _003Chero_003Ek__BackingField;

	public SafeAction<Hero, Hero> ClientEvent_OnHeroChanged;

	[CompilerGenerated]
	[SyncVar(hook = "OnGoldChanged")]
	private int _003Cgold_003Ek__BackingField;

	public SafeAction<int, int> ClientEvent_OnGoldChanged;

	public SafeAction<int> ClientEvent_OnSpendGold;

	public SafeAction<int> ClientEvent_OnEarnGold;

	[CompilerGenerated]
	[SyncVar(hook = "OnDreamDustChanged")]
	private int _003CdreamDust_003Ek__BackingField;

	public SafeAction<int, int> ClientEvent_OnDreamDustChanged;

	public SafeAction<int> ClientEvent_OnSpendDreamDust;

	public SafeAction<int> ClientEvent_OnEarnDreamDust;

	public SafeAction<int> ClientEvent_OnEarnStardust;

	[CompilerGenerated]
	[SyncVar(hook = "OnJonasTokenChanged")]
	private int _003CjonasToken_003Ek__BackingField;

	public SafeAction<int, int> ClientEvent_OnJonasTokenChanged;

	public SafeAction<int, int, DewPlayer> ClientEvent_OnGiveCurrency;

	[CompilerGenerated]
	[SyncVar]
	private float _003CcleanseRefundMultiplier_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private float _003CdismantleDreamDustMultiplier_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private float _003CsellPriceMultiplier_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private float _003CbuyPriceMultiplier_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int _003CshopAddedItems_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int _003CallowedShopRefreshes_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private float _003CpotionDropChanceMultiplier_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private float _003CdoubleChaosChance_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private bool _003CisReadingArtifactStory_003Ek__BackingField;

	public readonly SyncList<PlayerStarItem> stars = new SyncList<PlayerStarItem>();

	private int _nextOnScreenTimerId;

	private readonly List<NetworkedOnScreenTimerHandle> _onScreenTimersServer = new List<NetworkedOnScreenTimerHandle>();

	private readonly List<(int, OnScreenTimerHandle, RefValue<float>)> _onScreenTimersLocal = new List<(int, OnScreenTimerHandle, RefValue<float>)>();

	internal SampleCastInfoContext? _currentSampleContext;

	public readonly SyncList<string> ownershipKeys = new SyncList<string>();

	[NonSerialized]
	public List<string> ownedItems = new List<string>();

	public CSteamID steamId;

	[SyncVar(hook = "OnSteamIDChanged")]
	private CSteamID _steamId;

	[CompilerGenerated]
	[SyncVar(hook = "OnHeroTypeChanged")]
	private string _003CselectedHeroType_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnLoadoutChanged")]
	private HeroLoadoutData _003CselectedLoadout_003Ek__BackingField;

	public readonly SyncList<string> selectedAccessories = new SyncList<string>();

	[CompilerGenerated]
	[SyncVar(hook = "OnDejavuItemChanged")]
	private string _003CselectedDejavuItem_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar(hook = "OnIsReadyChanged")]
	private bool _003CisReady_003Ek__BackingField;

	public SafeAction<string> ClientEvent_OnSelectedHeroTypeChanged;

	public SafeAction<HeroLoadoutData> ClientEvent_OnSelectedLoadoutChanged;

	public SafeAction ClientEvent_OnSelectedAccessoriesChanged;

	public SafeAction<string> ClientEvent_OnSelectedDejavuItemChanged;

	public SafeAction<bool> ClientEvent_OnIsReadyChanged;

	protected NetworkBehaviourSyncVar ____controllingEntityNetId;

	protected NetworkBehaviourSyncVar ____003Chero_003Ek__BackingFieldNetId;

	public static DewPlayer creep { get; private set; }

	public static DewPlayer environment { get; private set; }

	public static DewPlayer local { get; private set; }

	public static IList<DewPlayer> humanPlayers => _humanPlayers;

	public bool isCreepPlayer => this == creep;

	public bool isEnvironmentPlayer => this == environment;

	public bool isHumanPlayer
	{
		get
		{
			if (!isCreepPlayer)
			{
				return !isEnvironmentPlayer;
			}
			return false;
		}
	}

	internal bool isPlayerNameSet { get; private set; }

	public string playerName => _playerName;

	public string equippedNametag => _equippedNametag;

	public bool isHostPlayer => _isHostPlayer;

	public Texture2D avatar
	{
		get
		{
			if (!(_avatar == null))
			{
				return _avatar;
			}
			return DefaultAvatar;
		}
	}

	public bool isKicked { get; private set; }

	public Entity controllingEntity
	{
		get
		{
			return Network_controllingEntity;
		}
		set
		{
			if (!base.isServer)
			{
				throw new Exception("Only server can change this.");
			}
			Network_controllingEntity = value;
		}
	}

	public Hero hero
	{
		[CompilerGenerated]
		get
		{
			return Network_003Chero_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Chero_003Ek__BackingField = value;
		}
	}

	public int gold
	{
		[CompilerGenerated]
		get
		{
			return _003Cgold_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003Cgold_003Ek__BackingField = value;
		}
	}

	public int dreamDust
	{
		[CompilerGenerated]
		get
		{
			return _003CdreamDust_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CdreamDust_003Ek__BackingField = value;
		}
	}

	public int jonasToken
	{
		[CompilerGenerated]
		get
		{
			return _003CjonasToken_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CjonasToken_003Ek__BackingField = value;
		}
	}

	public float cleanseRefundMultiplier
	{
		[CompilerGenerated]
		get
		{
			return _003CcleanseRefundMultiplier_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CcleanseRefundMultiplier_003Ek__BackingField = value;
		}
	} = 0.7f;

	public float dismantleDreamDustMultiplier
	{
		[CompilerGenerated]
		get
		{
			return _003CdismantleDreamDustMultiplier_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CdismantleDreamDustMultiplier_003Ek__BackingField = value;
		}
	} = 1f;

	public float sellPriceMultiplier
	{
		[CompilerGenerated]
		get
		{
			return _003CsellPriceMultiplier_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CsellPriceMultiplier_003Ek__BackingField = value;
		}
	} = 1f;

	public float buyPriceMultiplier
	{
		[CompilerGenerated]
		get
		{
			return _003CbuyPriceMultiplier_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CbuyPriceMultiplier_003Ek__BackingField = value;
		}
	} = 1f;

	public int shopAddedItems
	{
		[CompilerGenerated]
		get
		{
			return _003CshopAddedItems_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CshopAddedItems_003Ek__BackingField = value;
		}
	}

	public int allowedShopRefreshes
	{
		[CompilerGenerated]
		get
		{
			return _003CallowedShopRefreshes_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CallowedShopRefreshes_003Ek__BackingField = value;
		}
	}

	public float potionDropChanceMultiplier
	{
		[CompilerGenerated]
		get
		{
			return _003CpotionDropChanceMultiplier_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CpotionDropChanceMultiplier_003Ek__BackingField = value;
		}
	} = 1f;

	public float doubleChaosChance
	{
		[CompilerGenerated]
		get
		{
			return _003CdoubleChaosChance_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CdoubleChaosChance_003Ek__BackingField = value;
		}
	}

	public bool isReadingArtifactStory
	{
		[CompilerGenerated]
		get
		{
			return _003CisReadingArtifactStory_003Ek__BackingField;
		}
		[CompilerGenerated]
		internal set
		{
			Network_003CisReadingArtifactStory_003Ek__BackingField = value;
		}
	}

	public Vector3 cursorWorldPos { get; private set; }

	public InputMode inputMode { get; internal set; }

	public bool isGamepadExplicitAim { get; internal set; }

	public Entity gamepadTargetEnemy { get; internal set; }

	public float monsterKillGoldMultiplier { get; set; } = 1f;

	public string selectedHeroType
	{
		[CompilerGenerated]
		get
		{
			return _003CselectedHeroType_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CselectedHeroType_003Ek__BackingField = value;
		}
	} = "Hero_Lacerta";

	public HeroLoadoutData selectedLoadout
	{
		[CompilerGenerated]
		get
		{
			return _003CselectedLoadout_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CselectedLoadout_003Ek__BackingField = value;
		}
	} = new HeroLoadoutData();

	public string selectedDejavuItem
	{
		[CompilerGenerated]
		get
		{
			return _003CselectedDejavuItem_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Network_003CselectedDejavuItem_003Ek__BackingField = value;
		}
	}

	public bool isReady
	{
		[CompilerGenerated]
		get
		{
			return _003CisReady_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CisReady_003Ek__BackingField = value;
		}
	}

	public Role Network_role
	{
		get
		{
			return _role;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _role, 1uL, null);
		}
	}

	public string Network_playerName
	{
		get
		{
			return _playerName;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _playerName, 2uL, OnPlayerNameChanged);
		}
	}

	public string Network_equippedNametag
	{
		get
		{
			return _equippedNametag;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _equippedNametag, 4uL, OnEquippedNametagChanged);
		}
	}

	public bool Network_isHostPlayer
	{
		get
		{
			return _isHostPlayer;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _isHostPlayer, 8uL, null);
		}
	}

	public Texture2D Network_avatar
	{
		get
		{
			return _avatar;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _avatar, 16uL, null);
		}
	}

	public Entity Network_controllingEntity
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____controllingEntityNetId, ref _controllingEntity);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref _controllingEntity, 32uL, SelectedEntityChanged, ref ____controllingEntityNetId);
		}
	}

	public Hero Network_003Chero_003Ek__BackingField
	{
		get
		{
			return GetSyncVarNetworkBehaviour(____003Chero_003Ek__BackingFieldNetId, ref hero);
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter_NetworkBehaviour(value, ref hero, 64uL, OnHeroChanged, ref ____003Chero_003Ek__BackingFieldNetId);
		}
	}

	public int Network_003Cgold_003Ek__BackingField
	{
		get
		{
			return gold;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref gold, 128uL, OnGoldChanged);
		}
	}

	public int Network_003CdreamDust_003Ek__BackingField
	{
		get
		{
			return dreamDust;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref dreamDust, 256uL, OnDreamDustChanged);
		}
	}

	public int Network_003CjonasToken_003Ek__BackingField
	{
		get
		{
			return jonasToken;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref jonasToken, 512uL, OnJonasTokenChanged);
		}
	}

	public float Network_003CcleanseRefundMultiplier_003Ek__BackingField
	{
		get
		{
			return cleanseRefundMultiplier;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref cleanseRefundMultiplier, 1024uL, null);
		}
	}

	public float Network_003CdismantleDreamDustMultiplier_003Ek__BackingField
	{
		get
		{
			return dismantleDreamDustMultiplier;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref dismantleDreamDustMultiplier, 2048uL, null);
		}
	}

	public float Network_003CsellPriceMultiplier_003Ek__BackingField
	{
		get
		{
			return sellPriceMultiplier;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref sellPriceMultiplier, 4096uL, null);
		}
	}

	public float Network_003CbuyPriceMultiplier_003Ek__BackingField
	{
		get
		{
			return buyPriceMultiplier;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref buyPriceMultiplier, 8192uL, null);
		}
	}

	public int Network_003CshopAddedItems_003Ek__BackingField
	{
		get
		{
			return shopAddedItems;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref shopAddedItems, 16384uL, null);
		}
	}

	public int Network_003CallowedShopRefreshes_003Ek__BackingField
	{
		get
		{
			return allowedShopRefreshes;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref allowedShopRefreshes, 32768uL, null);
		}
	}

	public float Network_003CpotionDropChanceMultiplier_003Ek__BackingField
	{
		get
		{
			return potionDropChanceMultiplier;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref potionDropChanceMultiplier, 65536uL, null);
		}
	}

	public float Network_003CdoubleChaosChance_003Ek__BackingField
	{
		get
		{
			return doubleChaosChance;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref doubleChaosChance, 131072uL, null);
		}
	}

	public bool Network_003CisReadingArtifactStory_003Ek__BackingField
	{
		get
		{
			return isReadingArtifactStory;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isReadingArtifactStory, 262144uL, null);
		}
	}

	public CSteamID Network_steamId
	{
		get
		{
			return _steamId;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _steamId, 524288uL, OnSteamIDChanged);
		}
	}

	public string Network_003CselectedHeroType_003Ek__BackingField
	{
		get
		{
			return selectedHeroType;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref selectedHeroType, 1048576uL, OnHeroTypeChanged);
		}
	}

	public HeroLoadoutData Network_003CselectedLoadout_003Ek__BackingField
	{
		get
		{
			return selectedLoadout;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref selectedLoadout, 2097152uL, OnLoadoutChanged);
		}
	}

	public string Network_003CselectedDejavuItem_003Ek__BackingField
	{
		get
		{
			return selectedDejavuItem;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref selectedDejavuItem, 4194304uL, OnDejavuItemChanged);
		}
	}

	public bool Network_003CisReady_003Ek__BackingField
	{
		get
		{
			return isReady;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref isReady, 8388608uL, OnIsReadyChanged);
		}
	}

	public static implicit operator NetworkConnectionToClient(DewPlayer p)
	{
		return p.connectionToClient;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		_humanPlayers.Clear();
	}

	private void OnEquippedNametagChanged(string old, string newVal)
	{
		ClientEvent_OnEquippedNametagChanged?.Invoke(old, newVal);
	}

	protected override void Awake()
	{
		base.Awake();
		if (DefaultAvatar == null)
		{
			DefaultAvatar = Resources.Load<Texture2D>("Sprites/DefaultAvatar");
		}
		allies.Callback += TeamRelationChanged;
		enemies.Callback += TeamRelationChanged;
		neutrals.Callback += TeamRelationChanged;
		Awake_ItemAuth();
	}

	private void TeamRelationChanged(SyncList<DewPlayer>.Operation op, int itemindex, DewPlayer olditem, DewPlayer newitem)
	{
		DewPlayer item = ((newitem != null) ? newitem : olditem);
		if (item == null)
		{
			return;
		}
		foreach (Entity e in NetworkedManagerBase<ActorManager>.instance.allEntities)
		{
			if (!(e.owner != item))
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnRefreshEntityHealthbar?.Invoke(e);
			}
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		if (!base.isLocalPlayer)
		{
			return;
		}
		CmdSetPlayerName(DewSave.profile.name);
		OnStartClient_ItemAuth();
		if (SteamManagerBase.Initialized)
		{
			CmdSetSteamID(SteamUser.GetSteamID());
		}
		List<string> list = new List<string>();
		foreach (LucidDream item in DewResources.FindAllByType<LucidDream>())
		{
			string n = item.GetType().Name;
			if (Dew.IsLucidDreamIncludedInGame(n) && DewSave.profile.lucidDreams[n].status == UnlockStatus.Complete)
			{
				list.Add(n);
			}
		}
		SetUnlockedLucidDreams(list.ToArray());
		if (DewBuildProfile.current.platform == PlatformType.STEAM && SteamManagerBase.Initialized)
		{
			UpdateLocalPlayerAvatar();
		}
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(0.1f);
			while (SteamManagerBase.Initialized && steamId.m_SteamID == 0L)
			{
				yield return null;
			}
			CmdSetHeroType(DewSave.profile.preferredHero);
			if (!string.IsNullOrEmpty(DewSave.profile.preferredNametag))
			{
				CmdSetNametag_Imp(DewSave.profile.preferredNametag);
			}
		}
	}

	public override void LogicUpdate(float dt)
	{
		LogicUpdate_InGame();
		LogicUpdate_InGame_NetworkedOnScreenTimer();
	}

	private void UpdateLocalPlayerAvatar()
	{
		if (!base.isLocalPlayer || !SteamManagerBase.Initialized)
		{
			return;
		}
		try
		{
			int imageHandle = SteamFriends.GetMediumFriendAvatar(SteamUser.GetSteamID());
			if (imageHandle > 0)
			{
				SteamUtils.GetImageSize(imageHandle, out var w, out var h);
				byte[] buffer = new byte[w * h * 4];
				SteamUtils.GetImageRGBA(imageHandle, buffer, buffer.Length);
				Texture2D newTexture = new Texture2D((int)w, (int)h, TextureFormat.RGBA32, mipChain: false);
				newTexture.LoadRawTextureData(buffer);
				newTexture.filterMode = FilterMode.Trilinear;
				newTexture.Apply();
				Color[] pixels = newTexture.GetPixels();
				Color[] pixelsFlipped = new Color[pixels.Length];
				for (int i = 0; i < h; i++)
				{
					Array.Copy(pixels, i * w, pixelsFlipped, (h - i - 1) * w, w);
				}
				newTexture.SetPixels(pixelsFlipped);
				newTexture.Apply();
				CmdSetPlayerAvatar(newTexture);
			}
		}
		catch (Exception exception)
		{
			Debug.Log("Failed to update local player avatar.");
			Debug.LogException(exception);
		}
	}

	[Server]
	public void Kick()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewPlayer::Kick()' called when server was not active");
			return;
		}
		if (base.isLocalPlayer)
		{
			throw new InvalidOperationException();
		}
		if (base.connectionToClient != null)
		{
			TpcKick();
			isKicked = true;
			StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(0.35f);
			if (base.connectionToClient != null)
			{
				base.connectionToClient.Disconnect();
			}
		}
	}

	[TargetRpc]
	private void TpcKick()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendTargetRPCInternal(null, "System.Void DewPlayer::TpcKick()", 1044727803, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Command]
	private void CmdSetPlayerAvatar(Texture2D tex)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteTexture2D(tex);
		SendCommandInternal("System.Void DewPlayer::CmdSetPlayerAvatar(UnityEngine.Texture2D)", -689222938, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Command]
	private void CmdSetPlayerName(string newName)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(newName);
		SendCommandInternal("System.Void DewPlayer::CmdSetPlayerName(System.String)", -2009512102, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	public void CmdSetNametag(string ntName)
	{
		if (base.isLocalPlayer)
		{
			DewSave.profile.preferredNametag = ntName;
			CmdSetNametag_Imp(ntName);
		}
	}

	[Command]
	private void CmdSetNametag_Imp(string ntName)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(ntName);
		SendCommandInternal("System.Void DewPlayer::CmdSetNametag_Imp(System.String)", -443679264, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Command]
	public void CmdSetDejavuItem(string item)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(item);
		SendCommandInternal("System.Void DewPlayer::CmdSetDejavuItem(System.String)", 401430204, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Command]
	private void SetUnlockedLucidDreams(string[] dreams)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_System_002EString_005B_005D(writer, dreams);
		SendCommandInternal("System.Void DewPlayer::SetUnlockedLucidDreams(System.String[])", -1108135284, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	public override void OnStart()
	{
		base.OnStart();
		if (base.isLocalPlayer)
		{
			local = this;
		}
		if (_role == Role.Creep)
		{
			creep = this;
		}
		else if (_role == Role.Environment)
		{
			environment = this;
		}
		else
		{
			_humanPlayers.Add(this);
			DewNetworkManager.instance.onHumanPlayerAdd?.Invoke(this);
			UpdateGameObjectName();
		}
		Transform parent = ManagerBase<NetworkLogicPackage>.instance.transform.Find("Players");
		base.transform.parent = parent.transform;
		if (base.isServer && base.isLocalPlayer)
		{
			Network_isHostPlayer = true;
		}
		OnStart_Lobby();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_avatar != null)
		{
			UnityEngine.Object.Destroy(_avatar);
		}
	}

	public override void OnStop()
	{
		base.OnStop();
		if (environment == this)
		{
			environment = null;
		}
		if (creep == this)
		{
			creep = null;
		}
		if (_humanPlayers.Contains(this))
		{
			_humanPlayers.Remove(this);
			if (DewNetworkManager.instance != null)
			{
				DewNetworkManager.instance.onHumanPlayerRemove?.Invoke(this);
			}
		}
		if (local == this)
		{
			local = null;
		}
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		if (NetworkedManagerBase<PlayLobbyManager>.instance != null && NetworkedManagerBase<PlayLobbyManager>.instance.isServer)
		{
			lucidDreams.Clear();
			NetworkedManagerBase<PlayLobbyManager>.instance.UpdateAvailableLucidDreams();
		}
		if (!(NetworkedManagerBase<ActorManager>.instance != null))
		{
			return;
		}
		Actor[] array = NetworkedManagerBase<ActorManager>.instance.allActors.ToArray();
		foreach (Actor obj in array)
		{
			if (obj is SkillTrigger st && (st.tempOwner == this || (st.handOwner != null && st.handOwner.owner == this)))
			{
				st.Destroy();
			}
			if (obj is Gem g && (g.tempOwner == this || (g.handOwner != null && g.handOwner.owner == this)))
			{
				g.Destroy();
			}
		}
	}

	public TeamRelation GetTeamRelation(DewPlayer other)
	{
		if (allies.Contains(other))
		{
			return TeamRelation.Ally;
		}
		if (enemies.Contains(other))
		{
			return TeamRelation.Enemy;
		}
		if (neutrals.Contains(other))
		{
			return TeamRelation.Neutral;
		}
		if (this == other)
		{
			return TeamRelation.Own;
		}
		if ((this == creep && other == environment) || (this == environment && other == creep))
		{
			return TeamRelation.Ally;
		}
		if (this == environment || other == environment)
		{
			return TeamRelation.Neutral;
		}
		if (this == creep || other == creep)
		{
			return TeamRelation.Enemy;
		}
		if (isHumanPlayer && other.isHumanPlayer)
		{
			return TeamRelation.Ally;
		}
		return TeamRelation.Neutral;
	}

	public bool CheckEnemyOrNeutral(Entity target)
	{
		TeamRelation rel = GetTeamRelation(target);
		if (rel != TeamRelation.Enemy)
		{
			return rel == TeamRelation.Neutral;
		}
		return true;
	}

	[TargetRpc]
	public void SendLog(string message)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(message);
		SendTargetRPCInternal(null, "System.Void DewPlayer::SendLog(System.String)", -1656474440, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[TargetRpc]
	public void SendLogWarning(string message)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(message);
		SendTargetRPCInternal(null, "System.Void DewPlayer::SendLogWarning(System.String)", 286198702, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[TargetRpc]
	public void SendLogError(string message)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(message);
		SendTargetRPCInternal(null, "System.Void DewPlayer::SendLogError(System.String)", -1800275454, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	private void OnPlayerNameChanged(string oldName, string newName)
	{
		UpdateGameObjectName();
	}

	private void UpdateGameObjectName()
	{
		base.name = (base.isLocalPlayer ? "HumanPlayerLocal" : "HumanPlayer") + ((steamId != default(CSteamID)) ? $"({steamId.m_SteamID})" : "") + " " + playerName;
	}

	private void OnHeroChanged(Hero oldHero, Hero newHero)
	{
		ClientEvent_OnHeroChanged?.Invoke(oldHero, newHero);
	}

	private void OnGoldChanged(int oldValue, int newValue)
	{
		ClientEvent_OnGoldChanged?.Invoke(oldValue, newValue);
	}

	private void OnDreamDustChanged(int oldValue, int newValue)
	{
		ClientEvent_OnDreamDustChanged?.Invoke(oldValue, newValue);
	}

	private void OnJonasTokenChanged(int oldValue, int newValue)
	{
		ClientEvent_OnJonasTokenChanged?.Invoke(oldValue, newValue);
	}

	private void LogicUpdate_InGame()
	{
		if (!base.isLocalPlayer || !NetworkClient.ready || NetworkedManagerBase<GameManager>.softInstance == null)
		{
			return;
		}
		try
		{
			Vector3 pos = (cursorWorldPos = ((DewInput.currentMode != InputMode.Gamepad) ? ControlManager.GetWorldPositionOnGroundOnCursor(forDirectionalAttacks: false) : ((Network_003Chero_003Ek__BackingField == null) ? Vector3.zero : (ManagerBase<ControlManager>.instance.aimPoint.HasValue ? ManagerBase<ControlManager>.instance.aimPoint.Value : ((!ManagerBase<ControlManager>.instance.isLastMovementDirectionFresh) ? (Network_003Chero_003Ek__BackingField.agentPosition + Network_003Chero_003Ek__BackingField.transform.forward * 8f) : (Network_003Chero_003Ek__BackingField.agentPosition + ManagerBase<ControlManager>.instance.lastMovementDirection * 8f))))));
			CmdNotifyClientStatus(pos, DewInput.currentMode, ManagerBase<ControlManager>.instance.aimPoint.HasValue, ManagerBase<ControlManager>.instance.targetEnemy);
		}
		catch (Exception)
		{
		}
	}

	[Command]
	private void CmdNotifyClientStatus(Vector3 pos, InputMode mode, bool isExplicit, Entity targetEnemy)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteVector3(pos);
		GeneratedNetworkCode._Write_InputMode(writer, mode);
		writer.WriteBool(isExplicit);
		writer.WriteNetworkBehaviour(targetEnemy);
		SendCommandInternal("System.Void DewPlayer::CmdNotifyClientStatus(UnityEngine.Vector3,InputMode,System.Boolean,Entity)", 1813799408, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	public TeamRelation GetTeamRelation(Entity other)
	{
		return GetTeamRelation(other.owner);
	}

	private void SelectedEntityChanged(Entity before, Entity after)
	{
		ManagerBase<ControlManager>.instance.onSelectedEntityChanged?.Invoke(before, after);
	}

	[TargetRpc]
	public void TpcShowWorldPopMessage(WorldMessageSetting message)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_WorldMessageSetting(writer, message);
		SendTargetRPCInternal(null, "System.Void DewPlayer::TpcShowWorldPopMessage(WorldMessageSetting)", 728588415, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[TargetRpc]
	public void TpcShowCenterMessage(CenterMessageType type, string key)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_CenterMessageType(writer, type);
		writer.WriteString(key);
		SendTargetRPCInternal(null, "System.Void DewPlayer::TpcShowCenterMessage(CenterMessageType,System.String)", 463458304, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[TargetRpc]
	public void TpcNotifyDejavuUse()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendTargetRPCInternal(null, "System.Void DewPlayer::TpcNotifyDejavuUse()", -600899820, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[TargetRpc]
	public void TpcShowCenterMessage(CenterMessageType type, string key, string[] formatArgs)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_CenterMessageType(writer, type);
		writer.WriteString(key);
		GeneratedNetworkCode._Write_System_002EString_005B_005D(writer, formatArgs);
		SendTargetRPCInternal(null, "System.Void DewPlayer::TpcShowCenterMessage(CenterMessageType,System.String,System.String[])", -657416102, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void GiveStardust(int amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewPlayer::GiveStardust(System.Int32)' called when server was not active");
		}
		else
		{
			RpcGiveStardust(amount);
		}
	}

	[Command]
	public void CmdRequestStardust(int amount)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(amount);
		SendCommandInternal("System.Void DewPlayer::CmdRequestStardust(System.Int32)", 1004913566, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Command]
	public void CmdGiveCurrency(int goldAmount, int dreamDustAmount, DewPlayer target)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(goldAmount);
		writer.WriteInt(dreamDustAmount);
		writer.WriteNetworkBehaviour(target);
		SendCommandInternal("System.Void DewPlayer::CmdGiveCurrency(System.Int32,System.Int32,DewPlayer)", -1970337615, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcGiveStardust(int amount)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(amount);
		SendRPCInternal("System.Void DewPlayer::RpcGiveStardust(System.Int32)", -1224848007, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void SpendGold(int amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewPlayer::SpendGold(System.Int32)' called when server was not active");
			return;
		}
		amount = Mathf.Min(amount, gold);
		if (amount > 0)
		{
			gold -= amount;
			RpcInvokeOnSpendGold(amount);
		}
	}

	[Server]
	public void EarnGold(int amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewPlayer::EarnGold(System.Int32)' called when server was not active");
		}
		else if (amount > 0)
		{
			gold += amount;
			RpcInvokeOnEarnGold(amount);
		}
	}

	[Server]
	public void SpendDreamDust(int amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewPlayer::SpendDreamDust(System.Int32)' called when server was not active");
			return;
		}
		amount = Mathf.Min(amount, dreamDust);
		if (amount > 0)
		{
			dreamDust -= amount;
			RpcInvokeOnSpendDreamDust(amount);
		}
	}

	[Server]
	public void EarnDreamDust(int amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewPlayer::EarnDreamDust(System.Int32)' called when server was not active");
		}
		else if (amount > 0)
		{
			dreamDust += amount;
			RpcInvokeOnEarnDreamDust(amount);
		}
	}

	[ClientRpc]
	private void RpcInvokeOnSpendGold(int amount)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(amount);
		SendRPCInternal("System.Void DewPlayer::RpcInvokeOnSpendGold(System.Int32)", -900971321, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcInvokeOnEarnGold(int amount)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(amount);
		SendRPCInternal("System.Void DewPlayer::RpcInvokeOnEarnGold(System.Int32)", -1776477517, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcInvokeOnSpendDreamDust(int amount)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(amount);
		SendRPCInternal("System.Void DewPlayer::RpcInvokeOnSpendDreamDust(System.Int32)", 257724146, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcInvokeOnEarnDreamDust(int amount)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(amount);
		SendRPCInternal("System.Void DewPlayer::RpcInvokeOnEarnDreamDust(System.Int32)", 813730950, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcInvokeOnGiveCurrency(int goldAmount, int dreamDustAmount, DewPlayer target)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(goldAmount);
		writer.WriteInt(dreamDustAmount);
		writer.WriteNetworkBehaviour(target);
		SendRPCInternal("System.Void DewPlayer::RpcInvokeOnGiveCurrency(System.Int32,System.Int32,DewPlayer)", 1832206767, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void LogicUpdate_InGame_NetworkedOnScreenTimer()
	{
		if (!NetworkServer.isLoadingScene && NetworkClient.ready && !(NetworkedManagerBase<GameManager>.softInstance == null) && base.isServer)
		{
			for (int i = 0; i < _onScreenTimersServer.Count; i++)
			{
				TpcSetNetworkedOnScreenTimerValue(_onScreenTimersServer[i]._id, _onScreenTimersServer[i].valueGetter());
			}
		}
	}

	[Server]
	public void ShowOnScreenTimer(NetworkedOnScreenTimerHandle handle)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewPlayer::ShowOnScreenTimer(NetworkedOnScreenTimerHandle)' called when server was not active");
		}
		else if (!(NetworkedManagerBase<GameManager>.softInstance == null) && isHumanPlayer)
		{
			int id = (handle._id = _nextOnScreenTimerId++);
			_onScreenTimersServer.Add(handle);
			TpcCreateNetworkedOnScreenTimer(id, handle, handle.valueGetter());
		}
	}

	[Server]
	public void HideOnScreenTimer(NetworkedOnScreenTimerHandle handle)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewPlayer::HideOnScreenTimer(NetworkedOnScreenTimerHandle)' called when server was not active");
			return;
		}
		_onScreenTimersServer.Remove(handle);
		TpcRemoveNetworkedOnScreenTimer(handle._id);
	}

	[TargetRpc]
	private void TpcCreateNetworkedOnScreenTimer(int id, NetworkedOnScreenTimerHandle handle, float defaultValue)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(id);
		GeneratedNetworkCode._Write_NetworkedOnScreenTimerHandle(writer, handle);
		writer.WriteFloat(defaultValue);
		SendTargetRPCInternal(null, "System.Void DewPlayer::TpcCreateNetworkedOnScreenTimer(System.Int32,NetworkedOnScreenTimerHandle,System.Single)", 1344470087, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[TargetRpc]
	private void TpcSetNetworkedOnScreenTimerValue(int id, float value)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(id);
		writer.WriteFloat(value);
		SendTargetRPCInternal(null, "System.Void DewPlayer::TpcSetNetworkedOnScreenTimerValue(System.Int32,System.Single)", 1995958413, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[TargetRpc]
	private void TpcRemoveNetworkedOnScreenTimer(int id)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteInt(id);
		SendTargetRPCInternal(null, "System.Void DewPlayer::TpcRemoveNetworkedOnScreenTimer(System.Int32)", -2043110025, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Command]
	internal void DispatchSample_Cast(CastInfo info)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_CastInfo(writer, info);
		SendCommandInternal("System.Void DewPlayer::DispatchSample_Cast(CastInfo)", -622201125, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Command]
	internal void DispatchSample_Update(CastInfo info)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_CastInfo(writer, info);
		SendCommandInternal("System.Void DewPlayer::DispatchSample_Update(CastInfo)", -682732059, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	internal void StartSampleCastInfo(SampleCastInfoContext context)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewPlayer::StartSampleCastInfo(SampleCastInfoContext)' called when server was not active");
			return;
		}
		if (!isHumanPlayer)
		{
			try
			{
				context.cancelCallback();
				return;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return;
			}
		}
		if (_currentSampleContext.HasValue)
		{
			CancelSampleCastInfo();
		}
		_currentSampleContext = context;
		TpcSetSampleContext(context);
	}

	[TargetRpc]
	private void TpcSetSampleContext(SampleCastInfoContext? context)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteSampleCastInfoContext(context);
		SendTargetRPCInternal(null, "System.Void DewPlayer::TpcSetSampleContext(System.Nullable`1<SampleCastInfoContext>)", -560506649, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[TargetRpc]
	private void RpcSetCastMethod(CastMethodData method)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteCastMethodData(method);
		SendTargetRPCInternal(null, "System.Void DewPlayer::RpcSetCastMethod(CastMethodData)", -1137566233, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	internal void UpdateSampleCastInfo(CastMethodData castMethod)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewPlayer::UpdateSampleCastInfo(CastMethodData)' called when server was not active");
		}
		else if (_currentSampleContext.HasValue)
		{
			SampleCastInfoContext context = _currentSampleContext.Value;
			context.castMethod = castMethod;
			_currentSampleContext = context;
			RpcSetCastMethod(castMethod);
		}
	}

	[Server]
	internal void StopSampleCastInfo()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void DewPlayer::StopSampleCastInfo()' called when server was not active");
			return;
		}
		_currentSampleContext = null;
		TpcSetSampleContext(null);
	}

	private void CancelSampleCastInfo()
	{
		if (_currentSampleContext.HasValue)
		{
			try
			{
				_currentSampleContext.Value.cancelCallback?.Invoke();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			StopSampleCastInfo();
		}
	}

	[Command]
	private void PlaceholderFunction(SampleCastInfoContext context)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_SampleCastInfoContext(writer, context);
		SendCommandInternal("System.Void DewPlayer::PlaceholderFunction(SampleCastInfoContext)", 899946753, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	private void OnSteamIDChanged(CSteamID oldValue, CSteamID newValue)
	{
		steamId = default(CSteamID);
		if (ValidateSteamIdAssignRequest(newValue))
		{
			steamId = newValue;
		}
		UpdateGameObjectName();
		RevalidateAllItems();
	}

	[Command]
	public void CmdSetSteamID(CSteamID id)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_Steamworks_002ECSteamID(writer, id);
		SendCommandInternal("System.Void DewPlayer::CmdSetSteamID(Steamworks.CSteamID)", 56609821, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	private bool ValidateSteamIdAssignRequest(CSteamID id)
	{
		if (NetworkServer.dontListen)
		{
			return true;
		}
		if (steamId != default(CSteamID))
		{
			return false;
		}
		if (!(ManagerBase<LobbyManager>.instance.service is LobbyServiceSteam steam))
		{
			return false;
		}
		if (steam.currentLobby == null)
		{
			return false;
		}
		if (!steam.lobbyMembers.Contains(id))
		{
			return false;
		}
		foreach (DewPlayer humanPlayer in humanPlayers)
		{
			if (humanPlayer.steamId == id)
			{
				return false;
			}
		}
		return true;
	}

	[Command]
	public void CmdAuthorizeForUse(string ownershipKey)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(ownershipKey);
		SendCommandInternal("System.Void DewPlayer::CmdAuthorizeForUse(System.String)", 1599954497, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	private void Awake_ItemAuth()
	{
		ownershipKeys.Callback += OwnershipKeysOnCallback;
	}

	private void OnStartClient_ItemAuth()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		foreach (KeyValuePair<string, DewProfile.CosmeticsData> obj in DewSave.profile.accessories.Concat(DewSave.profile.emotes).Concat(DewSave.profile.nametags))
		{
			if (!string.IsNullOrEmpty(obj.Value.ownershipKey) && DewItem.IsItemGeneratedFromServer(obj.Key))
			{
				CmdAuthorizeForUse(obj.Value.ownershipKey);
			}
		}
	}

	public void RevalidateAllItems()
	{
		foreach (string k in ownershipKeys)
		{
			OwnershipKeysOnCallback(SyncList<string>.Operation.OP_ADD, -1, (string)null, k);
		}
	}

	private void OwnershipKeysOnCallback(SyncList<string>.Operation op, int itemindex, string olditem, string newitem)
	{
		if (steamId.m_SteamID != 0L && !string.IsNullOrEmpty(newitem))
		{
			DecryptedItemData data = DewItem.GetDecryptedItemData(newitem);
			if (data != null && !ownedItems.Contains(data.item) && ValidateOwnershipData(data))
			{
				ownedItems.Add(data.item);
			}
		}
	}

	private bool ValidateOwnershipData(DecryptedItemData data)
	{
		if (data.owner == 0L || string.IsNullOrEmpty(data.item))
		{
			Debug.Log(base.name + " requesting to use " + data.item + " failed due to malformed item data");
			return false;
		}
		if (data.owner != steamId.m_SteamID)
		{
			Debug.Log($"{base.name} requesting to use {data.item} failed due to Steam ID mismatch (Owner {data.owner} != Requester {steamId.m_SteamID})");
			return false;
		}
		Debug.Log(base.name + " requesting to use " + data.item + " SUCCESS");
		return true;
	}

	public bool IsAllowedToUseItem(string itemName)
	{
		if (string.IsNullOrEmpty(itemName))
		{
			return false;
		}
		if (itemName.StartsWith("Acc_"))
		{
			Accessory obj = DewResources.GetByName<Accessory>(itemName);
			if (obj == null)
			{
				return false;
			}
			if (!obj.generatedFromServer)
			{
				return true;
			}
			return ownedItems.Contains(itemName);
		}
		if (itemName.StartsWith("Nametag_"))
		{
			Nametag obj2 = DewResources.GetByName<Nametag>(itemName);
			if (obj2 == null)
			{
				return false;
			}
			if (!obj2.generatedFromServer)
			{
				return true;
			}
			return ownedItems.Contains(itemName);
		}
		if (itemName.StartsWith("Emote_"))
		{
			Emote obj3 = DewResources.GetByName<Emote>(itemName);
			if (obj3 == null)
			{
				return false;
			}
			if (!obj3.generatedFromServer)
			{
				return true;
			}
			return ownedItems.Contains(itemName);
		}
		return false;
	}

	private void OnIsReadyChanged(bool oldVal, bool newVal)
	{
		ClientEvent_OnIsReadyChanged?.Invoke(newVal);
	}

	private void OnStart_Lobby()
	{
		selectedAccessories.Callback += delegate
		{
			ClientEvent_OnSelectedAccessoriesChanged?.Invoke();
		};
	}

	[Command]
	public void CmdSetAccessories(List<string> accessories)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E(writer, accessories);
		SendCommandInternal("System.Void DewPlayer::CmdSetAccessories(System.Collections.Generic.List`1<System.String>)", -1792256470, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	private void OnHeroTypeChanged(string oldVal, string newVal)
	{
		ClientEvent_OnSelectedHeroTypeChanged?.Invoke(newVal);
		if (base.isLocalPlayer && NetworkedManagerBase<PlayLobbyManager>.instance != null)
		{
			NetworkedManagerBase<PlayLobbyManager>.instance.ClientEvent_OnLocalPlayerHeroChanged?.Invoke(newVal);
		}
	}

	private void OnLoadoutChanged(HeroLoadoutData oldVal, HeroLoadoutData newVal)
	{
		ClientEvent_OnSelectedLoadoutChanged?.Invoke(newVal);
	}

	private void OnDejavuItemChanged(string oldVal, string newVal)
	{
		ClientEvent_OnSelectedDejavuItemChanged?.Invoke(newVal);
	}

	public void CmdSetHeroType(string newType)
	{
		if (base.isLocalPlayer)
		{
			CmdSetHeroType_Imp(newType, DewSave.profile.heroLoadouts[newType][DewSave.profile.heroSelectedLoadoutIndex[newType]], DewSave.profile.heroEquippedAccs[newType]);
		}
	}

	[Command]
	private void CmdSetHeroType_Imp(string newType, HeroLoadoutData loadoutData, List<string> accessories)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(newType);
		GeneratedNetworkCode._Write_HeroLoadoutData(writer, loadoutData);
		GeneratedNetworkCode._Write_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E(writer, accessories);
		SendCommandInternal("System.Void DewPlayer::CmdSetHeroType_Imp(System.String,HeroLoadoutData,System.Collections.Generic.List`1<System.String>)", -951313567, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	public void CmdSetHeroLoadoutData(HeroLoadoutData newData)
	{
		if (DewBuildProfile.current.buildType == BuildType.DemoLite)
		{
			CmdSetHeroLoadoutData_Imp(newData);
			return;
		}
		HeroLoadoutData d = new HeroLoadoutData(newData);
		d.PopulateLevelsByLocalSaveData();
		CmdSetHeroLoadoutData_Imp(d);
	}

	[Command]
	private void CmdSetHeroLoadoutData_Imp(HeroLoadoutData newData)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_HeroLoadoutData(writer, newData);
		SendCommandInternal("System.Void DewPlayer::CmdSetHeroLoadoutData_Imp(HeroLoadoutData)", -1582242929, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Command]
	public void CmdSetIsReady(bool newReady)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteBool(newReady);
		SendCommandInternal("System.Void DewPlayer::CmdSetIsReady(System.Boolean)", 77796566, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[Command]
	public void CmdRequestToJoinCurrentLobby()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendCommandInternal("System.Void DewPlayer::CmdRequestToJoinCurrentLobby()", -1426181877, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	[TargetRpc]
	public void TpcMakePlayerChangeLobby(string lobbyId)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(lobbyId);
		SendTargetRPCInternal(null, "System.Void DewPlayer::TpcMakePlayerChangeLobby(System.String)", -492420642, writer, 0);
		NetworkWriterPool.Return(writer);
	}

	private async UniTask ChangeLobby_Imp(string lobbyId)
	{
		if (!NetworkServer.active && !ManagerBase<LobbyManager>.instance.isLobbyLeader)
		{
			Debug.Log("Lobby replaced by host: " + lobbyId);
			await ManagerBase<LobbyManager>.instance.service.JoinLobby(lobbyId);
			Debug.Log("Joined the replaced lobby");
		}
	}

	public DewPlayer()
	{
		InitSyncObject(allies);
		InitSyncObject(enemies);
		InitSyncObject(neutrals);
		InitSyncObject(stars);
		InitSyncObject(ownershipKeys);
		InitSyncObject(selectedAccessories);
	}

	static DewPlayer()
	{
		_humanPlayers = new List<DewPlayer>();
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetPlayerAvatar(UnityEngine.Texture2D)", InvokeUserCode_CmdSetPlayerAvatar__Texture2D, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetPlayerName(System.String)", InvokeUserCode_CmdSetPlayerName__String, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetNametag_Imp(System.String)", InvokeUserCode_CmdSetNametag_Imp__String, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetDejavuItem(System.String)", InvokeUserCode_CmdSetDejavuItem__String, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::SetUnlockedLucidDreams(System.String[])", InvokeUserCode_SetUnlockedLucidDreams__String_005B_005D, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdNotifyClientStatus(UnityEngine.Vector3,InputMode,System.Boolean,Entity)", InvokeUserCode_CmdNotifyClientStatus__Vector3__InputMode__Boolean__Entity, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdRequestStardust(System.Int32)", InvokeUserCode_CmdRequestStardust__Int32, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdGiveCurrency(System.Int32,System.Int32,DewPlayer)", InvokeUserCode_CmdGiveCurrency__Int32__Int32__DewPlayer, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::DispatchSample_Cast(CastInfo)", InvokeUserCode_DispatchSample_Cast__CastInfo, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::DispatchSample_Update(CastInfo)", InvokeUserCode_DispatchSample_Update__CastInfo, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::PlaceholderFunction(SampleCastInfoContext)", InvokeUserCode_PlaceholderFunction__SampleCastInfoContext, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetSteamID(Steamworks.CSteamID)", InvokeUserCode_CmdSetSteamID__CSteamID, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdAuthorizeForUse(System.String)", InvokeUserCode_CmdAuthorizeForUse__String, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetAccessories(System.Collections.Generic.List`1<System.String>)", InvokeUserCode_CmdSetAccessories__List_00601, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetHeroType_Imp(System.String,HeroLoadoutData,System.Collections.Generic.List`1<System.String>)", InvokeUserCode_CmdSetHeroType_Imp__String__HeroLoadoutData__List_00601, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetHeroLoadoutData_Imp(HeroLoadoutData)", InvokeUserCode_CmdSetHeroLoadoutData_Imp__HeroLoadoutData, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdSetIsReady(System.Boolean)", InvokeUserCode_CmdSetIsReady__Boolean, requiresAuthority: true);
		RemoteProcedureCalls.RegisterCommand(typeof(DewPlayer), "System.Void DewPlayer::CmdRequestToJoinCurrentLobby()", InvokeUserCode_CmdRequestToJoinCurrentLobby, requiresAuthority: true);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::RpcGiveStardust(System.Int32)", InvokeUserCode_RpcGiveStardust__Int32);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::RpcInvokeOnSpendGold(System.Int32)", InvokeUserCode_RpcInvokeOnSpendGold__Int32);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::RpcInvokeOnEarnGold(System.Int32)", InvokeUserCode_RpcInvokeOnEarnGold__Int32);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::RpcInvokeOnSpendDreamDust(System.Int32)", InvokeUserCode_RpcInvokeOnSpendDreamDust__Int32);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::RpcInvokeOnEarnDreamDust(System.Int32)", InvokeUserCode_RpcInvokeOnEarnDreamDust__Int32);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::RpcInvokeOnGiveCurrency(System.Int32,System.Int32,DewPlayer)", InvokeUserCode_RpcInvokeOnGiveCurrency__Int32__Int32__DewPlayer);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcKick()", InvokeUserCode_TpcKick);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::SendLog(System.String)", InvokeUserCode_SendLog__String);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::SendLogWarning(System.String)", InvokeUserCode_SendLogWarning__String);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::SendLogError(System.String)", InvokeUserCode_SendLogError__String);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcShowWorldPopMessage(WorldMessageSetting)", InvokeUserCode_TpcShowWorldPopMessage__WorldMessageSetting);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcShowCenterMessage(CenterMessageType,System.String)", InvokeUserCode_TpcShowCenterMessage__CenterMessageType__String);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcNotifyDejavuUse()", InvokeUserCode_TpcNotifyDejavuUse);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcShowCenterMessage(CenterMessageType,System.String,System.String[])", InvokeUserCode_TpcShowCenterMessage__CenterMessageType__String__String_005B_005D);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcCreateNetworkedOnScreenTimer(System.Int32,NetworkedOnScreenTimerHandle,System.Single)", InvokeUserCode_TpcCreateNetworkedOnScreenTimer__Int32__NetworkedOnScreenTimerHandle__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcSetNetworkedOnScreenTimerValue(System.Int32,System.Single)", InvokeUserCode_TpcSetNetworkedOnScreenTimerValue__Int32__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcRemoveNetworkedOnScreenTimer(System.Int32)", InvokeUserCode_TpcRemoveNetworkedOnScreenTimer__Int32);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcSetSampleContext(System.Nullable`1<SampleCastInfoContext>)", InvokeUserCode_TpcSetSampleContext__Nullable_00601);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::RpcSetCastMethod(CastMethodData)", InvokeUserCode_RpcSetCastMethod__CastMethodData);
		RemoteProcedureCalls.RegisterRpc(typeof(DewPlayer), "System.Void DewPlayer::TpcMakePlayerChangeLobby(System.String)", InvokeUserCode_TpcMakePlayerChangeLobby__String);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TpcKick()
	{
		DewNetworkManager.instance.isBeingKicked = true;
		ManagerBase<MessageManager>.instance.ShowMessageLocalized("Title_Message_YouAreKickedFromGame");
		DewNetworkManager.instance.EndSession();
	}

	protected static void InvokeUserCode_TpcKick(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcKick called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_TpcKick();
		}
	}

	protected void UserCode_CmdSetPlayerAvatar__Texture2D(Texture2D tex)
	{
		Network_avatar = tex;
	}

	protected static void InvokeUserCode_CmdSetPlayerAvatar__Texture2D(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetPlayerAvatar called on client.");
		}
		else
		{
			((DewPlayer)obj).UserCode_CmdSetPlayerAvatar__Texture2D(reader.ReadTexture2D());
		}
	}

	protected void UserCode_CmdSetPlayerName__String(string newName)
	{
		if (!isPlayerNameSet)
		{
			newName = newName.Trim();
			if (!DewProfile.ValidateProfileName(newName))
			{
				newName = "Traveler";
			}
			Network_playerName = newName;
			isPlayerNameSet = true;
		}
	}

	protected static void InvokeUserCode_CmdSetPlayerName__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetPlayerName called on client.");
		}
		else
		{
			((DewPlayer)obj).UserCode_CmdSetPlayerName__String(reader.ReadString());
		}
	}

	protected void UserCode_CmdSetNametag_Imp__String(string ntName)
	{
		if (string.IsNullOrEmpty(ntName) || !IsAllowedToUseItem(ntName))
		{
			Network_equippedNametag = null;
		}
		else
		{
			Network_equippedNametag = ntName;
		}
	}

	protected static void InvokeUserCode_CmdSetNametag_Imp__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetNametag_Imp called on client.");
		}
		else
		{
			((DewPlayer)obj).UserCode_CmdSetNametag_Imp__String(reader.ReadString());
		}
	}

	protected void UserCode_CmdSetDejavuItem__String(string item)
	{
		if (item == null)
		{
			Network_003CselectedDejavuItem_003Ek__BackingField = null;
		}
		else if (item.StartsWith("St_"))
		{
			if (Dew.IsSkillIncludedInGame(item) && !(DewResources.GetByShortTypeName<SkillTrigger>(item) == null))
			{
				Network_003CselectedDejavuItem_003Ek__BackingField = item;
			}
		}
		else if (item.StartsWith("Gem_") && Dew.IsGemIncludedInGame(item) && !(DewResources.GetByShortTypeName<Gem>(item) == null))
		{
			Network_003CselectedDejavuItem_003Ek__BackingField = item;
		}
	}

	protected static void InvokeUserCode_CmdSetDejavuItem__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetDejavuItem called on client.");
		}
		else
		{
			((DewPlayer)obj).UserCode_CmdSetDejavuItem__String(reader.ReadString());
		}
	}

	protected void UserCode_SetUnlockedLucidDreams__String_005B_005D(string[] dreams)
	{
		if (NetworkedManagerBase<PlayLobbyManager>.instance == null || dreams.Length > Dew.allLucidDreams.Count)
		{
			return;
		}
		lucidDreams.Clear();
		foreach (string d in dreams)
		{
			if (DewResources.GetByShortTypeName<LucidDream>(d) != null)
			{
				lucidDreams.Add(d);
			}
		}
		NetworkedManagerBase<PlayLobbyManager>.instance.UpdateAvailableLucidDreams();
	}

	protected static void InvokeUserCode_SetUnlockedLucidDreams__String_005B_005D(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command SetUnlockedLucidDreams called on client.");
		}
		else
		{
			((DewPlayer)obj).UserCode_SetUnlockedLucidDreams__String_005B_005D(GeneratedNetworkCode._Read_System_002EString_005B_005D(reader));
		}
	}

	protected void UserCode_SendLog__String(string message)
	{
		Debug.Log(message);
	}

	protected static void InvokeUserCode_SendLog__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC SendLog called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_SendLog__String(reader.ReadString());
		}
	}

	protected void UserCode_SendLogWarning__String(string message)
	{
		Debug.LogWarning(message);
	}

	protected static void InvokeUserCode_SendLogWarning__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC SendLogWarning called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_SendLogWarning__String(reader.ReadString());
		}
	}

	protected void UserCode_SendLogError__String(string message)
	{
		Debug.LogError(message);
	}

	protected static void InvokeUserCode_SendLogError__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC SendLogError called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_SendLogError__String(reader.ReadString());
		}
	}

	protected void UserCode_CmdNotifyClientStatus__Vector3__InputMode__Boolean__Entity(Vector3 pos, InputMode mode, bool isExplicit, Entity targetEnemy)
	{
		cursorWorldPos = pos;
		inputMode = mode;
		isGamepadExplicitAim = isExplicit;
		gamepadTargetEnemy = targetEnemy;
	}

	protected static void InvokeUserCode_CmdNotifyClientStatus__Vector3__InputMode__Boolean__Entity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdNotifyClientStatus called on client.");
		}
		else
		{
			((DewPlayer)obj).UserCode_CmdNotifyClientStatus__Vector3__InputMode__Boolean__Entity(reader.ReadVector3(), GeneratedNetworkCode._Read_InputMode(reader), reader.ReadBool(), reader.ReadNetworkBehaviour<Entity>());
		}
	}

	protected void UserCode_TpcShowWorldPopMessage__WorldMessageSetting(WorldMessageSetting message)
	{
		InGameUIManager.instance.ShowWorldPopMessage(message);
	}

	protected static void InvokeUserCode_TpcShowWorldPopMessage__WorldMessageSetting(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcShowWorldPopMessage called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_TpcShowWorldPopMessage__WorldMessageSetting(GeneratedNetworkCode._Read_WorldMessageSetting(reader));
		}
	}

	protected void UserCode_TpcShowCenterMessage__CenterMessageType__String(CenterMessageType type, string key)
	{
		InGameUIManager.instance.ShowCenterMessage(type, key);
	}

	protected static void InvokeUserCode_TpcShowCenterMessage__CenterMessageType__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcShowCenterMessage called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_TpcShowCenterMessage__CenterMessageType__String(GeneratedNetworkCode._Read_CenterMessageType(reader), reader.ReadString());
		}
	}

	protected void UserCode_TpcNotifyDejavuUse()
	{
		if (DewBuildProfile.current.buildType != BuildType.DemoLite && !string.IsNullOrEmpty(selectedDejavuItem) && NetworkedManagerBase<GameSettingsManager>.instance.localPlayerDejavuCost > 0)
		{
			DewSave.profile.stardust -= NetworkedManagerBase<GameSettingsManager>.instance.localPlayerDejavuCost;
			DewSave.profile.spentStardust += NetworkedManagerBase<GameSettingsManager>.instance.localPlayerDejavuCost;
			NetworkedManagerBase<GameSettingsManager>.instance.localPlayerDejavuCost = 0;
			DewSave.profile.itemStatistics[selectedDejavuItem].dejavuCostReductionPeriodTimestamp = DateTime.UtcNow.AddHours(12.0).ToTimestamp();
			DewSave.profile.preferredDejavuItem = selectedDejavuItem;
			DewSave.SaveProfile();
		}
	}

	protected static void InvokeUserCode_TpcNotifyDejavuUse(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcNotifyDejavuUse called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_TpcNotifyDejavuUse();
		}
	}

	protected void UserCode_TpcShowCenterMessage__CenterMessageType__String__String_005B_005D(CenterMessageType type, string key, string[] formatArgs)
	{
		InGameUIManager.instance.ShowCenterMessage(type, key, formatArgs);
	}

	protected static void InvokeUserCode_TpcShowCenterMessage__CenterMessageType__String__String_005B_005D(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcShowCenterMessage called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_TpcShowCenterMessage__CenterMessageType__String__String_005B_005D(GeneratedNetworkCode._Read_CenterMessageType(reader), reader.ReadString(), GeneratedNetworkCode._Read_System_002EString_005B_005D(reader));
		}
	}

	protected void UserCode_CmdRequestStardust__Int32(int amount)
	{
		if (amount > 0 && amount <= 100)
		{
			GiveStardust(amount);
		}
	}

	protected static void InvokeUserCode_CmdRequestStardust__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestStardust called on client.");
		}
		else
		{
			((DewPlayer)obj).UserCode_CmdRequestStardust__Int32(reader.ReadInt());
		}
	}

	protected void UserCode_CmdGiveCurrency__Int32__Int32__DewPlayer(int goldAmount, int dreamDustAmount, DewPlayer target)
	{
		if (target == null || target.Network_003Chero_003Ek__BackingField == null)
		{
			return;
		}
		goldAmount = Mathf.Min(goldAmount, gold);
		dreamDustAmount = Mathf.Min(dreamDustAmount, dreamDust);
		if ((goldAmount > 0 || dreamDustAmount > 0) && !(Network_003Chero_003Ek__BackingField == null))
		{
			gold -= goldAmount;
			dreamDust -= dreamDustAmount;
			Vector3 dropPos = Network_003Chero_003Ek__BackingField.agentPosition;
			if (Vector3.Distance(dropPos, target.Network_003Chero_003Ek__BackingField.position) > 10f || Network_003Chero_003Ek__BackingField.isKnockedOut || target.Network_003Chero_003Ek__BackingField.isKnockedOut)
			{
				dropPos = target.Network_003Chero_003Ek__BackingField.position;
			}
			if (goldAmount > 0)
			{
				NetworkedManagerBase<PickupManager>.instance.DropGold(isKillGold: false, isGivenByOtherPlayer: true, goldAmount, dropPos, target.Network_003Chero_003Ek__BackingField);
			}
			if (dreamDustAmount > 0)
			{
				NetworkedManagerBase<PickupManager>.instance.DropDreamDust(isGivenByOtherPlayer: true, dreamDustAmount, dropPos, target.Network_003Chero_003Ek__BackingField);
			}
			RpcInvokeOnGiveCurrency(goldAmount, dreamDustAmount, target);
		}
	}

	protected static void InvokeUserCode_CmdGiveCurrency__Int32__Int32__DewPlayer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGiveCurrency called on client.");
		}
		else
		{
			((DewPlayer)obj).UserCode_CmdGiveCurrency__Int32__Int32__DewPlayer(reader.ReadInt(), reader.ReadInt(), reader.ReadNetworkBehaviour<DewPlayer>());
		}
	}

	protected void UserCode_RpcGiveStardust__Int32(int amount)
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return new WaitForSeconds(0.2f);
			int remainingAmount = amount;
			while (remainingAmount > 0)
			{
				int amountToGive = Mathf.Max(1, amount / 15);
				amountToGive = Mathf.Min(remainingAmount, amountToGive);
				remainingAmount -= amountToGive;
				ClientEvent_OnEarnStardust?.Invoke(amountToGive);
				if (base.isLocalPlayer)
				{
					DewSave.profile.stardust += amountToGive;
				}
				yield return new WaitForSeconds(0.15f);
			}
		}
	}

	protected static void InvokeUserCode_RpcGiveStardust__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcGiveStardust called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_RpcGiveStardust__Int32(reader.ReadInt());
		}
	}

	protected void UserCode_RpcInvokeOnSpendGold__Int32(int amount)
	{
		ClientEvent_OnSpendGold?.Invoke(amount);
	}

	protected static void InvokeUserCode_RpcInvokeOnSpendGold__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnSpendGold called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_RpcInvokeOnSpendGold__Int32(reader.ReadInt());
		}
	}

	protected void UserCode_RpcInvokeOnEarnGold__Int32(int amount)
	{
		ClientEvent_OnEarnGold?.Invoke(amount);
	}

	protected static void InvokeUserCode_RpcInvokeOnEarnGold__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnEarnGold called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_RpcInvokeOnEarnGold__Int32(reader.ReadInt());
		}
	}

	protected void UserCode_RpcInvokeOnSpendDreamDust__Int32(int amount)
	{
		ClientEvent_OnSpendDreamDust?.Invoke(amount);
	}

	protected static void InvokeUserCode_RpcInvokeOnSpendDreamDust__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnSpendDreamDust called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_RpcInvokeOnSpendDreamDust__Int32(reader.ReadInt());
		}
	}

	protected void UserCode_RpcInvokeOnEarnDreamDust__Int32(int amount)
	{
		ClientEvent_OnEarnDreamDust?.Invoke(amount);
	}

	protected static void InvokeUserCode_RpcInvokeOnEarnDreamDust__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnEarnDreamDust called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_RpcInvokeOnEarnDreamDust__Int32(reader.ReadInt());
		}
	}

	protected void UserCode_RpcInvokeOnGiveCurrency__Int32__Int32__DewPlayer(int goldAmount, int dreamDustAmount, DewPlayer target)
	{
		ClientEvent_OnGiveCurrency?.Invoke(goldAmount, dreamDustAmount, target);
	}

	protected static void InvokeUserCode_RpcInvokeOnGiveCurrency__Int32__Int32__DewPlayer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInvokeOnGiveCurrency called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_RpcInvokeOnGiveCurrency__Int32__Int32__DewPlayer(reader.ReadInt(), reader.ReadInt(), reader.ReadNetworkBehaviour<DewPlayer>());
		}
	}

	protected void UserCode_TpcCreateNetworkedOnScreenTimer__Int32__NetworkedOnScreenTimerHandle__Single(int id, NetworkedOnScreenTimerHandle handle, float defaultValue)
	{
		RefValue<float> val = new RefValue<float>(defaultValue);
		OnScreenTimerHandle timer = new OnScreenTimerHandle
		{
			rawText = ((handle.localeKey != null) ? DewLocalization.GetUIValue(handle.localeKey) : DewLocalization.GetSkillName(handle.skillKey, 0)),
			fillAmountGetter = () => val.value
		};
		_onScreenTimersLocal.Add((id, timer, val));
		NetworkedManagerBase<ClientEventManager>.instance.OnShowOnScreenTimer?.Invoke(timer);
	}

	protected static void InvokeUserCode_TpcCreateNetworkedOnScreenTimer__Int32__NetworkedOnScreenTimerHandle__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcCreateNetworkedOnScreenTimer called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_TpcCreateNetworkedOnScreenTimer__Int32__NetworkedOnScreenTimerHandle__Single(reader.ReadInt(), GeneratedNetworkCode._Read_NetworkedOnScreenTimerHandle(reader), reader.ReadFloat());
		}
	}

	protected void UserCode_TpcSetNetworkedOnScreenTimerValue__Int32__Single(int id, float value)
	{
		foreach (var tuple in _onScreenTimersLocal)
		{
			if (tuple.Item1 == id)
			{
				tuple.Item3.value = value;
			}
		}
	}

	protected static void InvokeUserCode_TpcSetNetworkedOnScreenTimerValue__Int32__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcSetNetworkedOnScreenTimerValue called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_TpcSetNetworkedOnScreenTimerValue__Int32__Single(reader.ReadInt(), reader.ReadFloat());
		}
	}

	protected void UserCode_TpcRemoveNetworkedOnScreenTimer__Int32(int id)
	{
		for (int i = _onScreenTimersLocal.Count - 1; i >= 0; i--)
		{
			if (_onScreenTimersLocal[i].Item1 == id)
			{
				NetworkedManagerBase<ClientEventManager>.instance.OnHideOnScreenTimer?.Invoke(_onScreenTimersLocal[i].Item2);
				_onScreenTimersLocal.RemoveAt(i);
			}
		}
	}

	protected static void InvokeUserCode_TpcRemoveNetworkedOnScreenTimer__Int32(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcRemoveNetworkedOnScreenTimer called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_TpcRemoveNetworkedOnScreenTimer__Int32(reader.ReadInt());
		}
	}

	protected void UserCode_DispatchSample_Cast__CastInfo(CastInfo info)
	{
		if (!_currentSampleContext.HasValue)
		{
			return;
		}
		SampleCastInfoContext context = _currentSampleContext.Value;
		context.currentInfo = info;
		_currentSampleContext = context;
		try
		{
			_currentSampleContext.Value.castCallback?.Invoke(info);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_DispatchSample_Cast__CastInfo(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command DispatchSample_Cast called on client.");
		}
		else
		{
			((DewPlayer)obj).UserCode_DispatchSample_Cast__CastInfo(GeneratedNetworkCode._Read_CastInfo(reader));
		}
	}

	protected void UserCode_DispatchSample_Update__CastInfo(CastInfo info)
	{
		if (!_currentSampleContext.HasValue)
		{
			return;
		}
		SampleCastInfoContext context = _currentSampleContext.Value;
		context.currentInfo = info;
		_currentSampleContext = context;
		try
		{
			_currentSampleContext.Value.updateCallback?.Invoke(info);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected static void InvokeUserCode_DispatchSample_Update__CastInfo(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command DispatchSample_Update called on client.");
		}
		else
		{
			((DewPlayer)obj).UserCode_DispatchSample_Update__CastInfo(GeneratedNetworkCode._Read_CastInfo(reader));
		}
	}

	protected void UserCode_TpcSetSampleContext__Nullable_00601(SampleCastInfoContext? context)
	{
		if (!context.HasValue)
		{
			ManagerBase<ControlManager>.instance._localSampleContext = null;
			return;
		}
		SampleCastInfoContext val = context.Value;
		if (!val.isInitialInfoSet)
		{
			val.currentInfo = ManagerBase<ControlManager>.instance.GetCastInfo(val.castMethod, val.targetValidator);
		}
		if (val.trigger is SkillTrigger skill && Network_003Chero_003Ek__BackingField.Skill.TryGetSkillLocation(skill, out var type) && ManagerBase<ControlManager>.instance._castByKeyInfo.TryGetValue(type, out var tuple))
		{
			(val.castKey, _) = tuple;
		}
		if ((val.castOnButton == SampleCastInfoContext.CastOnButtonType.ByButton || val.castOnButton == SampleCastInfoContext.CastOnButtonType.ByButtonRelease) && DewInput.currentMode == InputMode.KeyboardAndMouse && DewSave.profile.controls.clickToCastInsteadOfHoldToCast)
		{
			val.castOnButton = SampleCastInfoContext.CastOnButtonType.ByButtonPress;
		}
		ManagerBase<ControlManager>.instance._localSampleContext = val;
	}

	protected static void InvokeUserCode_TpcSetSampleContext__Nullable_00601(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcSetSampleContext called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_TpcSetSampleContext__Nullable_00601(reader.ReadSampleCastInfoContext());
		}
	}

	protected void UserCode_RpcSetCastMethod__CastMethodData(CastMethodData method)
	{
		if (ManagerBase<ControlManager>.instance._localSampleContext.HasValue)
		{
			SampleCastInfoContext context = ManagerBase<ControlManager>.instance._localSampleContext.Value;
			context.castMethod = method;
			ManagerBase<ControlManager>.instance._localSampleContext = context;
		}
	}

	protected static void InvokeUserCode_RpcSetCastMethod__CastMethodData(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC RpcSetCastMethod called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_RpcSetCastMethod__CastMethodData(reader.ReadWriteCastMethodData());
		}
	}

	protected void UserCode_PlaceholderFunction__SampleCastInfoContext(SampleCastInfoContext context)
	{
	}

	protected static void InvokeUserCode_PlaceholderFunction__SampleCastInfoContext(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command PlaceholderFunction called on client.");
		}
		else
		{
			((DewPlayer)obj).UserCode_PlaceholderFunction__SampleCastInfoContext(GeneratedNetworkCode._Read_SampleCastInfoContext(reader));
		}
	}

	protected void UserCode_CmdSetSteamID__CSteamID(CSteamID id)
	{
		if (ValidateSteamIdAssignRequest(id))
		{
			Network_steamId = id;
		}
	}

	protected static void InvokeUserCode_CmdSetSteamID__CSteamID(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetSteamID called on client.");
		}
		else
		{
			((DewPlayer)obj).UserCode_CmdSetSteamID__CSteamID(GeneratedNetworkCode._Read_Steamworks_002ECSteamID(reader));
		}
	}

	protected void UserCode_CmdAuthorizeForUse__String(string ownershipKey)
	{
		if (ownershipKeys.Count <= 100 && !ownershipKeys.Contains(ownershipKey) && DewItem.GetDecryptedItemData(ownershipKey) != null)
		{
			ownershipKeys.Add(ownershipKey);
		}
	}

	protected static void InvokeUserCode_CmdAuthorizeForUse__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAuthorizeForUse called on client.");
		}
		else
		{
			((DewPlayer)obj).UserCode_CmdAuthorizeForUse__String(reader.ReadString());
		}
	}

	protected void UserCode_CmdSetAccessories__List_00601(List<string> accessories)
	{
		selectedAccessories.Clear();
		selectedAccessories.AddRange(accessories);
	}

	protected static void InvokeUserCode_CmdSetAccessories__List_00601(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetAccessories called on client.");
		}
		else
		{
			((DewPlayer)obj).UserCode_CmdSetAccessories__List_00601(GeneratedNetworkCode._Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E(reader));
		}
	}

	protected void UserCode_CmdSetHeroType_Imp__String__HeroLoadoutData__List_00601(string newType, HeroLoadoutData loadoutData, List<string> accessories)
	{
		if (NetworkedManagerBase<PlayLobbyManager>.instance == null)
		{
			Debug.LogWarning("Not in a lobby");
			return;
		}
		if (!(DewResources.GetByShortTypeName(newType) is Hero))
		{
			Debug.LogWarning("Hero not found: " + newType);
			return;
		}
		Network_003CselectedHeroType_003Ek__BackingField = newType;
		Network_003CselectedLoadout_003Ek__BackingField = (loadoutData.IsValidFor(newType) ? loadoutData : new HeroLoadoutData());
		selectedAccessories.Clear();
		selectedAccessories.AddRange(accessories);
	}

	protected static void InvokeUserCode_CmdSetHeroType_Imp__String__HeroLoadoutData__List_00601(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetHeroType_Imp called on client.");
		}
		else
		{
			((DewPlayer)obj).UserCode_CmdSetHeroType_Imp__String__HeroLoadoutData__List_00601(reader.ReadString(), GeneratedNetworkCode._Read_HeroLoadoutData(reader), GeneratedNetworkCode._Read_System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003E(reader));
		}
	}

	protected void UserCode_CmdSetHeroLoadoutData_Imp__HeroLoadoutData(HeroLoadoutData newData)
	{
		if (NetworkedManagerBase<PlayLobbyManager>.instance == null)
		{
			Debug.LogWarning("Not in a lobby");
		}
		else
		{
			Network_003CselectedLoadout_003Ek__BackingField = (newData.IsValidFor(selectedHeroType) ? newData : new HeroLoadoutData());
		}
	}

	protected static void InvokeUserCode_CmdSetHeroLoadoutData_Imp__HeroLoadoutData(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetHeroLoadoutData_Imp called on client.");
		}
		else
		{
			((DewPlayer)obj).UserCode_CmdSetHeroLoadoutData_Imp__HeroLoadoutData(GeneratedNetworkCode._Read_HeroLoadoutData(reader));
		}
	}

	protected void UserCode_CmdSetIsReady__Boolean(bool newReady)
	{
		Network_003CisReady_003Ek__BackingField = newReady;
		if (NetworkedManagerBase<ZoneManager>.instance != null && NetworkedManagerBase<ZoneManager>.instance.isVoting)
		{
			NetworkedManagerBase<ZoneManager>.instance.UpdateVoteStatus();
		}
	}

	protected static void InvokeUserCode_CmdSetIsReady__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetIsReady called on client.");
		}
		else
		{
			((DewPlayer)obj).UserCode_CmdSetIsReady__Boolean(reader.ReadBool());
		}
	}

	protected void UserCode_CmdRequestToJoinCurrentLobby()
	{
		if (ManagerBase<LobbyManager>.instance.service.currentLobby == null)
		{
			Debug.Log("Player " + base.connectionToClient.address + " requests to join lobby, but no lobby present");
			return;
		}
		Debug.Log("Player " + base.connectionToClient.address + " requests to join lobby " + ManagerBase<LobbyManager>.instance.service.currentLobby.id);
		TpcMakePlayerChangeLobby(ManagerBase<LobbyManager>.instance.service.currentLobby.id);
	}

	protected static void InvokeUserCode_CmdRequestToJoinCurrentLobby(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestToJoinCurrentLobby called on client.");
		}
		else
		{
			((DewPlayer)obj).UserCode_CmdRequestToJoinCurrentLobby();
		}
	}

	protected void UserCode_TpcMakePlayerChangeLobby__String(string lobbyId)
	{
		ChangeLobby_Imp(lobbyId);
	}

	protected static void InvokeUserCode_TpcMakePlayerChangeLobby__String(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TpcMakePlayerChangeLobby called on server.");
		}
		else
		{
			((DewPlayer)obj).UserCode_TpcMakePlayerChangeLobby__String(reader.ReadString());
		}
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			GeneratedNetworkCode._Write_DewPlayer_002FRole(writer, _role);
			writer.WriteString(_playerName);
			writer.WriteString(_equippedNametag);
			writer.WriteBool(_isHostPlayer);
			writer.WriteTexture2D(_avatar);
			writer.WriteNetworkBehaviour(Network_controllingEntity);
			writer.WriteNetworkBehaviour(Network_003Chero_003Ek__BackingField);
			writer.WriteInt(gold);
			writer.WriteInt(dreamDust);
			writer.WriteInt(jonasToken);
			writer.WriteFloat(cleanseRefundMultiplier);
			writer.WriteFloat(dismantleDreamDustMultiplier);
			writer.WriteFloat(sellPriceMultiplier);
			writer.WriteFloat(buyPriceMultiplier);
			writer.WriteInt(shopAddedItems);
			writer.WriteInt(allowedShopRefreshes);
			writer.WriteFloat(potionDropChanceMultiplier);
			writer.WriteFloat(doubleChaosChance);
			writer.WriteBool(isReadingArtifactStory);
			GeneratedNetworkCode._Write_Steamworks_002ECSteamID(writer, _steamId);
			writer.WriteString(selectedHeroType);
			GeneratedNetworkCode._Write_HeroLoadoutData(writer, selectedLoadout);
			writer.WriteString(selectedDejavuItem);
			writer.WriteBool(isReady);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			GeneratedNetworkCode._Write_DewPlayer_002FRole(writer, _role);
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteString(_playerName);
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteString(_equippedNametag);
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteBool(_isHostPlayer);
		}
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			writer.WriteTexture2D(_avatar);
		}
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_controllingEntity);
		}
		if ((base.syncVarDirtyBits & 0x40L) != 0L)
		{
			writer.WriteNetworkBehaviour(Network_003Chero_003Ek__BackingField);
		}
		if ((base.syncVarDirtyBits & 0x80L) != 0L)
		{
			writer.WriteInt(gold);
		}
		if ((base.syncVarDirtyBits & 0x100L) != 0L)
		{
			writer.WriteInt(dreamDust);
		}
		if ((base.syncVarDirtyBits & 0x200L) != 0L)
		{
			writer.WriteInt(jonasToken);
		}
		if ((base.syncVarDirtyBits & 0x400L) != 0L)
		{
			writer.WriteFloat(cleanseRefundMultiplier);
		}
		if ((base.syncVarDirtyBits & 0x800L) != 0L)
		{
			writer.WriteFloat(dismantleDreamDustMultiplier);
		}
		if ((base.syncVarDirtyBits & 0x1000L) != 0L)
		{
			writer.WriteFloat(sellPriceMultiplier);
		}
		if ((base.syncVarDirtyBits & 0x2000L) != 0L)
		{
			writer.WriteFloat(buyPriceMultiplier);
		}
		if ((base.syncVarDirtyBits & 0x4000L) != 0L)
		{
			writer.WriteInt(shopAddedItems);
		}
		if ((base.syncVarDirtyBits & 0x8000L) != 0L)
		{
			writer.WriteInt(allowedShopRefreshes);
		}
		if ((base.syncVarDirtyBits & 0x10000L) != 0L)
		{
			writer.WriteFloat(potionDropChanceMultiplier);
		}
		if ((base.syncVarDirtyBits & 0x20000L) != 0L)
		{
			writer.WriteFloat(doubleChaosChance);
		}
		if ((base.syncVarDirtyBits & 0x40000L) != 0L)
		{
			writer.WriteBool(isReadingArtifactStory);
		}
		if ((base.syncVarDirtyBits & 0x80000L) != 0L)
		{
			GeneratedNetworkCode._Write_Steamworks_002ECSteamID(writer, _steamId);
		}
		if ((base.syncVarDirtyBits & 0x100000L) != 0L)
		{
			writer.WriteString(selectedHeroType);
		}
		if ((base.syncVarDirtyBits & 0x200000L) != 0L)
		{
			GeneratedNetworkCode._Write_HeroLoadoutData(writer, selectedLoadout);
		}
		if ((base.syncVarDirtyBits & 0x400000L) != 0L)
		{
			writer.WriteString(selectedDejavuItem);
		}
		if ((base.syncVarDirtyBits & 0x800000L) != 0L)
		{
			writer.WriteBool(isReady);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref _role, null, GeneratedNetworkCode._Read_DewPlayer_002FRole(reader));
			GeneratedSyncVarDeserialize(ref _playerName, OnPlayerNameChanged, reader.ReadString());
			GeneratedSyncVarDeserialize(ref _equippedNametag, OnEquippedNametagChanged, reader.ReadString());
			GeneratedSyncVarDeserialize(ref _isHostPlayer, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref _avatar, null, reader.ReadTexture2D());
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _controllingEntity, SelectedEntityChanged, reader, ref ____controllingEntityNetId);
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref hero, OnHeroChanged, reader, ref ____003Chero_003Ek__BackingFieldNetId);
			GeneratedSyncVarDeserialize(ref gold, OnGoldChanged, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref dreamDust, OnDreamDustChanged, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref jonasToken, OnJonasTokenChanged, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref cleanseRefundMultiplier, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref dismantleDreamDustMultiplier, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref sellPriceMultiplier, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref buyPriceMultiplier, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref shopAddedItems, null, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref allowedShopRefreshes, null, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref potionDropChanceMultiplier, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref doubleChaosChance, null, reader.ReadFloat());
			GeneratedSyncVarDeserialize(ref isReadingArtifactStory, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref _steamId, OnSteamIDChanged, GeneratedNetworkCode._Read_Steamworks_002ECSteamID(reader));
			GeneratedSyncVarDeserialize(ref selectedHeroType, OnHeroTypeChanged, reader.ReadString());
			GeneratedSyncVarDeserialize(ref selectedLoadout, OnLoadoutChanged, GeneratedNetworkCode._Read_HeroLoadoutData(reader));
			GeneratedSyncVarDeserialize(ref selectedDejavuItem, OnDejavuItemChanged, reader.ReadString());
			GeneratedSyncVarDeserialize(ref isReady, OnIsReadyChanged, reader.ReadBool());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _role, null, GeneratedNetworkCode._Read_DewPlayer_002FRole(reader));
		}
		if ((num & 2L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _playerName, OnPlayerNameChanged, reader.ReadString());
		}
		if ((num & 4L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _equippedNametag, OnEquippedNametagChanged, reader.ReadString());
		}
		if ((num & 8L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _isHostPlayer, null, reader.ReadBool());
		}
		if ((num & 0x10L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _avatar, null, reader.ReadTexture2D());
		}
		if ((num & 0x20L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref _controllingEntity, SelectedEntityChanged, reader, ref ____controllingEntityNetId);
		}
		if ((num & 0x40L) != 0L)
		{
			GeneratedSyncVarDeserialize_NetworkBehaviour(ref hero, OnHeroChanged, reader, ref ____003Chero_003Ek__BackingFieldNetId);
		}
		if ((num & 0x80L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref gold, OnGoldChanged, reader.ReadInt());
		}
		if ((num & 0x100L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref dreamDust, OnDreamDustChanged, reader.ReadInt());
		}
		if ((num & 0x200L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref jonasToken, OnJonasTokenChanged, reader.ReadInt());
		}
		if ((num & 0x400L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref cleanseRefundMultiplier, null, reader.ReadFloat());
		}
		if ((num & 0x800L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref dismantleDreamDustMultiplier, null, reader.ReadFloat());
		}
		if ((num & 0x1000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref sellPriceMultiplier, null, reader.ReadFloat());
		}
		if ((num & 0x2000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref buyPriceMultiplier, null, reader.ReadFloat());
		}
		if ((num & 0x4000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref shopAddedItems, null, reader.ReadInt());
		}
		if ((num & 0x8000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref allowedShopRefreshes, null, reader.ReadInt());
		}
		if ((num & 0x10000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref potionDropChanceMultiplier, null, reader.ReadFloat());
		}
		if ((num & 0x20000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref doubleChaosChance, null, reader.ReadFloat());
		}
		if ((num & 0x40000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isReadingArtifactStory, null, reader.ReadBool());
		}
		if ((num & 0x80000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _steamId, OnSteamIDChanged, GeneratedNetworkCode._Read_Steamworks_002ECSteamID(reader));
		}
		if ((num & 0x100000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref selectedHeroType, OnHeroTypeChanged, reader.ReadString());
		}
		if ((num & 0x200000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref selectedLoadout, OnLoadoutChanged, GeneratedNetworkCode._Read_HeroLoadoutData(reader));
		}
		if ((num & 0x400000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref selectedDejavuItem, OnDejavuItemChanged, reader.ReadString());
		}
		if ((num & 0x800000L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref isReady, OnIsReadyChanged, reader.ReadBool());
		}
	}
}
