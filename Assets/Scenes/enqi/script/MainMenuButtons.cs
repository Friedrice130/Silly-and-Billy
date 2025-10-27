using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuButtons : MonoBehaviour
{
    public Image black;
    public Animator anim;

    public void startButton()
    {
        StartCoroutine(Fading());
        // SceneManager.LoadScene("Intro"); 
    }

    IEnumerator Fading()
    {
        anim.SetBool("Fade", true);
        SceneManager.LoadScene("Intro");
        yield return new WaitForSeconds(3f); // Adjust based on your fade animation length
    }

    public void quitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in editor
#else
        Application.Quit(); // Quit the built application
#endif
    }

    void Start()
    {

    }

    void Update()
    {

    }
}
