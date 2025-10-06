using UnityEngine;
using TMPro;

public class EndScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject endScreen;           // The parent GameObject to enable/disable
    [SerializeField] private TextMeshProUGUI matchResultText; // TMP text for the message

    /// <summary>
    /// Show the end screen with a specific message
    /// </summary>
    public void ShowEndScreen(string message)
    {
        Debug.Log("Showing End Screen");
        if (endScreen != null)
        {
            endScreen.SetActive(true);
            if (matchResultText != null)
                matchResultText.text = message;
        }
    }

    /// <summary>
    /// Hide the end screen
    /// </summary>
    public void HideEndScreen()
    {
        if (endScreen != null)
            endScreen.SetActive(false);
    }
}