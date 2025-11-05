using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PortalSwitchScene : MonoBehaviour
{
    [SerializeField] bool goNextLevel; // build index +1
    [SerializeField] string levelName;
    [SerializeField] private SceneFader.FadeType fadeType = SceneFader.FadeType.Goop;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (SceneLoader.instance == null)
            {
                Debug.LogError("SceneLoader instance is missiong. Cannot switch scene.");
                return;
            }

            if (goNextLevel)
            {
                // SceneLoader.instance.NextLevel();
                SceneLoader.instance.LoadNextLevelWithTransition(fadeType);
            }
            else
            {
                // SceneLoader.instance.LoadScene(levelName);
                SceneLoader.instance.LoadSceneWithTransition(levelName, fadeType);
            }

            GetComponent<Collider2D>().enabled = false;
        }
    }
}
