using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
	[SerializeField] private GameObject _networkMenuPanel;
	[SerializeField] private Button _hostBtn;
	[SerializeField] private Button _serverBtn;
	[SerializeField] private Button _clientBtn;

	private void Awake()
	{
		_networkMenuPanel.SetActive( true );

		_hostBtn.onClick.AddListener( () =>
		{
			if (_networkMenuPanel != null)
			{
				_networkMenuPanel.SetActive( false );
			}
			NetworkManager.Singleton.StartHost();
		} );
		_serverBtn.onClick.AddListener( () =>
		{
			if (_networkMenuPanel != null)
			{
				_networkMenuPanel.SetActive( false );
			}
			NetworkManager.Singleton.StartServer();
		} );
		_clientBtn.onClick.AddListener( () =>
		{
			if (_networkMenuPanel != null)
			{
				_networkMenuPanel.SetActive( false );
			}
			NetworkManager.Singleton.StartClient();
		} );
	}
}
