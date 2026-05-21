using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class CharacterUI : MonoBehaviour
{
	private Character _character;

	public CombatUI combat
	{
		get; private set;
	}
	public SliderUI sliders
	{
		get; private set;
	}

	private void Awake()
	{
		_character = GetComponent<Character>();
		combat = GetComponent<CombatUI>();
		sliders = GetComponent<SliderUI>();
	}
	private void Start()
	{
		if (_character == null)
			return;

		ulong localClientId = NetworkManager.Singleton.LocalClientId;

		bool isLocalPlayer =
			!_character.identity.GetAI() &&
			_character.OwnerClientId == localClientId;

		Camera cam = GetComponentInChildren<Camera>(true);
		AudioListener listener = GetComponentInChildren<AudioListener>(true);

		if (cam != null)
		{
			cam.gameObject.SetActive(isLocalPlayer);
			Debug.Assert(!cam.gameObject.activeSelf, $"Camera activated for character {_character.identity.GetCharacterID()} with the following information {gameObject.name} | Owner: {_character.OwnerClientId} | Local: {NetworkManager.Singleton.LocalClientId} | AI: {_character.identity.GetAI()}");
		}

		if (listener != null)
			listener.enabled = isLocalPlayer;
	}
}
