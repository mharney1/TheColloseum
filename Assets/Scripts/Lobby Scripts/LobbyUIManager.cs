using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
	[Header( "Slot Info" )]
	[SerializeField] private Transform _slotParent;
	[SerializeField] private LobbySlotUI _slotPrefab;

	[Header( "Header Info" )]
	[SerializeField] private TextMeshProUGUI _matchTypeText;
	[SerializeField] private TextMeshProUGUI _gameModeText;
	[SerializeField] private TextMeshProUGUI _lobbyStatusText;
	[SerializeField] private TextMeshProUGUI _timerText;

	[Header( "Buttons" )]
	[SerializeField] private Button _readyButton;
	[SerializeField] private Button _leaveButton;
	[SerializeField] private Button _startButton;

	private float _xOffset = 500f;
	private float _startY = 262.5f;
	private float _yOffset = -175f;

	private readonly List<LobbySlotUI> _slotUIs = new();

	private void Start()
	{
		SetHeader();
		BuildSlots();
		Refresh();
		BindButtons();
	}

	private void Update()
	{
		if (LobbyManager.S_INSTANCE == null)
		{
			return;
		}
		UpdateHeader();
		Refresh();
	}

	public void BuildSlots()
	{
		ClearSlots();

		var slots = LobbyManager.S_INSTANCE.slots;
		var orderedSlots = GetDisplayOrder( slots ); // re-use helper

		float x = -_xOffset;
		float y = _startY;
		int i = 0;
		bool isFirstColumn = true;

		foreach (var slot in orderedSlots)
		{
			var ui = Instantiate( _slotPrefab, _slotParent );
			ui.Bind( slot );
			bool needsSecondColumn = (slot.team == Team.TeamB) || (i >= 4);
			RectTransform rt = ui.GetComponent<RectTransform>();

			if (isFirstColumn && needsSecondColumn)
			{
				isFirstColumn = false;
				x = _xOffset;
				y = _startY;
			}

			rt.anchoredPosition = new Vector2( x, y );
			y += _yOffset;
			i++;
			_slotUIs.Add( ui );
		}
	}

	public void Refresh()
	{
		for (int i = 0; i < _slotUIs.Count; i++)
		{
			_slotUIs [ i ].Refresh();
		}
	}

	public void Shutdown()
	{
		enabled = false;

		UnbindButtons();
		ClearSlots();
	}

	private void ClearSlots()
	{
		foreach (var ui in _slotUIs)
		{
			Destroy( ui.gameObject );
		}

		_slotUIs.Clear();
	}

	private IEnumerable<LobbySlot> GetDisplayOrder(IReadOnlyList<LobbySlot> slots)
	{
		bool hasTeams = false;

		for (int i = 0; i < slots.Count; i++)
		{
			if (slots [ i ].team != Team.None)
			{
				hasTeams = true;
				break;
			}
		}

		if (hasTeams)
		{
			for (int i = 0; i < slots.Count; i++)
				if (slots [ i ].team == Team.TeamA)
					yield return slots [ i ];

			for (int i = 0; i < slots.Count; i++)
				if (slots [ i ].team == Team.TeamB)
					yield return slots [ i ];
		}
		else
		{
			for (int i = 0; i < slots.Count; i++)
				yield return slots [ i ];
		}
	}

	private void SetHeader()
	{
		var session = GameSession.S_INSTANCE;
		if (session == null)
			return;

		_matchTypeText.text = session.matchType.ToString();
		_gameModeText.text = session.gameMode.ToString();
		_lobbyStatusText.text = LobbyManager.S_INSTANCE.stateManager.currentState.ToString();
		_timerText.text = FormatTimer( LobbyManager.S_INSTANCE.timeManager.TimeRemaining );
	}

	private void UpdateHeader()
	{
		var session = GameSession.S_INSTANCE;
		if (session == null)
			return;

		_lobbyStatusText.text = LobbyManager.S_INSTANCE.stateManager.currentState.ToString();
		_timerText.text = FormatTimer( LobbyManager.S_INSTANCE.timeManager.TimeRemaining );
	}

	private string FormatTimer(float seconds)
	{
		int secs = Mathf.FloorToInt( seconds );
		return $"{secs:00}";
	}

	private void BindButtons()
	{
		_readyButton.onClick.AddListener( () =>
		{
			LobbyManager.S_INSTANCE.ToggleLocalPlayerReady();
			Refresh();
		} );

		_leaveButton.onClick.AddListener( () =>
		{
			Shutdown();
			LobbyManager.S_INSTANCE.Shutdown();
			GameSession.S_INSTANCE.gameMode = GameMode.None;
			FlowManager.S_INSTANCE.ToMenu();
		} );
		// _startButton disabled for now
	}

	private void UnbindButtons()
	{
		_readyButton.onClick.RemoveAllListeners();
		_leaveButton.onClick.RemoveAllListeners();
		_startButton.onClick.RemoveAllListeners();
	}
}
