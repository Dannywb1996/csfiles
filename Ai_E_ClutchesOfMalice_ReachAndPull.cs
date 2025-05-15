using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Ai_E_ClutchesOfMalice_ReachAndPull : AbilityInstance
{
	[HideInInspector]
	public float endSyncTime;

	public DewBeamRenderer line;

	public float lineShootDuration;

	public DewEase pullEase;

	public float pullDuration;

	public GameObject fxStart;

	public GameObject fxHit;

	public ScalingValue firstDmgFactor;

	public ScalingValue secondDmgFactor;

	public float procCoefficient;

	private bool _sustainEnable;

	protected override IEnumerator OnCreateSequenced()
	{
		Vector3 startPos = base.transform.position;
		line.SetPoints(startPos, startPos);
		line.enabled = true;
		Quaternion quaternion = Quaternion.LookRotation(base.info.target.position - startPos);
		FxPlayNew(fxStart, startPos, quaternion * fxStart.transform.localRotation);
		StartCoroutine(Routine());
		yield return new SI.WaitForCondition(() => _sustainEnable);
		yield return new SI.WaitForSeconds(endSyncTime - Time.time);
		if (base.isServer && !base.info.target.IsNullInactiveDeadOrKnockedOut() && !base.info.target.Status.hasUnstoppable)
		{
			Vector3 end = Dew.GetPositionOnGround(startPos) + Random.insideUnitSphere * 1.25f;
			Vector3 validAgentDestination_Closest = Dew.GetValidAgentDestination_Closest(base.info.target.agentPosition, end);
			validAgentDestination_Closest = Dew.GetPositionOnGround(validAgentDestination_Closest);
			base.info.target.Control.StartDaze(pullDuration);
			base.info.target.Control.StartDisplacement(new DispByDestination
			{
				destination = validAgentDestination_Closest,
				canGoOverTerrain = true,
				duration = pullDuration,
				ease = pullEase,
				isCanceledByCC = false,
				isFriendly = false,
				onCancel = base.Destroy,
				rotateForward = false
			});
			yield return new SI.WaitForSeconds(pullDuration);
			FxPlayNewNetworked(fxHit, base.info.target);
			Damage(secondDmgFactor, procCoefficient).Dispatch(base.info.target);
			DestroyIfActive();
		}
		IEnumerator Routine()
		{
			Vector3 endPos = Vector3.zero;
			float elapsedTime = 0f;
			Vector3 targetPos = base.info.target.Visual.GetCenterPosition();
			Vector3 pos = base.transform.position;
			while (elapsedTime < lineShootDuration)
			{
				pos = Vector3.Lerp(pos, targetPos, elapsedTime / lineShootDuration);
				line.SetEndPoint(pos);
				endPos = targetPos;
				elapsedTime += Time.deltaTime;
				yield return null;
			}
			line.SetEndPoint(endPos);
			_sustainEnable = true;
			if (base.isServer)
			{
				Damage(firstDmgFactor, procCoefficient).Dispatch(base.info.target);
			}
		}
	}

	protected override void ActiveFrameUpdate()
	{
		base.ActiveFrameUpdate();
		if (!_sustainEnable || line == null)
		{
			return;
		}
		if (base.info.target.IsNullInactiveDeadOrKnockedOut() || base.info.target.Status.hasUnstoppable)
		{
			Vector3 pos = line.lineRenderer.GetPosition(1);
			DOTween.To(() => pos, delegate(Vector3 x)
			{
				pos = x;
			}, base.transform.position, 0.1f).OnComplete(delegate
			{
				if (base.isServer)
				{
					DestroyIfActive();
				}
			});
			line.SetEndPoint(pos);
		}
		else
		{
			line.lineRenderer.SetPosition(1, base.info.target.Visual.GetCenterPosition());
		}
	}

	protected override void OnDestroyActor()
	{
		base.OnDestroyActor();
		line.enabled = false;
	}

	private void MirrorProcessed()
	{
	}
}
