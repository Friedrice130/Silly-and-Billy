using UnityEngine;

public class Fragment : MonoBehaviour
{
    public string fragmentID; // e.g. "Fragment1", "Fragment2", etc.

    private bool collected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player touched fragment " + fragmentID);

            PlayerPrefs.SetInt("Fragment" + fragmentID, 1);
            PlayerPrefs.Save();

            if (FragmentManager.instance != null)
            {
                Debug.Log("Calling FragmentManager...");
                FragmentManager.instance.UpdateFragmentCount();
            }

            Destroy(gameObject);
        }
    }
}
