using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;
    public static bool IsLoadingNewScene = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadNextLevelWithTransition(SceneFader.FadeType fadeType)
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        StartCoroutine(TransitionAndLoad(nextSceneIndex, fadeType));
    }

    public void LoadSceneWithTransition(string sceneName, SceneFader.FadeType fadeType)
    {
        StartCoroutine(TransitionAndLoad(sceneName, fadeType));
    }

    private IEnumerator TransitionAndLoad(object sceneIdentifier, SceneFader.FadeType fadeType)
    {
        if (SceneFader.Instance == null)
        {
            Debug.LogError("SceneFader not found. Loading scene instantly without transition.");
            LoadSceneInstantly(sceneIdentifier);
            yield break;
        }

        // fade out
        SceneFader.Instance.ChangeFadeEffect(fadeType);
        yield return StartCoroutine(SceneFader.Instance.HandleFade(1f, 0f));

        IsLoadingNewScene = true;

        // load new scene
        AsyncOperation asyncLoad = null;
        if (sceneIdentifier is int index)
        {
            asyncLoad = SceneManager.LoadSceneAsync(index);
        }
        else if (sceneIdentifier is string name)
        {
            asyncLoad = SceneManager.LoadSceneAsync(name);
        }

        if (asyncLoad == null)
        {
            Debug.LogError("Invalid scene identifier provided.");
            yield break;
        }

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        IsLoadingNewScene = false;

        // fade in
        yield return null;

        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.StartFadeInAfterLoad(fadeType);
        }
    }
    
    private void LoadSceneInstantly(object sceneIdentifier)
    {
        if (sceneIdentifier is int index)
        {
            SceneManager.LoadScene(index);
        }
        else if (sceneIdentifier is string name)
        {
            SceneManager.LoadScene(name);
        }
    }

    // public void NextLevel()
    // {
    //     SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    // }

    // public void LoadScene(string sceneName)
    // {
    //     SceneManager.LoadSceneAsync(sceneName);
    // }
}
