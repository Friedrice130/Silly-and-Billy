using UnityEngine;

public class EnemyDamagePlayerOnContact : MonoBehaviour
{
    // The player will die if they touch the enemy collider.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with an object tagged "Player"
        if (collision.gameObject.CompareTag("Player"))
        {
            // Find the GameController to initiate the death sequence
            PlayerStateController gc = FindFirstObjectByType<PlayerStateController>();

            if (gc != null)
            {
                // Instant death upon touching the enemy.
                Debug.Log("Player touched enemy and died!");
                gc.PlayerDied();
            }
        }
    }

    // You can keep the FaceTarget and FindClosestPlayer methods if you still want the enemy to face the player.

    // Helper function to find the closest player
    private Transform FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = player.transform;
            }
        }
        return closest;
    }

    // Helper function to flip the enemy to face the target
    private void FaceTarget(Vector3 targetPosition)
    {
        // Determine the required direction (1 for right, -1 for left)
        int requiredDirection = (targetPosition.x > transform.position.x) ? 1 : -1;

        // Get the enemy's current localScale X value
        float currentScaleX = transform.localScale.x;

        // If the required direction is 1 (Right) and current scale is negative (Left), flip it.
        // If the required direction is -1 (Left) and current scale is positive (Right), flip it.
        if ((requiredDirection == 1 && currentScaleX < 0) || (requiredDirection == -1 && currentScaleX > 0))
        {
            // Flip the enemy visually by multiplying the X scale by -1
            transform.localScale = new Vector3(-currentScaleX, transform.localScale.y, transform.localScale.z);
        }
    }
}