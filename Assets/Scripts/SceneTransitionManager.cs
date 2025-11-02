using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayableDirector timeline; 
    [SerializeField] private CanvasGroup loadingScreen;
    [SerializeField] private string nextSceneName;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1f;     

    private void Start()
    {
        if (timeline != null)
            timeline.stopped += OnTimelineFinished;

        if (loadingScreen != null)
            loadingScreen.alpha = 0f; 
    }

    private void OnTimelineFinished(PlayableDirector director)
    {
        StartCoroutine(TransitionToNextScene());
    }

    private IEnumerator TransitionToNextScene()
    {
        if (loadingScreen != null)
        {
            float t = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                loadingScreen.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
                yield return null;
            }
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }


        yield return new WaitForSeconds(0.5f);

        asyncLoad.allowSceneActivation = true;
    }
}
