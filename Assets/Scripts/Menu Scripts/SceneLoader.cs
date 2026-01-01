using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
	public static SceneLoader S_INSTANCE;

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

	public void LoadSceneAsync(string sceneName)
	{
		StartCoroutine( LoadRoutine( sceneName ) );
	}

	private IEnumerator LoadRoutine(string sceneName)
	{
		AsyncOperation op = SceneManager.LoadSceneAsync( sceneName );
		op.allowSceneActivation = false;

		while (op.progress < 0.9f)
		{
			yield return null;
		}

		op.allowSceneActivation = true;
	}
}
