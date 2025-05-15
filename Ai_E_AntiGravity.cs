using System;
using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Ai_E_AntiGravity : AbilityInstance
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Ad_CheckFloating
	{
	}

	public DewCollider range;

	public ScalingValue dmgFactor;

	public float procCoefficient;

	public DewEase ascendEase;

	public GameObject gravityEffect;

	public GameObject gravityEntEffect;

	public GameObject impactEffect;

	public GameObject descendEffect;

	public GameObject hitEffect;

	public float sustainTime;

	public float ascendTime;

	public float descendTime;

	public float dmgDelay;

	public float ascendHeight;

	public float bindPostDuration;

	private float _bindDuration;

	protected override IEnumerator OnCreateSequenced()
	{
		if (!base.isServer)
		{
			yield break;
		}
		_bindDuration = ascendTime + sustainTime + descendTime;
		range.transform.position = base.info.point;
		FxPlayNetworked(gravityEffect, base.info.point, Quaternion.identity);
		ArrayReturnHandle<Entity> handle;
		ReadOnlySpan<Entity> entities = range.GetEntities(out handle, tvDefaultHarmfulEffectTargets);
		for (int i = 0; i < entities.Length; i++)
		{
			Entity entity = entities[i];
			if (!entity.Status.hasCrowdControlImmunity && !entity.IsNullInactiveDeadOrKnockedOut())
			{
				CreateBasicEffect(entity, new StunEffect(), _bindDuration + bindPostDuration, "stun_antigravity");
				if (!entity.HasData<Ad_CheckFloating>())
				{
					entity.AddData(default(Ad_CheckFloating));
					FxPlayNewNetworked(gravityEntEffect, entity);
					RpcGravitySequence(entity);
				}
			}
		}
		handle.Return();
		yield return new SI.WaitForSeconds(_bindDuration + dmgDelay);
		FxPlayNetworked(impactEffect, base.info.point, Quaternion.identity);
		ArrayReturnHandle<Entity> handle2;
		ReadOnlySpan<Entity> entities2 = range.GetEntities(out handle2, tvDefaultHarmfulEffectTargets);
		for (int j = 0; j < entities2.Length; j++)
		{
			Entity entity2 = entities2[j];
			FxPlayNewNetworked(hitEffect, entity2);
			Damage(dmgFactor, procCoefficient).Dispatch(entity2);
			if (entity2.HasData<Ad_CheckFloating>())
			{
				entity2.RemoveData<Ad_CheckFloating>();
			}
		}
		handle2.Return();
		Destroy();
	}

	[ClientRpc]
	private void RpcGravitySequence(Entity e)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteNetworkBehaviour(e);
		SendRPCInternal("System.Void Ai_E_AntiGravity::RpcGravitySequence(Entity)", 975538599, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcGravitySequence__Entity(Entity e)
	{
		StartCoroutine(Routine());
		IEnumerator Routine()
		{
			EaseFunction easeFunc = EasingFunction.GetEasingFunction(ascendEase);
			EntityTransformModifier entT = e.Visual.GetNewTransformModifier();
			for (float t = 0f; t < ascendTime; t += Time.deltaTime)
			{
				if (e.IsNullInactiveDeadOrKnockedOut())
				{
					yield break;
				}
				float v = t / ascendTime;
				v = easeFunc(0f, 1f, v);
				entT.worldOffset = Vector3.up * (v * ascendHeight);
				yield return null;
			}
			entT.worldOffset = Vector3.up * ascendHeight;
			yield return new WaitForSeconds(sustainTime);
			FxPlayNew(descendEffect, e);
			for (float t = 0f; t < descendTime; t += Time.deltaTime)
			{
				if (e.IsNullInactiveDeadOrKnockedOut())
				{
					yield break;
				}
				float num = t / descendTime;
				entT.worldOffset = Vector3.up * ((1f - num) * ascendHeight);
				yield return null;
			}
			entT.worldOffset = Vector3.zero;
			entT.Stop();
			yield return null;
			e.Visual.FixTailsAndClothes();
		}
	}

	protected static void InvokeUserCode_RpcGravitySequence__Entity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcGravitySequence called on server.");
		}
		else
		{
			((Ai_E_AntiGravity)obj).UserCode_RpcGravitySequence__Entity(reader.ReadNetworkBehaviour<Entity>());
		}
	}

	static Ai_E_AntiGravity()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Ai_E_AntiGravity), "System.Void Ai_E_AntiGravity::RpcGravitySequence(Entity)", InvokeUserCode_RpcGravitySequence__Entity);
	}
}
