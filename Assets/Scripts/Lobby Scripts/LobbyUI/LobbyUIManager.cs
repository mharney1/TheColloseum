using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
	[Header("Slot Info")]
	[SerializeField] private Transform _slotParent;
	[SerializeField] private SlotUI _slotPrefab;

	[Header("Header Info")]
	[SerializeField] private TextMeshProUGUI _matchTypeText;
	[SerializeField] private TextMeshProUGUI _gameModeText;
	[SerializeField] private TextMeshProUGUI _lobbyStatusText;
	[SerializeField] private TextMeshProUGUI _timerText;

	[Header("Buttons")]
	[SerializeField] private Button _readyButton;
	[SerializeField] private Button _leaveButton;
	[SerializeField] private Button _startButton;

	private LobbyManager _lobby;
	private bool _isShuttingDown;

	private LobbySlotLayout _slotLayout;
	private LobbyHeaderModifier _headerModifier;
	private LobbyButtonBinder _buttonBinder;

	private void Start()
	{
		StartCoroutine(InitializeUI());
	}

	private IEnumerator InitializeUI()
	{
		while (LobbyManager.S_INSTANCE == null)
			yield return null;

		_lobby = LobbyManager.S_INSTANCE;

		while (!_lobby.IsSpawned)
			yield return null;

		var slots = _lobby.SlotManager.GetSlots();

		while (slots.Count == 0)
			yield return null;

		_slotLayout = new LobbySlotLayout( _slotParent, _slotPrefab );

		_headerModifier = new LobbyHeaderModifier( _matchTypeText, _gameModeText, _lobbyStatusText, _timerText );

		_buttonBinder = new LobbyButtonBinder( _readyButton, _leaveButton, _startButton );

		BindToSlots();

		_slotLayout.Build(slots);

		_headerModifier.SetHeader(_lobby);

		_buttonBinder.Bind(_lobby);

		Refresh();
	}

	private void Update()
	{
		if (IsInvalid())
			return;

		_headerModifier.UpdateHeader(_lobby);
	}

	private void OnDisable()
	{
		_isShuttingDown = true;

		StopAllCoroutines();

		if (_lobby != null)
		{
			UnbindFromSlots();
		}

		_buttonBinder?.Unbind();
	}

	private void BindToSlots()
	{
		var slots = _lobby.SlotManager.GetSlots();

		slots.OnListChanged += OnSlotsChanged;
	}

	private void UnbindFromSlots()
	{
		var slots = _lobby.SlotManager.GetSlots();

		slots.OnListChanged -= OnSlotsChanged;
	}

	private void OnSlotsChanged(NetworkListEvent<SlotData> changeEvent)
	{
		if (IsInvalid())
			return;

		Refresh();
	}

	public void Refresh()
	{
		if (IsInvalid())
			return;

		var slots = _lobby.SlotManager.GetSlots();

		if (slots.Count == 0)
			return;

		_slotLayout.Refresh(slots);
	}

	private bool IsInvalid()
	{
		return _isShuttingDown || _lobby == null || !_lobby.IsSpawned;
	}
}
