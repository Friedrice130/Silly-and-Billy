using UnityEngine;
// using UnityEngine.UI;

public class BossHealthTrigger : MonoBehaviour
{
    [SerializeField] private StationaryBoss secondBoss;

    [Tooltip("The tag of the object(s) that should trigger the event (e.g., 'Player').")]
    public string targetTag = "Player";

    private void Awake()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            if (secondBoss != null && !secondBoss.isDead)
            {
                Debug.Log("Showing boss health bar.");
                secondBoss.ToggleHealthBar(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            Debug.Log("Player exited boss area.");
            secondBoss.ToggleHealthBar(false);
        }
    }


}
