using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbySlotLayout
{
	private const float C_X_OFFSET = 500f;
	private const float C_START_Y = 262.5f;
	private const float C_Y_OFFSET = -175f;

	private readonly Transform _slotParent;
	private readonly SlotUI _slotPrefab;

	private readonly List<SlotUI> _slotUIs = new();

	public LobbySlotLayout(
		Transform slotParent,
		SlotUI slotPrefab
	)
	{
		_slotParent = slotParent;
		_slotPrefab = slotPrefab;
	}

	public void Build(NetworkList<SlotData> slots)
	{
		Clear();

		bool useTwoColumns = UseTwoColumns(slots);

		float leftY = C_START_Y;
		float rightY = C_START_Y;

		for (int i = 0; i < slots.Count; i++)
		{
			var slot = slots [ i ];

			var ui = Object.Instantiate(
				_slotPrefab,
				_slotParent
			);

			ui.Bind(slot);

			RectTransform rt = ui.GetComponent<RectTransform>();

			if (!useTwoColumns)
			{
				rt.anchoredPosition = new Vector2(
					0f,
					leftY
				);

				leftY += C_Y_OFFSET;
			}
			else
			{
				bool isRightColumn =
					slot.team == Team.TeamB;

				if (isRightColumn)
				{
					rt.anchoredPosition = new Vector2(
						C_X_OFFSET,
						rightY
					);

					rightY += C_Y_OFFSET;
				}
				else
				{
					rt.anchoredPosition = new Vector2(
						-C_X_OFFSET,
						leftY
					);

					leftY += C_Y_OFFSET;
				}
			}

			_slotUIs.Add(ui);
		}
	}

	public void Refresh(NetworkList<SlotData> slots)
	{
		for (int i = 0; i < _slotUIs.Count; i++)
		{
			if (_slotUIs [ i ] == null)
				continue;

			if (i >= slots.Count)
				continue;

			_slotUIs [ i ].Refresh(slots [ i ]);
		}
	}

	private void Clear()
	{
		foreach (var ui in _slotUIs)
		{
			if (ui != null)
			{
				Object.Destroy(ui.gameObject);
			}
		}

		_slotUIs.Clear();
	}

	private bool UseTwoColumns(NetworkList<SlotData> slots)
	{
		if (slots.Count > 4)
			return true;

		for (int i = 0; i < slots.Count; i++)
		{
			if (slots [ i ].team != Team.None)
				return true;
		}

		return false;
	}
}
