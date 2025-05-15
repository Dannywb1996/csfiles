using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FIMSpace.FSpine;
using FIMSpace.FTail;
using MagicaCloth2;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(EntityHighlightProvider))]
[LogicUpdatePriority(-299)]
public class EntityVisual : EntityComponent, ICleanup
{
	public enum EntityDeathBehavior
	{
		None,
		HideModel,
		Dissolve
	}

	private const float ElementalPropertySpeed = 8f;

	private const float YVelGravity = 18.8f;

	private const float KnockUpStrengthMultiplier = 6f;

	public SafeAction<bool> ClientEvent_OnRendererEnabledChanged;

	public SafeAction<bool> ClientEvent_OnGroundMarkerHiddenChanged;

	public Renderer[] bodyRenderers;

	public Transform healthBarPosition;

	public bool hasGoldDissolve;

	public EntityDeathBehavior deathBehavior = EntityDeathBehavior.Dissolve;

	public float dissolveDelay = 0.2f;

	public float dissolveDuration = 1f;

	public float spawnDuration;

	public bool invulnerableWhileSpawning;

	public float dazeAfterSpawnDuration;

	public bool doPositionOffset;

	public AnimationCurve spawnXOffset = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public AnimationCurve spawnYOffset = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public AnimationCurve spawnZOffset = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public bool doScaling;

	public bool useSeparateAxis;

	public AnimationCurve spawnXScale = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public AnimationCurve spawnYScale = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public AnimationCurve spawnZScale = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public DewAnimationClip spawnAnim;

	public GameObject spawnEffect;

	public bool delaySpawnEffectUntilSpawned;

	public GameObject spawnEffectOnGround;

	public bool delaySpawnEffectOnGroundTilSpawned;

	[FormerlySerializedAs("startEffect")]
	public GameObject loopEffect;

	[FormerlySerializedAs("delayStartEffectTilSpawned")]
	public bool delayLoopEffectTilSpawned;

	public GameObject deathEffect;

	public GameObject takeDamageEffect;

	[NonSerialized]
	[SyncVar]
	public bool skipSpawning;

	[NonSerialized]
	[SyncVar]
	public bool invisibleByDefault;

	private List<Material> _materials;

	private int _rendererOffCounter;

	private int _groundMarkerHiddenCounter;

	public List<Renderer> renderers;

	private Renderer[] _characterRenderers;

	public List<Renderer> solidRenderers;

	private MagicaCloth[] _clothes;

	private TailAnimator2[] _tails;

	private Transform _apHead;

	private Transform _apLeftHand;

	private Transform _apRightHand;

	private Transform _apLeftFoot;

	private Transform _apRightFoot;

	private Transform _apWeapon;

	private Transform _apMuzzle;

	private Transform _apCenter;

	private bool _isDissolving;

	private Vector3 _currentDamage;

	private GameObject _stunEffect;

	[SyncVar(hook = "OnShouldShowStunnedChanged")]
	private bool _shouldShowStunned;

	internal List<FxAttachToEntity> _attachedEffects;

	private ListReturnHandle<FxAttachToEntity> _attachedEffectsHandle;

	private List<ParticleSystem> _pausedSystemsByRendererOff;

	private ListReturnHandle<ParticleSystem> _pausedSystemsByRendererOffHandle;

	private FSpineAnimator _spineAnimator;

	[CompilerGenerated]
	[SyncVar]
	private int _003CgenericStackIndicatorMax_003Ek__BackingField;

	[CompilerGenerated]
	[SyncVar]
	private int _003CgenericStackIndicatorValue_003Ek__BackingField;

	private static readonly List<ParticleSystem> _pausedSystemsByTeleport;

	private static readonly int DissolveStrength;

	private static readonly int CMBaseColor;

	private static readonly int CMEmission;

	private static readonly int CMOpacity;

	private static readonly int CMDissolveColor;

	private List<EntityColorModifier> _colorModifiers = new List<EntityColorModifier>();

	private bool _isColorModifiersDirty;

	private static readonly int FireStrength;

	private static readonly int ColdStrength;

	private static readonly int LightStrength;

	private static readonly int VoidStrength;

	internal float _eFireTarget;

	internal float _eColdTarget;

	internal float _eVoidTarget;

	internal float _eLightTarget;

	private float _eFire;

	private float _eCold;

	private float _eVoid;

	private float _eLight;

	private List<EntityShellModifier> _shellModifiers = new List<EntityShellModifier>();

	private bool _isShellModifiersDirty;

	public SafeAction ClientEvent_OnSpawnComplete;

	private StatusEffect[] _spawnProtections;

	private EntityTransformModifier _spawnModifier;

	private List<EntityTransformModifier> _transformModifiers = new List<EntityTransformModifier>();

	private bool _isTransformModifiersDirty;

	[NonSerialized]
	public Transform modelTransform;

	private Vector3 _modelOriginalLocalPosition;

	private Vector3 _modelOriginalLocalScale;

	private bool _didHideModelDueToSmallSize;

	public float currentYOffset { get; private set; }

	public float currentYVelocity { get; private set; }

	public EntityHighlightProvider highlight { get; private set; }

	public IReadOnlyList<Material> materials => _materials;

	public bool isRendererOff => _rendererOffCounter > 0;

	public bool isGroundMarkerHidden => _groundMarkerHiddenCounter > 0;

	public int genericStackIndicatorMax
	{
		[CompilerGenerated]
		get
		{
			return _003CgenericStackIndicatorMax_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CgenericStackIndicatorMax_003Ek__BackingField = value;
		}
	}

	public int genericStackIndicatorValue
	{
		[CompilerGenerated]
		get
		{
			return _003CgenericStackIndicatorValue_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			Network_003CgenericStackIndicatorValue_003Ek__BackingField = value;
		}
	}

	bool ICleanup.canDestroy => !_isDissolving;

	public bool isSpawning { get; private set; }

	public Vector3 etWorldOffset { get; private set; }

	public Vector3 etLocalOffset { get; private set; }

	public Vector3 etScaleMultiplier { get; private set; } = Vector3.one;

	public Quaternion etRotation { get; private set; } = Quaternion.identity;

