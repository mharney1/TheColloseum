using System.Collections;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class EndScreenUI : MonoBehaviour
{
	private Character _character;

	[SerializeField] private GameObject _endScreen;
	[SerializeField] private TextMeshProUGUI _resultText;
	[SerializeField] private Button _exitButton;

	private void Awake()
	{
		_character = GetComponentInParent<Character>();

		_exitButton.onClick.AddListener(() => StartCoroutine(ExitRoutine()));
	}
	private void Start()
	{
		if (!_character.IsOwner)
		{
			this.enabled = false;
		}
		else if (_character.identity.GetAI())
		{
			this.enabled = false;
		}
		else
		{
			CombatDirector.GameEnded += ShowEndScreen;
		}				
	}
	private void OnDisable()
	{
		if (_character.IsOwner)
		{
			CombatDirector.GameEnded -= ShowEndScreen;
		}
	}
	private void ShowEndScreen(EndData endData)
	{
		if (_endScreen == null)
		{
			return;
		}
		string message;
		if (endData.TeamBased)
		{
			if (endData.WinningTeams.Contains(_character.identity.GetTeam()))
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
			if (endData.WinningPlayerIds.Contains(_character.identity.GetCharacterID()))
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
		_resultText.text = message;
		_endScreen.SetActive(true);
	}

	private IEnumerator ExitRoutine()
	{

		if (NetworkBootstrap.S_INSTANCE != null)
		{
			yield return NetworkBootstrap.S_INSTANCE.LeaveGame().AsIEnumerator();
		}

		SceneLoader.S_INSTANCE.LoadScene("Main_Menu");
	}
}
