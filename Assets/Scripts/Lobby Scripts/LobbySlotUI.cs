using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class LobbySlotUI : MonoBehaviour
{
    [SerializeField] private Image _background;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private GameObject _hostTag;
    [SerializeField] private Image _readyCheck;

    private LobbySlot _slot;

    public void Bind( LobbySlot slot )
    {
        _slot = slot;
		Refresh();
    }

    public void Refresh()
    {
        if ( _slot == null )
            return;

		_nameText.text = ResolveName();
		_hostTag.SetActive( _slot.isHost);
		if (_slot.isReady)
		{
			_readyCheck.color = Color.green;
		}
		else
		{
			_readyCheck.color = Color.white;
		}
			_background.color = ResolveColor();
	}

    private string ResolveName()
    {
        return _slot.status switch
        {
            SlotStatus.Human => "Player",
            SlotStatus.AI => "AI",
            _ => "Open"
        };
    }
    private Color ResolveColor()
    {
        if ( _slot.team == Team.TeamA )
            return Color.blue;

        if ( _slot.team == Team.TeamB )
            return Color.red;

        return Color.gray;
    }
}
