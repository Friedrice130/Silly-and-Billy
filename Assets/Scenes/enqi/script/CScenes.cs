using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;  // For XR input

public class CScenes : MonoBehaviour
{
    public List<GameObject> pages;
    private int currentPageIndex = 0;

    private List<GameObject> currentSteps = new List<GameObject>();
    private int currentStepIndex = 0;

    public AudioClip clickSound;
    public AudioClip sound;

    private AudioSource audioSource;

    public Image black;
    public Animator anim;

    private InputDevice rightController;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        ShowPage(0);
        InitializeControllers();
    }

    void InitializeControllers()
    {
        var rightHandedControllers = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandedControllers);

        if (rightHandedControllers.Count > 0)
        {
            rightController = rightHandedControllers[0];
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ShowNextStep();
        }

        bool triggerPressed = false;
        if (rightController.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed) && triggerPressed)
        {
            ShowNextStep();
        }
    }


    void ShowPage(int index)
    {
        foreach (var page in pages) page.SetActive(false);

        currentPageIndex = index;
        if (currentPageIndex >= pages.Count)
        {
            EndCutscene();
            return;
        }

        GameObject currentPage = pages[currentPageIndex];
        currentPage.SetActive(true);

        currentSteps.Clear();
        foreach (Transform child in currentPage.transform)
        {
            if (child.CompareTag("CutscenesElement"))
            {
                child.gameObject.SetActive(false);
                currentSteps.Add(child.gameObject);
            }
        }

        Button nextButton = currentPage.transform.Find("Next")?.GetComponent<Button>();
        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (nextButton != null) nextButton.onClick.RemoveAllListeners();
        if (nextButton != null) nextButton.onClick.AddListener(ShowNextPage);

        currentStepIndex = 0;
        ShowNextStep();
    }

    void ShowNextStep()
    {
        if (currentStepIndex < currentSteps.Count)
        {
            GameObject step = currentSteps[currentStepIndex];
            step.SetActive(true);

            Animator anim = step.GetComponent<Animator>();
            if (anim != null)
            {
                anim.ResetTrigger("Appear");
                anim.SetTrigger("Appear");
            }

            if (clickSound != null)
            {
                audioSource.PlayOneShot(clickSound);
            }

            currentStepIndex++;

            if (currentStepIndex == currentSteps.Count)
            {
                Button nextButton = pages[currentPageIndex].transform.Find("Next")?.GetComponent<Button>();
                if (nextButton != null)
                {
                    nextButton.gameObject.SetActive(true);
                }
            }
        }
    }


    void ShowNextPage()
    {
        ShowPage(currentPageIndex + 1);
    }

    void EndCutscene()
    {
        // StartCoroutine(Fading());
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level 1");
    }

    IEnumerator Fading()
    {
        anim.SetBool("Fade", true);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Level 1");
    }
}
