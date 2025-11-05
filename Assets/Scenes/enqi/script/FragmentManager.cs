using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class FragmentManager : MonoBehaviour
{
    public static FragmentManager instance;

    [Header("Fragment Settings")]
    public int totalFragments = 4;
    private int collectedFragments = 0;

    [Header("UI Settings")]
    public GameObject popupPrefab; // prefab with TMP_Text
    public Canvas uiCanvas;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayerPrefs.DeleteAll();
        UpdateFragmentCount(false); // no popup on start

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (uiCanvas == null)
            uiCanvas = FindFirstObjectByType<Canvas>(); // auto reassign in new scene
    }

    public void UpdateFragmentCount(bool showPopup = true)
    {
        // Recount how many fragments are collected
        collectedFragments = 0;
        for (int i = 1; i <= totalFragments; i++)
        {
            if (PlayerPrefs.GetInt("Fragment" + i, 0) == 1)
                collectedFragments++;
        }

        // Show popup only when count > 0 and showPopup is true
        if (showPopup && collectedFragments > 0)
            ShowPopup();
    }

    private void ShowPopup()
    {
        if (popupPrefab != null && uiCanvas != null)
        {
            GameObject popup = Instantiate(popupPrefab, uiCanvas.transform);
            TMP_Text text = popup.GetComponentInChildren<TMP_Text>();

            if (collectedFragments < totalFragments)
                text.text = $"{collectedFragments}/{totalFragments} fragments collected!";
            else
                text.text = $"All {totalFragments}/{totalFragments} fragments collected!";

            Destroy(popup, 2f);
        }
    }

    public bool AllCollected()
    {
        return collectedFragments >= totalFragments;
    }
}
