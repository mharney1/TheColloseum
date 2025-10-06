using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatUI : MonoBehaviour
{
    [SerializeField] private GameObject preppingScreen; 
    [SerializeField] private Button attackButton;
    [SerializeField] private Button blockButton;
    [SerializeField] private Button counterButton;
    [SerializeField] private Button restButton;

    [SerializeField] private GameObject preppedScreen; 
    [SerializeField] private Button changeButton;
    [SerializeField] private TextMeshProUGUI currentChoiceText;

    private Character player;

    public void Setup(Character newPlayer)
    {
        player = newPlayer;
        attackButton?.onClick.AddListener(() => OnActionSelected(Choices.Attack));
        blockButton?.onClick.AddListener(() => OnActionSelected(Choices.Block));
        counterButton?.onClick.AddListener(() => OnActionSelected(Choices.Counter));
        restButton?.onClick.AddListener(() => OnActionSelected(Choices.Rest));
        changeButton?.onClick.AddListener(() => ShowActionButtons());
    }
    public void ShowUI ()
    {
        if (!player.isDizzy() && player.GetPair() != -1) ShowActionButtons();
        else
        {
            if (player.GetPair() == -1) currentChoiceText.text = "Waiting for an Opponent";
            if (player.isDizzy()) currentChoiceText.text = "Dizzy";
            ShowDisabled();
        }
    }
    private void OnActionSelected(Choices choice)
    {
        preppingScreen?.SetActive(false);
        preppedScreen?.SetActive(true);
        currentChoiceText.text = choice.ToString();
        player.SetChoiceServerRpc(choice);
    }
    private void ShowActionButtons()
    {
        preppingScreen?.SetActive(true);
        preppedScreen?.SetActive(false);
        currentChoiceText.text = "";
    }
    private void ShowDisabled()
    {
        preppedScreen?.SetActive(true);
        changeButton.interactable = false;
    }
    public void HideUI()
    {
        preppingScreen?.SetActive(false);
        preppedScreen?.SetActive(false);
        changeButton.interactable = true;
        currentChoiceText.text = "";
    }
}