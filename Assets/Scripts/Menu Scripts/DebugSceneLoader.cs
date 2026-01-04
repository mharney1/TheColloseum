using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;

public class DebugSceneLoader : MonoBehaviour
{
	[SerializeField] private string _lobbySceneName = "Lobby";

	[ContextMenu( "Load Lobby Scene" )]
	public void LoadLobby()
	{
		SceneManager.LoadScene( _lobbySceneName );
	}
	[ContextMenu( "Force Finalize Lobby" )]
	private void DebugFinalize()
	{
		LobbyManager.S_INSTANCE.aiManager.AIFinal();
	}
}
[CustomEditor( typeof( DebugSceneLoader ) )]
public class DebugSceneLoaderEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		DebugSceneLoader loader = (DebugSceneLoader) target;

		GUILayout.Space( 10 );

		if (GUILayout.Button( "Load Lobby Scene" ))
		{
			loader.LoadLobby();
		}
	}
}
