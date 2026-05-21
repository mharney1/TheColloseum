using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class SlotUI: MonoBehaviour
{
    [SerializeField] private Image _background;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private GameObject _hostTag;
	[SerializeField] private Image _readyCheck;
	[SerializeField] private Image _checkBackground;


	public void Bind(SlotData slot)
	{
		Refresh(slot);
	}

	public void Refresh( SlotData slot)
	{
		_nameText.text = ResolveName(slot);
		_hostTag.SetActive(slot.isHost);
		//_readyCheck.gameObject.SetActive( slot.status == SlotStatus.Human );
		_checkBackground.gameObject.SetActive( slot.status == SlotStatus.Human);
		_readyCheck.color = slot.isReady ? Color.green : Color.white;
		_background.color = ResolveColor(slot);
	}

	private string ResolveName(SlotData slot)
    {
		return slot.status switch
		{
			SlotStatus.Human => $"Player {slot.clientId}",
			SlotStatus.AI => "AI",
			_ => "Open"
		};
	}
    private Color ResolveColor(SlotData slot)
    {
		if (slot.team == Team.TeamA)
			return Color.blue;

		if (slot.team == Team.TeamB)
			return Color.red;

		return Color.gray;
	}
}
