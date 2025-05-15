using UnityEngine;

[DewResourceLink(ResourceLinkBy.Guid)]
[CreateAssetMenu(fileName = "New Dew Animation Clip", menuName = "Dew Animation Clip")]
public class DewAnimationClip : ScriptableObject, ILinkedByGuid
{
	public enum OverrideWalkBehavior
	{
		None,
		Full,
		UpperBody
	}

	public DewAnimationEntry[] entries;

	public bool onlyUpperBody;

	public OverrideWalkBehavior overrideWalk;

	public float overrideWalkNormalizedDuration = 1f;

	public bool hideWeaponOnHeroes = true;

	[field: HideInInspector]
	[field: SerializeField]
	public string resourceId { get; set; }

	private void OnValidate()
	{
		for (int i = 0; i < entries.Length; i++)
		{
			if (entries[i] != null)
			{
				entries[i].Validate();
			}
		}
	}

	private bool IsValid()
	{
		if (entries == null || entries.Length == 0)
		{
			Debug.LogWarning("DewAnimationClip (" + base.name + ") has no entries configured.");
			return false;
		}
		return true;
	}

	public int GetEntryIndex()
	{
		if (!IsValid())
		{
			return -1;
		}
		return Random.Range(0, entries.Length);
	}

	public static int GetEntryIndex(float clipSelectValue, int entriesLength)
	{
		return (int)(clipSelectValue * (float)entriesLength);
	}

	public int GetEntryIndex(float clipSelectValue)
	{
		if (!IsValid())
		{
			return -1;
		}
		return GetEntryIndex(clipSelectValue, entries.Length);
	}

	public DewAnimationEntry GetEntry()
	{
		if (!IsValid())
		{
			return null;
		}
		return entries[Random.Range(0, entries.Length)];
	}

	public DewAnimationEntry GetEntry(int index)
	{
		if (!IsValid())
		{
			return null;
		}
		return entries[index];
	}

	public DewAnimationEntry GetEntry(float value)
	{
		if (!IsValid())
		{
			return null;
		}
		return entries[(int)(value * (float)entries.Length)];
	}
}
