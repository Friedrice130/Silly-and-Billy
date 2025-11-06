using UnityEngine;

public class FinalBossHPTrigger : MonoBehaviour
{
    [SerializeField] private NewFinalBoss targetBoss;

    void Start()
    {
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

        targetBoss.ToggleHealthBar(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (targetBoss != null && other.CompareTag("Player"))
        {
            targetBoss.ToggleHealthBar(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // hide health bar when player exits the trigger, but only if the boss is alive
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