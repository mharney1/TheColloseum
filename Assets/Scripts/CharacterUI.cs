using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CharacterUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject preppingScreen; 
    [SerializeField] private Button attackButton;
    [SerializeField] private Button blockButton;
    [SerializeField] private Button counterButton;
    [SerializeField] private Button restButton;

    [SerializeField] private GameObject preppedScreen; 
    [SerializeField] private Button changeButton;
    [SerializeField] private TextMeshProUGUI currentChoiceText;

    private Character player;
    private bool isOwnerUI = false;

    private void Start()
    {
        StartCoroutine(WaitForPlayer());
    }

    private IEnumerator SubscribePhaseManager()
    {
        while (PhaseManager.Instance == null)
            yield return null;

        PhaseManager.Instance.CurrentPhase.OnValueChanged += HandlePhaseChanged;
    }
    private IEnumerator WaitForPlayer()
    {
        while (player == null)
        {
            player = GetComponentInParent<Character>();
            yield return null;
        }

        // Now player is guaranteed to exist
        if (!player.IsOwner)
        {
            preppingScreen?.SetActive(false);
            preppedScreen?.SetActive(false);
            yield break;
        }

        isOwnerUI = true;

        // Wire up buttons
        attackButton?.onClick.AddListener(() => OnActionSelected(Choices.Attack));
        blockButton?.onClick.AddListener(() => OnActionSelected(Choices.Block));
        counterButton?.onClick.AddListener(() => OnActionSelected(Choices.Counter));
        restButton?.onClick.AddListener(() => OnActionSelected(Choices.Rest));
        changeButton?.onClick.AddListener(ShowActionButtons);

        // Wait for PhaseManager to exist, then subscribe
        StartCoroutine(SubscribePhaseManager());

        HideUI();
    }
    private void OnDisable()
    {
        if (!isOwnerUI) return;

        if (PhaseManager.Instance != null)
            PhaseManager.Instance.CurrentPhase.OnValueChanged -= HandlePhaseChanged;
    }

    private void HandlePhaseChanged(Phase previous, Phase current)
    {
        if (!isOwnerUI) return;

        if (current == Phase.Prepare)
            ShowUI();
        else
            HideUI();
    }

    private void OnActionSelected(Choices choice)
    {
        preppingScreen?.SetActive(false);
        preppedScreen?.SetActive(true);
        if (currentChoiceText != null) currentChoiceText.text = choice.ToString();

        // Send to server via Character
        player.SetChoiceServerRpc(choice);
    }

    private void ShowActionButtons()
    {
        preppingScreen?.SetActive(true);
        preppedScreen?.SetActive(false);
    }

    public void ShowUI()
    {
        preppingScreen?.SetActive(true);
        preppedScreen?.SetActive(false);
        if (currentChoiceText != null) currentChoiceText.text = "";
    }

    public void HideUI()
    {
        preppingScreen?.SetActive(false);
        preppedScreen?.SetActive(false);
        if (currentChoiceText != null) currentChoiceText.text = "";
    }
}