	public bool NetworkskipSpawning
	{
		get
		{
			return skipSpawning;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref skipSpawning, 1uL, null);
		}
	}

	public bool NetworkinvisibleByDefault
	{
		get
		{
			return invisibleByDefault;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref invisibleByDefault, 2uL, null);
		}
	}

	public bool Network_shouldShowStunned
	{
		get
		{
			return _shouldShowStunned;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref _shouldShowStunned, 4uL, OnShouldShowStunnedChanged);
		}
	}

	public int Network_003CgenericStackIndicatorMax_003Ek__BackingField
	{
		get
		{
			return genericStackIndicatorMax;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref genericStackIndicatorMax, 8uL, null);
		}
	}

	public int Network_003CgenericStackIndicatorValue_003Ek__BackingField
	{
		get
		{
			return genericStackIndicatorValue;
		}
		[param: In]
		set
		{
			GeneratedSyncVarSetter(value, ref genericStackIndicatorValue, 16uL, null);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_attachedEffects = DewPool.GetList(out _attachedEffectsHandle);
		_pausedSystemsByRendererOff = DewPool.GetList(out _pausedSystemsByRendererOffHandle);
		highlight = GetComponent<EntityHighlightProvider>();
		_spineAnimator = GetComponentInChildren<FSpineAnimator>();
		if (_spineAnimator != null && _spineAnimator.enabled)
		{
			_spineAnimator.enabled = false;
			ClientEvent_OnSpawnComplete += (Action)delegate
			{
				_spineAnimator.enabled = true;
			};
		}
		ClientEvent_OnRendererEnabledChanged += new Action<bool>(HandleAttachedEffects);
	}

	public override void OnStart()
	{
		base.OnStart();
		base.entity.ClientEntityEvent_OnIsSleepingChanged += (Action<bool>)delegate(bool v)
		{
			if (v)
			{
				DisableRenderersLocal();
			}
			else
			{
				EnableRenderersLocal();
			}
		};
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_attachedEffects != null)
		{
			_attachedEffectsHandle.Return();
			_attachedEffects = null;
		}
		if (_pausedSystemsByRendererOff != null)
		{
			_pausedSystemsByRendererOffHandle.Return();
			_pausedSystemsByRendererOff = null;
		}
	}

	public override void OnLateStartServer()
	{
		base.OnLateStartServer();
		if (!skipSpawning && spawnAnim != null)
		{
			base.entity.Animation.PlayAbilityAnimation(spawnAnim);
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		if (base.entity is Hero h)
		{
			foreach (string acc in h.accessories)
			{
				if (h.owner.IsAllowedToUseItem(acc))
				{
					UnityEngine.Object.Instantiate(DewResources.GetByName<Accessory>(acc)).Setup(base.transform, h.GetType().Name);
				}
			}
		}
		GameObject newEff = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Effects/EntityEffect"), base.transform);
		renderers = new List<Renderer>();
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer ren in componentsInChildren)
		{
			if ((deathEffect != null && IsSelfOrChildOfParent(ren.transform, deathEffect.transform)) || ren.GetComponentInParent<Actor>(includeInactive: true) != base.entity)
			{
				continue;
			}
			if (ren is ParticleSystemRenderer)
			{
				FxParticleSystem fx = ren.GetComponentInParent<FxParticleSystem>();
				if (fx != null && fx.dontDisableAsPartOfEntityRenderer)
				{
					continue;
				}
			}
			renderers.Add(ren);
		}
		if (invisibleByDefault)
		{
			DisableRenderersLocal();
		}
		componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
		_characterRenderers = componentsInChildren;
		if (_characterRenderers.Length == 0)
		{
			componentsInChildren = GetComponentsInChildren<MeshRenderer>();
			_characterRenderers = componentsInChildren;
		}
		solidRenderers = new List<Renderer>();
		solidRenderers.AddRange(GetComponentsInChildren<MeshRenderer>());
		solidRenderers.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>());
		for (int i2 = solidRenderers.Count - 1; i2 >= 0; i2--)
		{
			if (IsSelfOrChildOfParent(solidRenderers[i2].transform, newEff.transform))
			{
				solidRenderers.RemoveAt(i2);
			}
		}
		_materials = new List<Material>();
		foreach (Renderer ren2 in solidRenderers)
		{
			Material[] sharedMats = ren2.sharedMaterials;
			for (int j = 0; j < sharedMats.Length; j++)
			{
				Material newMat = UnityEngine.Object.Instantiate(sharedMats[j]);
				_materials.Add(newMat);
				sharedMats[j] = newMat;
			}
			ren2.sharedMaterials = sharedMats;
		}
		_tails = GetComponentsInChildren<TailAnimator2>();
		_clothes = GetComponentsInChildren<MagicaCloth>();
		EntityVisualPoint[] componentsInChildren2 = GetComponentsInChildren<EntityVisualPoint>(includeInactive: true);
		foreach (EntityVisualPoint point in componentsInChildren2)
		{
			switch (point.type)
			{
			case EntityVisualPointType.Head:
				_apHead = point.transform;
				break;
			case EntityVisualPointType.LeftHand:
				_apLeftHand = point.transform;
				break;
			case EntityVisualPointType.RightHand:
				_apRightHand = point.transform;
				break;
			case EntityVisualPointType.LeftFoot:
				_apLeftFoot = point.transform;
				break;
			case EntityVisualPointType.RightFoot:
				_apRightFoot = point.transform;
				break;
			case EntityVisualPointType.Weapon:
				_apWeapon = point.transform;
				break;
			case EntityVisualPointType.Muzzle:
				_apMuzzle = point.transform;
				break;
			case EntityVisualPointType.Center:
				_apCenter = point.transform;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		_stunEffect = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Effects/Status/Stun"), base.transform);
		DoTransformOnStartClient();
		DoSpawnOnStartClient();
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		base.entity.EntityEvent_OnTakeDamage += new Action<EventInfoDamage>(HandleTakeDamage);
		base.entity.EntityEvent_OnDeath += new Action<EventInfoKill>(HandleDeath);
	}

	private void HandleDeath(EventInfoKill obj)
	{
		RpcHandleDeath(new GibInfo
		{
			normalizedCurrentDamage = _currentDamage / base.entity.maxHealth,
			velocity = base.entity.AI.estimatedVelocityUnclamped,
			yVelocity = base.entity.Visual.currentYVelocity
		});
	}

	[ClientRpc]
	private void RpcHandleDeath(GibInfo gibInfo)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_GibInfo(writer, gibInfo);
		SendRPCInternal("System.Void EntityVisual::RpcHandleDeath(GibInfo)", -1082789707, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private IEnumerator RoutineAnimateDissolve(float delay, float duration)
	{
		_isDissolving = true;
		EntityColorModifier colorMod = GetNewColorModifier();
		float factor = 1f / duration;
		yield return new WaitForSeconds(delay);
		for (float v = 0f; v <= 1f; v += Time.deltaTime * factor)
		{
			colorMod.dissolveAmount = v;
			yield return null;
		}
		colorMod.dissolveAmount = 1f;
		_isDissolving = false;
	}

	public override void OnStop()
	{
		base.OnStop();
		if (_materials == null)
		{
			return;
		}
		foreach (Material material in _materials)
		{
			UnityEngine.Object.Destroy(material);
		}
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		base.entity.EntityEvent_OnTakeDamage -= new Action<EventInfoDamage>(HandleTakeDamage);
		base.entity.EntityEvent_OnDeath -= new Action<EventInfoKill>(HandleDeath);
	}

	public override void FrameUpdate()
	{
		base.FrameUpdate();
		if (base.entity.isSleeping)
		{
			return;
		}
		currentYOffset += currentYVelocity * Time.deltaTime;
		if (currentYOffset > 0f)
		{
			currentYVelocity -= 18.8f * Time.deltaTime;
			if (currentYOffset > 5f && currentYVelocity > 0f)
			{
				currentYVelocity -= 18.8f * Time.deltaTime;
			}
		}
		if (currentYOffset < 0f)
		{
			currentYOffset = 0f;
			currentYVelocity = 0f;
		}
		DoElementalFrameUpdate();
		DoColorFrameUpdate();
		DoTransformFrameUpdate();
		DoShellFrameUpdate();
	}

	public override void LogicUpdate(float dt)
	{
		base.LogicUpdate(dt);
		if (!base.entity.isSleeping)
		{
			DoElementalLogicUpdate();
			DoStunLogicUpdate();
			_currentDamage *= 0.95f;
		}
	}

	[ClientRpc]
	public void SetYOffset(float offset)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(offset);
		SendRPCInternal("System.Void EntityVisual::SetYOffset(System.Single)", -1151011751, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public void SetYOffsetLocal(float offset)
	{
		currentYOffset = offset;
		DoTransformFrameUpdate();
	}

	[ClientRpc]
	public void SetYVelocity(float velocity)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(velocity);
		SendRPCInternal("System.Void EntityVisual::SetYVelocity(System.Single)", -1982953905, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public void SetYVelocityLocal(float velocity)
	{
		currentYVelocity = velocity;
	}

	private void DoStunLogicUpdate()
	{
		if (base.isServer)
		{
			Network_shouldShowStunned = base.entity.Status.hasStun;
		}
	}

	private void OnShouldShowStunnedChanged(bool oldVal, bool newVal)
	{
		if (newVal)
		{
			FxPlay(_stunEffect, base.entity);
		}
		else
		{
			FxStop(_stunEffect);
		}
	}

	private void HandleTakeDamage(EventInfoDamage info)
	{
		if (info.damage.direction.HasValue)
		{
			_currentDamage += (info.damage.amount + info.damage.discardedAmount) * info.damage.direction.Value;
		}
		RpcShowHitEffect();
		if (takeDamageEffect != null)
		{
			FxPlayNewNetworked(takeDamageEffect, base.entity);
		}
	}

	[ClientRpc]
	private void RpcShowHitEffect()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void EntityVisual::RpcShowHitEffect()", -1699861942, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void KnockUpLocal(float strength, bool isFriendly)
	{
		if (isFriendly || !base.entity.Status.hasCrowdControlImmunity)
		{
			if (currentYVelocity > 0.25f)
			{
				strength *= 0.66f;
			}
			if (currentYVelocity > 1f)
			{
				strength *= 0.66f;
			}
			if (currentYVelocity < 0f)
			{
				currentYVelocity *= 0.25f;
			}
			currentYVelocity += strength * 6f;
		}
	}

	void ICleanup.OnCleanup()
	{
		RpcCleanup();
	}

	[ClientRpc]
	private void RpcCleanup()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void EntityVisual::RpcCleanup()", 1675077287, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private bool IsSelfOrChildOfParent(Transform child, Transform parent)
	{
		while (true)
		{
			if (child == null)
			{
				return false;
			}
			if (child == parent)
			{
				break;
			}
			child = child.parent;
		}
		return true;
	}

	internal void DoActionBeforeTeleport()
	{
		base.entity.Visual.PauseAttachedParticleSystems();
	}

	internal void DoActionAfterTeleport()
	{
		base.entity.Visual.ResumeAttachedParticleSystems();
		FixTailsAndClothes();
	}

	public void FixTailsAndClothes()
	{
		FixTails();
		FixClothes();
	}

	public void FixTails()
	{
		TailAnimator2[] tails = _tails;
		foreach (TailAnimator2 t in tails)
		{
			try
			{
				t.User_ReposeTail();
			}
			catch (Exception)
			{
			}
		}
	}

	public void FixClothes()
	{
		MagicaCloth[] clothes = _clothes;
		foreach (MagicaCloth c in clothes)
		{
			try
			{
				c.ResetCloth();
			}
			catch (Exception)
			{
			}
		}
	}

	internal void PauseAttachedParticleSystems()
	{
		_pausedSystemsByTeleport.Clear();
		ListReturnHandle<ParticleSystem> handle;
		List<ParticleSystem> pses = DewPool.GetList(out handle);
		foreach (FxAttachToEntity attachedEffect in _attachedEffects)
		{
			attachedEffect.GetComponentsInChildren(pses);
			foreach (ParticleSystem p in pses)
			{
				if (p.isPlaying && !p.isPaused && p.isEmitting)
				{
					FxParticleSystem data = p.GetComponentInParent<FxParticleSystem>();
					if (!(data != null) || !data.dontPauseAttachedWhenTeleport)
					{
						p.Pause(withChildren: true);
						_pausedSystemsByTeleport.Add(p);
					}
				}
			}
		}
		handle.Return();
	}

	internal void ResumeAttachedParticleSystems()
	{
		foreach (ParticleSystem p in _pausedSystemsByTeleport)
		{
			if (p != null)
			{
				p.Play();
			}
		}
		_pausedSystemsByTeleport.Clear();
	}

	private void HandleAttachedEffects(bool isRendererOn)
	{
		if (_pausedSystemsByRendererOff == null)
		{
			return;
		}
		if (isRendererOn)
		{
			foreach (ParticleSystem p in _pausedSystemsByRendererOff)
			{
				if (!(p == null))
				{
					ParticleSystem.EmissionModule e = p.emission;
					e.enabled = true;
				}
			}
			_pausedSystemsByRendererOff.Clear();
			return;
		}
		foreach (FxAttachToEntity attachedEffect in _attachedEffects)
		{
			ListReturnHandle<ParticleSystem> handle;
			foreach (ParticleSystem p2 in ((Component)attachedEffect).GetComponentsInChildrenNonAlloc(out handle))
			{
				if (!p2.isPlaying || p2.isPaused || !p2.isEmitting)
				{
					continue;
				}
				FxParticleSystem data = p2.GetComponentInParent<FxParticleSystem>();
				if (!(data != null) || !data.dontPauseAttachedWhenRendererDisabled)
				{
					ParticleSystem.EmissionModule e2 = p2.emission;
					if (e2.enabled)
					{
						e2.enabled = false;
						_pausedSystemsByRendererOff.Add(p2);
					}
				}
			}
			handle.Return();
		}
	}

	public EntityColorModifier GetNewColorModifier()
	{
		EntityColorModifier mod = new EntityColorModifier();
		_colorModifiers.Add(mod);
		mod._parent = base.entity;
		return mod;
	}

	internal void RemoveColorModifier(EntityColorModifier modifier)
	{
		if (_colorModifiers.Remove(modifier))
		{
			_isColorModifiersDirty = true;
		}
	}

	internal void DirtyColorModifiers()
	{
		_isColorModifiersDirty = true;
	}

	private void DoColorFrameUpdate()
	{
		if (!_isColorModifiersDirty)
		{
			return;
		}
		_isColorModifiersDirty = false;
		Color baseColor = Color.white;
		Color emission = Color.black;
		float emissionBiggestMag = 0f;
		float emissionMagSum = 0f;
		float dissolveAmount = 0f;
		float opacity = 1f;
		Color dissolveColor = (hasGoldDissolve ? new Color(1f, 0.8f, 0.3f) : new Color(0.4f, 0.85f, 1f));
		foreach (EntityColorModifier m in _colorModifiers)
		{
			baseColor *= m.baseColor;
			float emissionMag = GetMagnitude(m.emission);
			if (emissionMag > emissionBiggestMag)
			{
				emissionBiggestMag = emissionMag;
			}
			emission += m.emission;
			emissionMagSum += emissionMag;
			dissolveAmount = Mathf.Max(m.dissolveAmount, dissolveAmount);
			opacity = Mathf.Min(m.opacity, opacity);
			if (m.dissolveColor.HasValue)
			{
				dissolveColor = m.dissolveColor.Value;
			}
		}
		emission = ((!(emissionMagSum < 0.0001f)) ? (emission / emissionMagSum * emissionBiggestMag) : Color.black);
		dissolveAmount = Mathf.Clamp(dissolveAmount, 0f, 1f);
		SetShaderPropertyLocal(CMBaseColor, baseColor);
		SetShaderPropertyLocal(CMEmission, emission);
		SetShaderPropertyLocal(CMOpacity, opacity);
		SetShaderPropertyLocal(DissolveStrength, dissolveAmount);
		SetShaderPropertyLocal(CMDissolveColor, dissolveColor);
		static float GetMagnitude(Color c)
		{
			return Vector3.Magnitude(new Vector3(c.r, c.g, c.b));
		}
	}

	private void DoElementalLogicUpdate()
	{
		if (!base.entity.Status.isDead)
		{
			_eFireTarget = ((base.entity.Status.fireStack > 0) ? 1 : 0);
			_eColdTarget = (base.entity.Status.hasCold ? 1 : 0);
			_eVoidTarget = base.entity.Status.darkStack;
			_eLightTarget = base.entity.Status.lightStack;
		}
	}

	private void DoElementalFrameUpdate()
	{
		if (Mathf.Abs(_eFireTarget - _eFire) > 0.0001f)
		{
			_eFire = Mathf.MoveTowards(_eFire, _eFireTarget, 8f * Time.deltaTime);
			SetShaderPropertyLocal(FireStrength, _eFire);
		}
		if (Mathf.Abs(_eColdTarget - _eCold) > 0.0001f)
		{
			_eCold = Mathf.MoveTowards(_eCold, _eColdTarget, 8f * Time.deltaTime);
			SetShaderPropertyLocal(ColdStrength, _eCold);
		}
		if (Mathf.Abs(_eVoidTarget - _eVoid) > 0.0001f)
		{
			_eVoid = Mathf.MoveTowards(_eVoid, _eVoidTarget, 8f * Time.deltaTime);
			SetShaderPropertyLocal(VoidStrength, _eVoid);
		}
		if (Mathf.Abs(_eLightTarget - _eLight) > 0.0001f)
		{
			_eLight = Mathf.MoveTowards(_eLight, _eLightTarget, 8f * Time.deltaTime);
			SetShaderPropertyLocal(LightStrength, _eLight);
		}
	}

	[ClientRpc]
	public void SetShaderProperty(string key, float value)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(key);
		writer.WriteFloat(value);
		SendRPCInternal("System.Void EntityVisual::SetShaderProperty(System.String,System.Single)", -597797103, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public void SetShaderPropertyLocal(string key, float value)
	{
		for (int i = 0; i < _materials.Count; i++)
		{
			_materials[i].SetFloat(key, value);
		}
	}

	public void SetShaderPropertyLocal(int propertyId, float value)
	{
		for (int i = 0; i < _materials.Count; i++)
		{
			_materials[i].SetFloat(propertyId, value);
		}
	}

	[ClientRpc]
	public void SetShaderProperty(string key, Color value)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteString(key);
		writer.WriteColor(value);
		SendRPCInternal("System.Void EntityVisual::SetShaderProperty(System.String,UnityEngine.Color)", -1542144884, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public void SetShaderPropertyLocal(string key, Color value)
	{
		for (int i = 0; i < _materials.Count; i++)
		{
			_materials[i].SetColor(key, value);
		}
	}

	public void SetShaderPropertyLocal(int propertyId, Color value)
	{
		for (int i = 0; i < _materials.Count; i++)
		{
			_materials[i].SetColor(propertyId, value);
		}
	}

	[ClientRpc]
	public void KnockUp(float strength, bool isFriendly)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteFloat(strength);
		writer.WriteBool(isFriendly);
		SendRPCInternal("System.Void EntityVisual::KnockUp(System.Single,System.Boolean)", 242751419, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	public void KnockUp(KnockUpStrength strength, bool isFriendly)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		GeneratedNetworkCode._Write_KnockUpStrength(writer, strength);
		writer.WriteBool(isFriendly);
		SendRPCInternal("System.Void EntityVisual::KnockUp(KnockUpStrength,System.Boolean)", 628801700, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[Server]
	public void DisableRenderers()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityVisual::DisableRenderers()' called when server was not active");
			return;
		}
		DisableRenderersLocal();
		RpcDisableRenderers();
	}

	[ClientRpc]
	private void RpcDisableRenderers()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void EntityVisual::RpcDisableRenderers()", 1936931655, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public void DisableRenderersLocal()
	{
		if (_rendererOffCounter == 255)
		{
			Debug.LogWarning("You're not supposed to disable renderers more than 255 times at once: " + base.entity.GetActorReadableName());
		}
		_rendererOffCounter++;
		if (_rendererOffCounter == 1)
		{
			foreach (Renderer ren in renderers)
			{
				if (!(ren == null))
				{
					ren.enabled = false;
				}
			}
			ClientEvent_OnRendererEnabledChanged?.Invoke(arg: false);
		}
		if (_rendererOffCounter == 0)
		{
			Debug.Log("Negative renderer off counter resolved: " + base.entity.GetActorReadableName());
		}
	}

	[Server]
	public void EnableRenderers()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityVisual::EnableRenderers()' called when server was not active");
			return;
		}
		EnableRenderersLocal();
		RpcEnableRenderers();
	}

	[ClientRpc]
	private void RpcEnableRenderers()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void EntityVisual::RpcEnableRenderers()", 813325712, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public void EnableRenderersLocal()
	{
		if (_rendererOffCounter == 0)
		{
			Debug.LogWarning("Enabled renderers of an already visible entity: " + base.entity.GetActorReadableName());
		}
		_rendererOffCounter--;
		if (_rendererOffCounter != 0)
		{
			return;
		}
		foreach (Renderer ren in renderers)
		{
			if (!(ren == null))
			{
				ren.enabled = true;
			}
		}
		ClientEvent_OnRendererEnabledChanged?.Invoke(arg: true);
		DoTransformFrameUpdate();
	}

	public void AddSharedMaterialLocal(Material material)
	{
		foreach (Renderer solidRenderer in solidRenderers)
		{
			Material[] mats = new Material[solidRenderer.sharedMaterials.Length + 1];
			solidRenderer.sharedMaterials.CopyTo(mats, 0);
			mats[^1] = material;
			solidRenderer.sharedMaterials = mats;
		}
	}

	public void RemoveMaterialLocal(Material material)
	{
		bool removed = false;
		foreach (Renderer ren in solidRenderers)
		{
			if (!ren.sharedMaterials.Contains(material))
			{
				return;
			}
			List<Material> mats = ren.sharedMaterials.ToList();
			mats.RemoveAll((Material m) => m == material);
			ren.sharedMaterials = mats.ToArray();
			removed = true;
		}
		if (!removed)
		{
			Debug.LogWarning($"Couldn't find {material} from {this}");
		}
	}

	public Bounds GetBodyBounds()
	{
		if (bodyRenderers == null || bodyRenderers.Length == 0)
		{
			return GetRenderBounds();
		}
		Bounds result = bodyRenderers[0].bounds;
		for (int i = 1; i < bodyRenderers.Length; i++)
		{
			result.Encapsulate(bodyRenderers[i].bounds);
		}
		return result;
	}

	public Bounds GetRenderBounds()
	{
		if (_characterRenderers == null || _characterRenderers.Length == 0 || _characterRenderers[0] == null)
		{
			return new Bounds(base.transform.position, Vector3.zero);
		}
		Bounds result = _characterRenderers[0].bounds;
		for (int i = 1; i < _characterRenderers.Length; i++)
		{
			if (!(_characterRenderers[i] == null))
			{
				result.Encapsulate(_characterRenderers[i].bounds);
			}
		}
		return result;
	}

	public Vector3 GetBasePosition()
	{
		return base.transform.position + currentYOffset * Vector3.up;
	}

	public Vector3 GetCenterPosition()
	{
		if (_apCenter != null)
		{
			return _apCenter.position;
		}
		return GetBodyBounds().center;
	}

	public Vector3 GetAbovePosition()
	{
		Bounds bounds = GetBodyBounds();
		return bounds.center + Vector3.up * bounds.extents.y;
	}

	public Vector3 GetMuzzlePosition()
	{
		if (_apMuzzle == null)
		{
			Debug.LogWarning($"'{this}' does not have muzzle point configured");
			return GetCenterPosition();
		}
		return _apMuzzle.transform.position;
	}

	public Quaternion GetMuzzleRotation()
	{
		if (_apMuzzle == null)
		{
			Debug.LogWarning($"'{this}' does not have muzzle point configured");
			return base.transform.rotation;
		}
		return _apMuzzle.transform.rotation;
	}

	public Vector3 GetWeaponPosition()
	{
		if (_apWeapon == null)
		{
			Debug.LogWarning($"'{this}' does not have weapon point configured");
			return GetCenterPosition();
		}
		return _apWeapon.transform.position;
	}

	public Quaternion GetWeaponRotation()
	{
		if (_apWeapon == null)
		{
			Debug.LogWarning($"'{this}' does not have weapon point configured");
			return base.transform.rotation;
		}
		return _apWeapon.transform.rotation;
	}

	public Vector3 GetBonePosition(HumanBodyBones bone)
	{
		if (bone == HumanBodyBones.Head && _apHead != null)
		{
			return _apHead.position;
		}
		if (bone == HumanBodyBones.LeftHand && _apLeftHand != null)
		{
			return _apLeftHand.position;
		}
		if (bone == HumanBodyBones.RightHand && _apRightHand != null)
		{
			return _apRightHand.position;
		}
		if (bone == HumanBodyBones.LeftFoot && _apLeftFoot != null)
		{
			return _apLeftFoot.position;
		}
		if (bone == HumanBodyBones.RightFoot && _apRightFoot != null)
		{
			return _apRightFoot.position;
		}
		if (base.entity.Animation.animator == null)
		{
			return GetCenterPosition();
		}
		Transform tr = base.entity.Animation.animator.GetBoneTransform(bone);
		if (tr == null)
		{
			return GetCenterPosition();
		}
		return tr.position;
	}

	public Quaternion GetBoneRotation(HumanBodyBones bone)
	{
		if (bone == HumanBodyBones.Head && _apHead != null)
		{
			return _apHead.rotation;
		}
		if (bone == HumanBodyBones.LeftHand && _apLeftHand != null)
		{
			return _apLeftHand.rotation;
		}
		if (bone == HumanBodyBones.RightHand && _apRightHand != null)
		{
			return _apRightHand.rotation;
		}
		if (bone == HumanBodyBones.LeftFoot && _apLeftFoot != null)
		{
			return _apLeftFoot.rotation;
		}
		if (bone == HumanBodyBones.RightFoot && _apRightFoot != null)
		{
			return _apRightFoot.rotation;
		}
		if (base.entity.Animation.animator == null)
		{
			return base.transform.rotation;
		}
		Transform tr = base.entity.Animation.animator.GetBoneTransform(bone);
		if (tr == null)
		{
			return base.transform.rotation;
		}
		return tr.rotation;
	}

	[Server]
	public void HideGroundMarker()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityVisual::HideGroundMarker()' called when server was not active");
			return;
		}
		HideGroundMarkerLocal();
		RpcHideGroundMarker();
	}

	[ClientRpc]
	private void RpcHideGroundMarker()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void EntityVisual::RpcHideGroundMarker()", -975010430, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public void HideGroundMarkerLocal()
	{
		_groundMarkerHiddenCounter++;
		if (_groundMarkerHiddenCounter == 1)
		{
			ClientEvent_OnGroundMarkerHiddenChanged?.Invoke(arg: true);
		}
	}

	[Server]
	public void ShowGroundMarker()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityVisual::ShowGroundMarker()' called when server was not active");
			return;
		}
		ShowGroundMarkerLocal();
		RpcShowGroundMarker();
	}

	[ClientRpc]
	private void RpcShowGroundMarker()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void EntityVisual::RpcShowGroundMarker()", -365993987, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	public void ShowGroundMarkerLocal()
	{
		_groundMarkerHiddenCounter--;
		if (_groundMarkerHiddenCounter == 0)
		{
			ClientEvent_OnGroundMarkerHiddenChanged?.Invoke(arg: false);
		}
	}

	public EntityShellModifier GetNewShellModifier()
	{
		EntityShellModifier mod = new EntityShellModifier();
		_shellModifiers.Add(mod);
		mod._parent = base.entity;
		return mod;
	}

	internal void RemoveShellModifier(EntityShellModifier modifier)
	{
		if (_shellModifiers.Remove(modifier))
		{
			_isShellModifiersDirty = true;
		}
	}

	internal void DirtyShellModifiers()
	{
		_isShellModifiersDirty = true;
	}

	private void DoShellFrameUpdate()
	{
		if (!_isShellModifiersDirty)
		{
			return;
		}
		_isShellModifiersDirty = false;
		Color color = Color.black;
		float opacity = 0f;
		foreach (EntityShellModifier m in _shellModifiers)
		{
			color += m.color * m.opacity;
			opacity += m.opacity;
		}
		if (opacity > 0.001f)
		{
			color /= opacity;
			color.a = 0f;
			highlight.meshHighlight.glowPasses[0].color = color * 1f;
		}
		highlight.meshHighlight.glow = Mathf.Clamp01(opacity);
	}

	[Server]
	public void SkipSpawning()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void EntityVisual::SkipSpawning()' called when server was not active");
		}
		else
		{
			if (!isSpawning)
			{
				return;
			}
			NetworkskipSpawning = true;
			base.entity.Control.CancelOngoingChannels();
			if (_spawnProtections != null)
			{
				StatusEffect[] spawnProtections = _spawnProtections;
				foreach (StatusEffect s in spawnProtections)
				{
					if (!s.IsNullOrInactive())
					{
						s.Destroy();
					}
				}
			}
			RpcSkipSpawning();
		}
	}

	[ClientRpc]
	private void RpcSkipSpawning()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void EntityVisual::RpcSkipSpawning()", -486594075, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private IEnumerator WaitForSecondsSpawn(float duration)
	{
		float startTime = Time.time;
		while (!skipSpawning && Time.time - startTime < duration)
		{
			yield return null;
		}
	}

	private void DoSpawnOnStartClient()
	{
		if (loopEffect != null)
		{
			StartCoroutine(Routine());
		}
		if (skipSpawning)
		{
			return;
		}
		if (spawnEffect != null)
		{
			StartCoroutine(Routine2());
		}
		if (spawnEffectOnGround != null)
		{
			StartCoroutine(Routine3());
		}
		if (spawnDuration + dazeAfterSpawnDuration > 0f && base.isServer)
		{
			base.entity.Control.StartDaze(spawnDuration + dazeAfterSpawnDuration);
		}
		if (spawnDuration > 0f)
		{
			if (base.isServer && invulnerableWhileSpawning)
			{
				_spawnProtections = new StatusEffect[3]
				{
					base.entity.CreateBasicEffect(base.entity, new InvulnerableEffect(), spawnDuration),
					base.entity.CreateBasicEffect(base.entity, new UncollidableEffect(), spawnDuration),
					base.entity.CreateBasicEffect(base.entity, new InvisibleEffect(), spawnDuration)
				};
			}
			StartCoroutine(SetIsSpawningFlag());
		}
		if (doPositionOffset || doScaling)
		{
			_spawnModifier = GetNewTransformModifier();
			StartCoroutine(Animate());
		}
		if (skipSpawning)
		{
			ClientEvent_OnSpawnComplete?.Invoke();
		}
		else
		{
			StartCoroutine(Routine4());
		}
		IEnumerator Animate()
		{
			for (float t = 0f; t < spawnDuration; t += Time.deltaTime)
			{
				float val = t / spawnDuration;
				if (!isSpawning || skipSpawning)
				{
					break;
				}
				if (doPositionOffset)
				{
					Vector3 offset = new Vector3(spawnXOffset.Evaluate(val), spawnYOffset.Evaluate(val), spawnZOffset.Evaluate(val));
					_spawnModifier.localOffset = offset;
				}
				if (doScaling)
				{
					float xScale = spawnXScale.Evaluate(val);
					Vector3 scale = (useSeparateAxis ? new Vector3(xScale, spawnYScale.Evaluate(val), spawnZScale.Evaluate(val)) : new Vector3(xScale, xScale, xScale));
					_spawnModifier.scaleMultiplier = scale;
				}
				if (t == 0f)
				{
					DoTransformFrameUpdate();
				}
				yield return null;
			}
			_spawnModifier.Stop();
			_spawnModifier = null;
		}
		IEnumerator Routine()
		{
			if (!skipSpawning && delayLoopEffectTilSpawned && spawnDuration > 0f)
			{
				yield return WaitForSecondsSpawn(spawnDuration);
			}
			else
			{
				yield return null;
			}
			if (!base.entity.IsNullInactiveDeadOrKnockedOut())
			{
				FxPlay(loopEffect, base.entity);
			}
		}
		IEnumerator Routine2()
		{
			if (delaySpawnEffectUntilSpawned && spawnDuration > 0f)
			{
				yield return WaitForSecondsSpawn(spawnDuration);
			}
			else
			{
				yield return null;
			}
			if (!base.entity.IsNullInactiveDeadOrKnockedOut())
			{
				FxPlay(spawnEffect, base.entity);
			}
		}
		IEnumerator Routine3()
		{
			if (delaySpawnEffectOnGroundTilSpawned && spawnDuration > 0f)
			{
				yield return WaitForSecondsSpawn(spawnDuration);
			}
			else
			{
				yield return null;
			}
			if (!base.entity.IsNullInactiveDeadOrKnockedOut())
			{
				FxPlay(spawnEffectOnGround, Dew.GetPositionOnGround(base.entity.position), base.entity.rotation);
			}
		}
		IEnumerator Routine4()
		{
			yield return WaitForSecondsSpawn(spawnDuration);
			yield return null;
			ClientEvent_OnSpawnComplete?.Invoke();
		}
		IEnumerator SetIsSpawningFlag()
		{
			isSpawning = true;
			for (float t = 0f; t < spawnDuration; t += Time.deltaTime)
			{
				if (base.entity.IsNullInactiveDeadOrKnockedOut() || base.entity.Control.isDisplacing || skipSpawning)
				{
					if (base.isServer && spawnAnim != null)
					{
						base.entity.Animation.StopAbilityAnimation(spawnAnim);
					}
					isSpawning = false;
					yield break;
				}
				yield return null;
			}
			isSpawning = false;
		}
	}

	private void DoTransformOnStartClient()
	{
		if (base.entity.Animation.animator != null)
		{
			modelTransform = base.entity.Animation.animator.transform;
		}
		else
		{
			modelTransform = base.transform.Find("Model");
			if (modelTransform == null)
			{
				TryFindModelTransform<Animator>();
			}
			if (modelTransform == null)
			{
				TryFindModelTransform<SkinnedMeshRenderer>();
			}
			if (modelTransform == null)
			{
				TryFindModelTransform<MeshRenderer>();
			}
		}
		if (modelTransform != null)
		{
			_modelOriginalLocalPosition = modelTransform.localPosition;
			_modelOriginalLocalScale = modelTransform.localScale;
		}
	}

	private void TryFindModelTransform<T>() where T : Component
	{
		T c = base.transform.GetComponentInChildren<T>();
		if (c != null)
		{
			modelTransform = c.transform;
		}
	}

	public EntityTransformModifier GetNewTransformModifier()
	{
		EntityTransformModifier mod = new EntityTransformModifier();
		_transformModifiers.Add(mod);
		mod._parent = base.entity;
		return mod;
	}

	internal void RemoveTransformModifier(EntityTransformModifier modifier)
	{
		if (_transformModifiers.Remove(modifier))
		{
			_isTransformModifiersDirty = true;
		}
	}

	internal void DirtyTransformModifiers()
	{
		_isTransformModifiersDirty = true;
	}

	private void DoTransformFrameUpdate()
	{
		if (modelTransform == null)
		{
			return;
		}
		bool needClothRefresh = false;
		if (_isTransformModifiersDirty)
		{
			bool lastScaleIsSmall = etScaleMultiplier.sqrMagnitude < 0.25f;
			etWorldOffset = Vector3.zero;
			etLocalOffset = Vector3.zero;
			etScaleMultiplier = Vector3.one;
			etRotation = Quaternion.identity;
			foreach (EntityTransformModifier m in _transformModifiers)
			{
				etWorldOffset += m.worldOffset;
				etLocalOffset += m.localOffset;
				etScaleMultiplier = Vector3.Scale(etScaleMultiplier, m.scaleMultiplier);
				etRotation *= m.rotation;
			}
			if (lastScaleIsSmall && etScaleMultiplier.sqrMagnitude > 0.25f)
			{
				needClothRefresh = true;
			}
			_isTransformModifiersDirty = false;
		}
		Vector3 lastPos = modelTransform.position;
		Transform et = base.transform;
		Vector3 pos = et.position + et.rotation * (_modelOriginalLocalPosition + etLocalOffset) + etWorldOffset + Vector3.up * currentYOffset;
		modelTransform.position = pos;
		modelTransform.localRotation = etRotation;
		Vector3 clampedScale = etScaleMultiplier;
		if (clampedScale.x < 0.1f || clampedScale.y < 0.1f || clampedScale.z < 0.1f)
		{
			if (!_didHideModelDueToSmallSize)
			{
				_didHideModelDueToSmallSize = true;
				DisableRenderersLocal();
			}
			if (clampedScale.x < 0.1f)
			{
				clampedScale = clampedScale.WithX(0.1f);
			}
			if (clampedScale.y < 0.1f)
			{
				clampedScale = clampedScale.WithY(0.1f);
			}
			if (clampedScale.z < 0.1f)
			{
				clampedScale = clampedScale.WithZ(0.1f);
			}
		}
		else if (_didHideModelDueToSmallSize)
		{
			_didHideModelDueToSmallSize = false;
			EnableRenderersLocal();
		}
		modelTransform.localScale = Vector3.Scale(_modelOriginalLocalScale, clampedScale);
		if (!needClothRefresh && Vector3.Distance(lastPos, modelTransform.position) > 1f)
		{
			needClothRefresh = true;
		}
		if (needClothRefresh)
		{
			FixTailsAndClothes();
		}
	}

	static EntityVisual()
	{
		_pausedSystemsByTeleport = new List<ParticleSystem>(128);
		DissolveStrength = Shader.PropertyToID("_DissolveStrength");
		CMBaseColor = Shader.PropertyToID("_CMBaseColor");
		CMEmission = Shader.PropertyToID("_CMEmission");
		CMOpacity = Shader.PropertyToID("_CMOpacity");
		CMDissolveColor = Shader.PropertyToID("_CMDissolveColor");
		FireStrength = Shader.PropertyToID("_FireStrength");
		ColdStrength = Shader.PropertyToID("_ColdStrength");
		LightStrength = Shader.PropertyToID("_LightStrength");
		VoidStrength = Shader.PropertyToID("_VoidStrength");
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::RpcHandleDeath(GibInfo)", InvokeUserCode_RpcHandleDeath__GibInfo);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::SetYOffset(System.Single)", InvokeUserCode_SetYOffset__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::SetYVelocity(System.Single)", InvokeUserCode_SetYVelocity__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::RpcShowHitEffect()", InvokeUserCode_RpcShowHitEffect);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::RpcCleanup()", InvokeUserCode_RpcCleanup);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::SetShaderProperty(System.String,System.Single)", InvokeUserCode_SetShaderProperty__String__Single);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::SetShaderProperty(System.String,UnityEngine.Color)", InvokeUserCode_SetShaderProperty__String__Color);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::KnockUp(System.Single,System.Boolean)", InvokeUserCode_KnockUp__Single__Boolean);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::KnockUp(KnockUpStrength,System.Boolean)", InvokeUserCode_KnockUp__KnockUpStrength__Boolean);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::RpcDisableRenderers()", InvokeUserCode_RpcDisableRenderers);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::RpcEnableRenderers()", InvokeUserCode_RpcEnableRenderers);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::RpcHideGroundMarker()", InvokeUserCode_RpcHideGroundMarker);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::RpcShowGroundMarker()", InvokeUserCode_RpcShowGroundMarker);
		RemoteProcedureCalls.RegisterRpc(typeof(EntityVisual), "System.Void EntityVisual::RpcSkipSpawning()", InvokeUserCode_RpcSkipSpawning);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcHandleDeath__GibInfo(GibInfo gibInfo)
	{
		switch (deathBehavior)
		{
		case EntityDeathBehavior.HideModel:
			DisableRenderersLocal();
			break;
		case EntityDeathBehavior.Dissolve:
			StartCoroutine(RoutineAnimateDissolve(dissolveDelay, dissolveDuration));
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case EntityDeathBehavior.None:
			break;
		}
		if (deathEffect != null)
		{
			FxGibs[] componentsInChildren = deathEffect.GetComponentsInChildren<FxGibs>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].info = gibInfo;
			}
			FxPlayNew(deathEffect, base.entity);
		}
		if (loopEffect != null)
		{
			FxStop(loopEffect);
		}
	}

	protected static void InvokeUserCode_RpcHandleDeath__GibInfo(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcHandleDeath called on server.");
		}
		else
		{
			((EntityVisual)obj).UserCode_RpcHandleDeath__GibInfo(GeneratedNetworkCode._Read_GibInfo(reader));
		}
	}

	protected void UserCode_SetYOffset__Single(float offset)
	{
		SetYOffsetLocal(offset);
	}

	protected static void InvokeUserCode_SetYOffset__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC SetYOffset called on server.");
		}
		else
		{
			((EntityVisual)obj).UserCode_SetYOffset__Single(reader.ReadFloat());
		}
	}

	protected void UserCode_SetYVelocity__Single(float velocity)
	{
		SetYVelocityLocal(velocity);
	}

	protected static void InvokeUserCode_SetYVelocity__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC SetYVelocity called on server.");
		}
		else
		{
			((EntityVisual)obj).UserCode_SetYVelocity__Single(reader.ReadFloat());
		}
	}

	protected void UserCode_RpcShowHitEffect()
	{
		highlight.ShowHit();
	}

	protected static void InvokeUserCode_RpcShowHitEffect(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowHitEffect called on server.");
		}
		else
		{
			((EntityVisual)obj).UserCode_RpcShowHitEffect();
		}
	}

	protected void UserCode_RpcCleanup()
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			while (_isDissolving && this != null)
			{
				yield return null;
			}
			if (!(this == null))
			{
				if (loopEffect != null)
				{
					FxStop(loopEffect);
				}
				DisableRenderersLocal();
			}
		}
	}

	protected static void InvokeUserCode_RpcCleanup(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCleanup called on server.");
		}
		else
		{
			((EntityVisual)obj).UserCode_RpcCleanup();
		}
	}

	protected void UserCode_SetShaderProperty__String__Single(string key, float value)
	{
		SetShaderPropertyLocal(key, value);
	}

	protected static void InvokeUserCode_SetShaderProperty__String__Single(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC SetShaderProperty called on server.");
		}
		else
		{
			((EntityVisual)obj).UserCode_SetShaderProperty__String__Single(reader.ReadString(), reader.ReadFloat());
		}
	}

	protected void UserCode_SetShaderProperty__String__Color(string key, Color value)
	{
		SetShaderPropertyLocal(key, value);
	}

	protected static void InvokeUserCode_SetShaderProperty__String__Color(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC SetShaderProperty called on server.");
		}
		else
		{
			((EntityVisual)obj).UserCode_SetShaderProperty__String__Color(reader.ReadString(), reader.ReadColor());
		}
	}

	protected void UserCode_KnockUp__Single__Boolean(float strength, bool isFriendly)
	{
		KnockUpLocal(strength, isFriendly);
	}

	protected static void InvokeUserCode_KnockUp__Single__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC KnockUp called on server.");
		}
		else
		{
			((EntityVisual)obj).UserCode_KnockUp__Single__Boolean(reader.ReadFloat(), reader.ReadBool());
		}
	}

	protected void UserCode_KnockUp__KnockUpStrength__Boolean(KnockUpStrength strength, bool isFriendly)
	{
		KnockUpLocal(strength switch
		{
			KnockUpStrength.Small => 0.8f, 
			KnockUpStrength.Normal => 1.1f, 
			KnockUpStrength.Big => 1.5f, 
			_ => throw new ArgumentOutOfRangeException("strength", strength, null), 
		}, isFriendly);
	}

	protected static void InvokeUserCode_KnockUp__KnockUpStrength__Boolean(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC KnockUp called on server.");
		}
		else
		{
			((EntityVisual)obj).UserCode_KnockUp__KnockUpStrength__Boolean(GeneratedNetworkCode._Read_KnockUpStrength(reader), reader.ReadBool());
		}
	}

	protected void UserCode_RpcDisableRenderers()
	{
		if (!base.isServer)
		{
			DisableRenderersLocal();
		}
	}

	protected static void InvokeUserCode_RpcDisableRenderers(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDisableRenderers called on server.");
		}
		else
		{
			((EntityVisual)obj).UserCode_RpcDisableRenderers();
		}
	}

	protected void UserCode_RpcEnableRenderers()
	{
		if (!base.isServer)
		{
			EnableRenderersLocal();
		}
	}

	protected static void InvokeUserCode_RpcEnableRenderers(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcEnableRenderers called on server.");
		}
		else
		{
			((EntityVisual)obj).UserCode_RpcEnableRenderers();
		}
	}

	protected void UserCode_RpcHideGroundMarker()
	{
		if (!base.isServer)
		{
			HideGroundMarkerLocal();
		}
	}

	protected static void InvokeUserCode_RpcHideGroundMarker(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcHideGroundMarker called on server.");
		}
		else
		{
			((EntityVisual)obj).UserCode_RpcHideGroundMarker();
		}
	}

	protected void UserCode_RpcShowGroundMarker()
	{
		if (!base.isServer)
		{
			ShowGroundMarkerLocal();
		}
	}

	protected static void InvokeUserCode_RpcShowGroundMarker(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowGroundMarker called on server.");
		}
		else
		{
			((EntityVisual)obj).UserCode_RpcShowGroundMarker();
		}
	}

	protected void UserCode_RpcSkipSpawning()
	{
		if (spawnEffect != null && spawnEffect.activeSelf)
		{
			FxStop(spawnEffect);
			spawnEffect.SetActive(value: false);
			spawnEffect.SetActive(value: true);
		}
		if (spawnEffectOnGround != null && spawnEffectOnGround.activeSelf)
		{
			FxStop(spawnEffectOnGround);
			spawnEffectOnGround.SetActive(value: false);
			spawnEffectOnGround.SetActive(value: true);
		}
	}

	protected static void InvokeUserCode_RpcSkipSpawning(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSkipSpawning called on server.");
		}
		else
		{
			((EntityVisual)obj).UserCode_RpcSkipSpawning();
		}
	}

	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(skipSpawning);
			writer.WriteBool(invisibleByDefault);
			writer.WriteBool(_shouldShowStunned);
			writer.WriteInt(genericStackIndicatorMax);
			writer.WriteInt(genericStackIndicatorValue);
			return;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(skipSpawning);
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteBool(invisibleByDefault);
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteBool(_shouldShowStunned);
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteInt(genericStackIndicatorMax);
		}
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			writer.WriteInt(genericStackIndicatorValue);
		}
	}

	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			GeneratedSyncVarDeserialize(ref skipSpawning, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref invisibleByDefault, null, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref _shouldShowStunned, OnShouldShowStunnedChanged, reader.ReadBool());
			GeneratedSyncVarDeserialize(ref genericStackIndicatorMax, null, reader.ReadInt());
			GeneratedSyncVarDeserialize(ref genericStackIndicatorValue, null, reader.ReadInt());
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref skipSpawning, null, reader.ReadBool());
		}
		if ((num & 2L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref invisibleByDefault, null, reader.ReadBool());
		}
		if ((num & 4L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref _shouldShowStunned, OnShouldShowStunnedChanged, reader.ReadBool());
		}
		if ((num & 8L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref genericStackIndicatorMax, null, reader.ReadInt());
		}
		if ((num & 0x10L) != 0L)
		{
			GeneratedSyncVarDeserialize(ref genericStackIndicatorValue, null, reader.ReadInt());
		}
	}
}
