using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalSwitchScene : MonoBehaviour
{
    [SerializeField] bool goNextLevel;
    [SerializeField] string levelName;

    private void OnTriggerEnter2D(Collider2D collision)

    {
        if (collision.CompareTag("Player"))
        {
            if (goNextLevel)
            {
                SceneLoader.instance.NextLevel();
            }
            else
            {
                SceneLoader.instance.LoadScene(levelName);
            }
        }
    }
}