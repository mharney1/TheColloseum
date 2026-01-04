using UnityEngine;
using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour
{
	[SerializeField] private Slider _progressBar;
	private void Start()
	{
		SceneLoader.S_INSTANCE.BeginLoadingTarget( _progressBar );
	}
}
