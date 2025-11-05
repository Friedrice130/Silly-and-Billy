using UnityEngine;

public class FinalBossHPTrigger : MonoBehaviour
{
    // Change the serialized field to reference the NewFinalBoss script
    [SerializeField] private NewFinalBoss targetBoss;

    void Start()
    {
        // Safety check: Try to find the boss if not assigned in the Inspector
        if (targetBoss == null)
        {
            targetBoss = FindFirstObjectByType<NewFinalBoss>();

            if (targetBoss == null)
            {
                Debug.LogError("FinalBossHPTrigger requires a NewFinalBoss component to be assigned or present in the scene.");
                enabled = false;
                return;
            }
        }

        // Use the boss's public method to initially hide the health bar
        targetBoss.ToggleHealthBar(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Show the health bar when a Player enters the trigger
        if (targetBoss != null && other.CompareTag("Player"))
        {
            targetBoss.ToggleHealthBar(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Hide the health bar when a Player exits the trigger, but only if the boss is alive
        if (targetBoss != null && other.CompareTag("Player"))
        {
            // We only hide if the boss is NOT dead.
            if (!targetBoss.isDead)
            {
                targetBoss.ToggleHealthBar(false);
            }
        }
    }
}