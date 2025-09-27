using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private GameObject networkMenuPanel;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button clientBtn;

    private void Awake()
    {
        networkMenuPanel.SetActive(true);
        
        hostBtn.onClick.AddListener(() =>
        {
            if (networkMenuPanel != null)
            {
                networkMenuPanel.SetActive(false);
            }
            NetworkManager.Singleton.StartHost();
        });
        serverBtn.onClick.AddListener(() =>
        {
             if (networkMenuPanel != null)
            {
                networkMenuPanel.SetActive(false);
            }
            NetworkManager.Singleton.StartServer();
        });
        clientBtn.onClick.AddListener(() =>
        {
             if (networkMenuPanel != null)
            {
                networkMenuPanel.SetActive(false);
            }
            NetworkManager.Singleton.StartClient();
        });
    }
}
