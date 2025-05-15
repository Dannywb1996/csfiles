using System;
using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_E_WinterDive : AbilityInstance
{
	public DewCollider range;

	public KnockUpStrength knockUpStrength;

	public ScalingValue dmgFactor;

	public float castingDuration;

	public float ascendTime;

	public float ascendHeight;

	public float descendTime;

	public float postDelay;

	public float stunDuration;

	public DewAnimationClip landingAnim;

	public GameObject ascendEffect;

	public GameObject descendEffect;

	public GameObject landingEffect;

	public GameObject hitEffect;

	private EntityTransformModifier _entTransform;

	protected override IEnumerator OnCreateSequenced()
	{
		_entTransform = base.info.caster.Visual.GetNewTransformModifier();
		if (base.isServer)
		{
			base.info.caster.Control.StartDaze(castingDuration + ascendTime + descendTime);
			FxPlayNetworked(ascendEffect, base.info.caster);
			RpcAscend();
			CreateBasicEffect(base.info.caster, new UncollidableEffect(), castingDuration + descendTime + postDelay);
			CreateBasicEffect(base.info.caster, new InvulnerableEffect(), castingDuration + descendTime + postDelay);
			CreateBasicEffect(base.info.caster, new InvisibleEffect(), castingDuration + descendTime + postDelay);
			yield return new SI.WaitForSeconds(castingDuration / 2f);
			FxStopNetworked(ascendEffect);
			Vector3 dest = Dew.GetValidAgentDestination_Closest(base.info.caster.agentPosition, base.info.point);
			base.info.caster.Control.Teleport(dest);
			base.info.caster.Animation.PlayAbilityAnimation(landingAnim);
			range.transform.position = dest;
			yield return new SI.WaitForSeconds(castingDuration / 2f);
			FxPlayNetworked(descendEffect, base.info.caster);
			RpcDescend();
			base.info.caster.Control.StartDaze(postDelay);
			yield return new SI.WaitForSeconds(descendTime);
			FxPlayNetworked(landingEffect, dest, Quaternion.identity);
			ArrayReturnHandle<Entity> handle;
			ReadOnlySpan<Entity> entities = range.GetEntities(out handle, tvDefaultHarmfulEffectTargets);
			for (int i = 0; i < entities.Length; i++)
			{
				Entity entity = entities[i];
				FxPlayNewNetworked(hitEffect, entity);
				entity.Visual.KnockUp(knockUpStrength, isFriendly: false);
				Damage(dmgFactor).SetElemental(ElementalType.Cold).Dispatch(entity);
				CreateBasicEffect(entity, new StunEffect(), stunDuration, "winterdive_stun");
			}
			handle.Return();
			FxStopNetworked(descendEffect);
			Destroy();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_entTransform != null)
		{
			_entTransform.Stop();
		}
	}

	[ClientRpc]
	private void RpcAscend()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void Ai_E_WinterDive::RpcAscend()", 1646478346, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	[ClientRpc]
	private void RpcDescend()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void Ai_E_WinterDive::RpcDescend()", 994226764, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcAscend()
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			for (float t = 0f; t < ascendTime; t += Time.deltaTime)
			{
				float num = t / ascendTime;
				_entTransform.worldOffset = Vector3.up * (num * ascendHeight);
				_entTransform.scaleMultiplier = Vector3.one * (1f - num);
				yield return null;
			}
			_entTransform.worldOffset = Vector3.up * ascendHeight;
			_entTransform.scaleMultiplier = Vector3.zero;
			base.info.caster.Visual.DisableRenderersLocal();
		}
	}

	protected static void InvokeUserCode_RpcAscend(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAscend called on server.");
		}
		else
		{
			((Ai_E_WinterDive)obj).UserCode_RpcAscend();
		}
	}

	protected void UserCode_RpcDescend()
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			base.info.caster.Visual.EnableRenderersLocal();
			for (float t = 0f; t < descendTime; t += Time.deltaTime)
			{
				float num = t / descendTime;
				_entTransform.worldOffset = Vector3.up * ((1f - num) * ascendHeight);
				_entTransform.scaleMultiplier = Vector3.one * num;
				yield return null;
			}
			_entTransform.worldOffset = Vector3.zero;
			_entTransform.scaleMultiplier = Vector3.one;
			yield return null;
			base.info.caster.Visual.FixTailsAndClothes();
		}
	}

	protected static void InvokeUserCode_RpcDescend(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDescend called on server.");
		}
		else
		{
			((Ai_E_WinterDive)obj).UserCode_RpcDescend();
		}
	}

	static Ai_E_WinterDive()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_E_WinterDive), "System.Void Ai_E_WinterDive::RpcAscend()", InvokeUserCode_RpcAscend);
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_E_WinterDive), "System.Void Ai_E_WinterDive::RpcDescend()", InvokeUserCode_RpcDescend);
	}
}
