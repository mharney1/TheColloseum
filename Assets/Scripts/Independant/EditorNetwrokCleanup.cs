#if UNITY_EDITOR
using UnityEditor;
using Unity.Netcode;

[InitializeOnLoad]
public static class EditorNetworkCleanup
{
	static EditorNetworkCleanup()
	{
		EditorApplication.playModeStateChanged += OnPlayModeChanged;
	}

	private static void OnPlayModeChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.ExitingPlayMode)
		{
			if (NetworkManager.Singleton != null &&
				NetworkManager.Singleton.IsListening)
			{
				NetworkManager.Singleton.Shutdown();
			}
		}
	}
}
#endif
