using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The bullet prefab that the player also uses.")]
    [SerializeField] private GameObject hostileBulletPrefab;
    [Tooltip("The empty GameObject from where the bullet will spawn.")]
    [SerializeField] private Transform firePoint;

    [Header("Settings")]
    [Tooltip("Time delay between shots (e.g., 2.0 means one shot every 2 seconds).")]
    [SerializeField] private float fireRate = 2f;
    [Tooltip("The range within which the enemy will start shooting.")]
    [SerializeField] private float attackRange = 15f;

    private float nextFireTime;

    void Update()
    {
        // Simple AI: Find the closest player to shoot at.
        Transform target = FindClosestPlayer();

        // Check if a target is found and is within attack range
        if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            // Turn to face the target before shooting
            FaceTarget(target.position);

            // Fire when the cooldown allows
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    private void Shoot()
    {
        if (firePoint == null || hostileBulletPrefab == null) return;

        // Determine firing direction based on the enemy's facing direction (localScale.x)
        int direction = (transform.localScale.x > 0) ? 1 : -1;

        // 1. Instantiate the bullet
        GameObject bulletObject = Instantiate(hostileBulletPrefab, firePoint.position, Quaternion.identity);

        // 2. Set Bullet Properties
        Bullet bulletScript = bulletObject.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            // CRITICAL: Mark this bullet as hostile!
            bulletScript.isHostile = true;

            // Launch the bullet in the determined direction
            bulletScript.Launch(direction);
        }

        // TODO: Add Enemy shooting sound/VFX
    }

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
    // In EnemyShooting.cs

    private void FaceTarget(Vector3 targetPosition)
    {
        // Determine the required direction (1 for right, -1 for left)
        int requiredDirection = (targetPosition.x > transform.position.x) ? 1 : -1;

        // Get the enemy's current localScale X value
        float currentScaleX = transform.localScale.x;

        // If the required direction is 1 (Right) and current scale is negative (Left), flip it.
        // If the required direction is -1 (Left) and current scale is positive (Right), flip it.
        if ((requiredDirection == 1 && currentScaleX > 0) || (requiredDirection == -1 && currentScaleX < 0))
        {
            // Flip the enemy visually by multiplying the X scale by -1
            transform.localScale = new Vector3(-currentScaleX, transform.localScale.y, transform.localScale.z);
        }
        // NOTE: This logic assumes your enemy sprite initially faces RIGHT when localScale.x is positive (e.g., 1).
        // If your enemy initially faces LEFT when localScale.x is positive, you must reverse this logic.
    }
}