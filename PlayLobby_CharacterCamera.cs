using UnityEngine;

public class PlayLobby_CharacterCamera : MonoBehaviour
{
	public PlayLobby_Character basisCharacter;

	private Vector3 _localPos;

	private Quaternion _localRot;

	private PlayLobby_Character[] _characters;

	private void Awake()
	{
		_characters = Object.FindObjectsOfType<PlayLobby_Character>(includeInactive: true);
		Transform basis = basisCharacter.transform;
		_localPos = basis.InverseTransformPoint(base.transform.position);
		_localRot = Quaternion.Inverse(basis.rotation) * base.transform.rotation;
	}

	private void OnEnable()
	{
		UpdatePosition();
	}

	private void LateUpdate()
	{
		UpdatePosition();
	}

	private void UpdatePosition()
	{
		PlayLobby_Character character = GetLocalPlayerCharacter();
		if (!(character == null))
		{
			Transform basis = character.transform;
			base.transform.SetPositionAndRotation(basis.TransformPoint(_localPos), basis.rotation * _localRot);
		}
	}

	private PlayLobby_Character GetLocalPlayerCharacter()
	{
		if (DewPlayer.local == null)
		{
			return null;
		}
		PlayLobby_Character[] characters = _characters;
		foreach (PlayLobby_Character c in characters)
		{
			if (c.playerIndex >= 0 && c.playerIndex < DewPlayer.humanPlayers.Count && DewPlayer.humanPlayers[c.playerIndex] == DewPlayer.local)
			{
				return c;
			}
		}
		return null;
	}
}
