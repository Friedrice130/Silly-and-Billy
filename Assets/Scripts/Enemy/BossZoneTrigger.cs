using UnityEngine;

public class BossZoneTrigger : MonoBehaviour
{
    [SerializeField] private FinalBoss finalBoss;
    
    [Tooltip("The tag of the object(s) that should trigger the event (e.g., 'Player').")]
    public string targetTag = "Player";

    private void Awake()
    {
        if (finalBoss == null)
        {
            finalBoss = FindFirstObjectByType<FinalBoss>();
            if (finalBoss == null)
            {
                Debug.LogError("BossZoneTrigger: FinalBoss reference is missing and could not be found!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            if (finalBoss != null && !finalBoss.isDead)
            {
                finalBoss.ToggleHealthBar(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            finalBoss.ToggleHealthBar(false); 
        }
    }
}