using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PortalSwitchScene : MonoBehaviour
{
    [SerializeField] bool goNextLevel; // build index +1
    [SerializeField] string levelName; // type scene name manually 


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
