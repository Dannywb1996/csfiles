using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Se_OntologicalShield : StatusEffect
{
	public GameObject fxNegateDamage;

	private float _lastTextPopupTime;

	protected override void OnCreate()
	{
		base.OnCreate();
		if (!base.isServer)
		{
			return;
		}
		DoInvulnerable(delegate
		{
			if (!(Time.time - _lastTextPopupTime < 0.25f))
			{
				_lastTextPopupTime = Time.time;
				FxPlayNewNetworked(fxNegateDamage, base.victim);
				RpcShowPopupText();
			}
		});
	}

	[ClientRpc]
	private void RpcShowPopupText()
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		SendRPCInternal("System.Void Se_OntologicalShield::RpcShowPopupText()", -2040554841, writer, 0, includeOwner: true);
		NetworkWriterPool.Return(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcShowPopupText()
	{
		InGameUIManager.instance.ShowWorldPopMessage(new WorldMessageSetting
		{
			rawText = DewLocalization.GetUIValue("Se_OntologicalShield_Popup"),
			color = new Color(0.865f, 1f, 0.45f),
			worldPosGetter = () => (!(base.victim != null)) ? Vector3.zero : base.victim.Visual.GetCenterPosition()
		});
	}

	protected static void InvokeUserCode_RpcShowPopupText(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowPopupText called on server.");
		}
		else
		{
			((Se_OntologicalShield)obj).UserCode_RpcShowPopupText();
		}
	}

	static Se_OntologicalShield()
	{
		RemoteProcedureCalls.RegisterRpc(typeof(Se_OntologicalShield), "System.Void Se_OntologicalShield::RpcShowPopupText()", InvokeUserCode_RpcShowPopupText);
	}
}
