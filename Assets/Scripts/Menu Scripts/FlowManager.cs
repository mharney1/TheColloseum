using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class FlowManager : MonoBehaviour
{
	public static FlowManager S_INSTANCE;

	private void Awake()
	{
		if (S_INSTANCE == null)
		{
			S_INSTANCE = this;
			DontDestroyOnLoad( gameObject );
		}
		else
		{
			Destroy( gameObject );
		}
	}

	public void ToLobby()
	{
		SceneLoader.S_INSTANCE.LoadScene( "Lobby_Scene" );
	}

	public void ToCombat()
	{
		SceneLoader.S_INSTANCE.LoadScene( "Combat_Scene" );
	}

	public void ToMenu()
	{
		SceneLoader.S_INSTANCE.LoadScene( "Main_Menu" );
	}




}
