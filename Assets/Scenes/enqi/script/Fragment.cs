using UnityEngine;

public class Fragment : MonoBehaviour
{
    // new 
    public GameObject collectedUIPrefab;

    public string fragmentID; // e.g. "Fragment1", "Fragment2", etc.
    private bool collected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            //collected = true;
            //Debug.Log("Player touched fragment " + fragmentID);

            //PlayerPrefs.SetInt("Fragment" + fragmentID, 1);
            //PlayerPrefs.Save();

            //if (FragmentManager.instance != null)
            //{
            //    Debug.Log("Calling FragmentManager...");
            //    FragmentManager.instance.UpdateFragmentCount();
            //}

            // -------- UI MANUAL TYPE

            if (collectedUIPrefab != null)
            {
                GameObject canvas = GameObject.FindWithTag("PopUp");

                if (canvas != null)
                {
                    GameObject uiPanel = Instantiate(collectedUIPrefab, canvas.transform);
                    Debug.Log("Spawned collected UI panel: " + uiPanel.name);
                }
                else
                {
                    Debug.LogError("Popup not found! Tag your primary Canvas as 'Popup'.");
                }
            }
            else
            {
                Debug.LogWarning("collectedUIPrefab is not assigned on the Fragment component!");
            }

            Destroy(gameObject);
        }
    }
}
