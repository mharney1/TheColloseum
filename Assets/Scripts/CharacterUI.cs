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
		if ( _character != null && _character.IsOwner)
		{
			Camera cam = GetComponentInChildren<Camera>( true );
			if (cam != null)
			{
				cam.gameObject.SetActive( true );
			}
		}
	}
}
