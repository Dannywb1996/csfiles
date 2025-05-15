using System;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_MirageSkin_Delusion_Delusional : StackedStatusEffect
{
	private class Ad_HealPreventedText
	{
		public float lastShowTime;
	}

	public FxVolume delusionalVolume;

	public Formula duration;

	[NonSerialized]
	public bool isEternal;

	public GameObject fxStackUpdated;

	private OnScreenTimerHandle _timer;

	protected override void OnCreate()
	{
		base.OnCreate();
		InGameUIManager.instance.ShowWorldPopMessage(new WorldMessageSetting
		{
			rawText = DewLocalization.GetUIValue("Se_MirageSkin_Delusion_Delusional_Popup"),
			color = new Color(1f, 0.5f, 0.7f),
			worldPosGetter = () => (base.victim != null) ? base.victim.Visual.GetCenterPosition() : Vector3.zero
		});
		float num;
		if (AttrCustomizeResources.Config.enableHealthReduceMultiplierAddByZone)
		{
			num = (float)(90.0 + 10.0 * (1.0 - 1.0 / Math.Pow(2.0, Math.Min(100, NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex))));
		}
		else
		{
			num = base.stack;
		}
		if (base.victim.isOwned)
		{
			_timer = ShowOnScreenTimerLocally(new OnScreenTimerHandle
			{
				rawTextGetter = () => string.Format("{0} <b><alpha=999>({1}%)</b>", DewLocalization.GetUIValue("Se_MirageSkin_Delusion_Delusional_Name"), num),
				color = new Color(1f, 0.4f, 0.7f),
				fillAmountGetter = () => base.normalizedDuration ?? 1f
			});
		}
		if (base.isServer)
		{
			base.victim.takenHealProcessor.Add(VictimOntakenHealProcessor, 10);
			UpdateDuration();
		}
	}

	protected override void OnStackChange(int oldStack, int newStack)
	{
		base.OnStackChange(oldStack, newStack);
		if (base.isServer)
		{
			UpdateDuration();
		}
	}

	private void UpdateDuration()
	{
		if (!isEternal)
		{
			SetTimer(duration.Evaluate(base.stack));
		}
	}

	protected override void ActiveLogicUpdate(float dt)
	{
		base.ActiveLogicUpdate(dt);
		float target = (float)base.stack / (float)maxStack * 0.5f;
		delusionalVolume.targetStrength = Mathf.MoveTowards(delusionalVolume.targetStrength, target, 0.3f * Time.deltaTime);
		delusionalVolume.UpdateVolume();
		if (base.isServer && base.victim.IsNullInactiveDeadOrKnockedOut())
		{
			DestroyIfActive();
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		if (_timer != null)
		{
			HideOnScreenTimerLocally(_timer);
			_timer = null;
		}
		if (base.isServer && base.victim != null)
		{
			base.victim.takenHealProcessor.Remove(VictimOntakenHealProcessor);
		}
	}

	private void VictimOntakenHealProcessor(ref HealData data, Actor actor, Entity target)
	{
		float num = ((!AttrCustomizeResources.Config.enableHealthReduceMultiplierAddByZone) ? ((float)base.stack) : ((float)(90.0 + 10.0 * (1.0 - 1.0 / Math.Pow(2.0, Math.Min(100, NetworkedManagerBase<ZoneManager>.instance.currentZoneIndex))))));
		data.ApplyReduction(num / 100f);
		if (!base.victim.TryGetData<Ad_HealPreventedText>(out var data2))
		{
			data2 = new Ad_HealPreventedText();
			base.victim.AddData(data2);
		}
		if (!(Time.time - data2.lastShowTime < 0.35f))
		{
			data2.lastShowTime = Time.time;
			RpcShowHealPrevented();
		}
	}

	[ClientRpc]
	private void RpcShowHealPrevented()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void Se_MirageSkin_Delusion_Delusional::RpcShowHealPrevented()", 1645532168, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcShowHealPrevented()
	{
		if (!base.victim.IsNullInactiveDeadOrKnockedOut())
		{
			InGameUIManager.instance.ShowWorldPopMessage(new WorldMessageSetting
			{
				rawText = DewLocalization.GetUIValue("Se_MirageSkin_Delusion_Delusional_HealReduced"),
				color = new Color(0.85f, 0.3f, 0.2f),
				worldPosGetter = () => (base.victim != null) ? base.victim.Visual.GetCenterPosition() : Vector3.zero
			});
		}
	}

	protected static void InvokeUserCode_RpcShowHealPrevented(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowHealPrevented called on server.");
		}
		else
		{
			((Se_MirageSkin_Delusion_Delusional)obj).UserCode_RpcShowHealPrevented();
		}
	}

	static Se_MirageSkin_Delusion_Delusional()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Se_MirageSkin_Delusion_Delusional), "System.Void Se_MirageSkin_Delusion_Delusional::RpcShowHealPrevented()", InvokeUserCode_RpcShowHealPrevented);
	}
}
