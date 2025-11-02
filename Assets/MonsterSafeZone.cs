using UnityEngine;

public class MonsterSafeZone : MonoBehaviour
{
    public ChasingMonster monster;  // Reference to the monster script

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player enters the safe zone
        if (other.CompareTag("Player"))
        {
            monster.player = null; // Monster stops chasing
            Debug.Log("Player entered safe zone - monster stops chasing");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Player leaves the safe zone
        if (other.CompareTag("Player"))
        {
            monster.player = other.transform; // Monster starts chasing
            Debug.Log("Player left safe zone - monster starts chasing");
        }
    }
}
