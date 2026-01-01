using System.Linq;
using TMPro;
using UnityEngine;

public class EndScreenUI : MonoBehaviour
{
	private Character _character;

	[SerializeField] private GameObject _endScreen;           // The parent GameObject to enable/disable
	[SerializeField] private TextMeshProUGUI _matchResultText; // TMP text for the message

	private void Awake()
	{
		_character = GetComponentInParent<Character>();
	}
	private void Start()
	{
		if (!_character.IsOwner)
		{
			this.enabled = false;
		}
		else
		{
			WinManager.GameEnd += GenerateMessage;
		}				
	}
	private void OnDisable()
	{
		if (_character.IsOwner)
		{
			WinManager.GameEnd -= GenerateMessage;
		}
	}
	private void GenerateMessage(EndData endData)
	{
		if (_endScreen == null )
		{
			return;
		}
		string message;
		if (endData.TeamBased)
		{
			if (endData.WinningTeams.Contains( _character.identity.team ))
			{
				if (endData.Tie)
				{
					message = "Your team has come to a draw.";
				}
				else
				{
					message = "Your team has defeated the enemy.";
				}
			}
			else
			{
				message = "Your team has been defeated.";
			}
		}
		else
		{
			if (endData.WinningPlayerIds.Contains( _character.OwnerClientId ))
			{
				if (endData.Tie)
				{
					message = "You have come to a draw.";
				}
				else
				{
					message = "You have defeated the enemy.";
				}
			}
			else
			{
				message = "You have been defeated.";
			}
		}
		_endScreen.SetActive( true );
		_matchResultText.text = message;
	}
}
