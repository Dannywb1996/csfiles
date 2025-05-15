using System;
using System.Collections.Generic;
using UnityEngine;

public class Shrine_Stardust : Shrine, IShrineCustomAction, IInteractable
{
	[NonSerialized]
	public int amount = 1;

	public Transform shakeTransform;

	private Vector3 _originalPos;

	private float _nextShakeTime;

	private float _nextPosChangeTime;

	public override bool isRegularReward => true;

	int IInteractable.priority => -1000;

	float IInteractable.focusDistance => 4.25f;

	public override void OnStartClient()
	{
		base.OnStartClient();
		_originalPos = shakeTransform.localPosition;
		shakeTransform.localRotation = Quaternion.Euler(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360));
		float scale = UnityEngine.Random.Range(0.8f, 1.2f);
		Vector3 offset = Vector3.up * UnityEngine.Random.Range(-0.5f, 0.5f);
		availableEffect.transform.localScale *= scale;
		useEffect.transform.localScale *= scale;
		availableEffect.transform.localPosition += offset;
		useEffect.transform.localPosition += offset;
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (Time.time > _nextShakeTime)
		{
			_nextShakeTime = Time.time + 1f / 60f;
			shakeTransform.localPosition = _originalPos + UnityEngine.Random.onUnitSphere * 0.02f;
		}
	}

	protected override bool OnUse(Entity entity)
	{
		foreach (DewPlayer humanPlayer in DewPlayer.humanPlayers)
		{
			humanPlayer.GiveStardust(amount);
		}
		Destroy();
		return true;
	}

	public string GetRawAction()
	{
		return DewLocalization.GetUIValue("InGame_Interact_GetStardust");
	}

	public override void OnSaveActor(Dictionary<string, object> data)
	{
		base.OnSaveActor(data);
		data["amount"] = amount;
	}

	public override void OnLoadActor(Dictionary<string, object> data)
	{
		base.OnLoadActor(data);
		amount = (int)data["amount"];
	}

	private void MirrorProcessed()
	{
	}
}
