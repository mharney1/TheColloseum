using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
	public static SceneLoader S_INSTANCE;

	[SerializeField] private string _loadingSceneName = "Loading_Scene";

	private string _targetScene;

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

	public void LoadScene(string sceneName)
	{
		_targetScene = sceneName;
		SceneManager.LoadScene( _loadingSceneName );
	}

	public void BeginLoadingTarget( Slider progressBar )
	{
		StartCoroutine( LoadRoutine( progressBar ) );
	}

	private IEnumerator LoadRoutine( Slider progressBar )
	{
		float displayedValue = 0;
		AsyncOperation op = SceneManager.LoadSceneAsync( _targetScene );
		op.allowSceneActivation = false;

		while (op.progress < 0.9f)
		{
			displayedValue = Mathf.Lerp( displayedValue, op.progress, Time.deltaTime * 5f );
			progressBar.value = displayedValue;
			yield return null;
		}

		op.allowSceneActivation = true;
	}
}
