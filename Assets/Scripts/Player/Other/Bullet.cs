using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 1f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private int damage = 1;

    [Header("Enemy Bullet Source")]
    public bool isHostile = false;

    private Rigidbody2D rb;
    private int direction = 1; // 1 for right, -1 for left

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Start()
    {
        // Destroy the bullet after a set time to prevent memory leaks
        Destroy(gameObject, lifeTime);
    }

    // This method is called by PlayerAbilities when the bullet is spawned
    // In Bullet.cs

    // In Bullet.cs

    public void Launch(int launchDirection)
    {
        // The direction variable (1 or -1) is correct for the velocity.
        direction = launchDirection;

        rb.linearVelocity = new Vector2(speed * direction, 0f);
        Debug.Log("Bullet launched with velocity: " + rb.linearVelocity);

        float visualFlipDirection = -direction;
        float scaleValue = 0.6f; // Use your desired scale value here

        transform.localScale = new Vector3(visualFlipDirection * scaleValue, scaleValue, scaleValue);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. ENEMY BULLET VS PLAYER
        if (isHostile && other.CompareTag("Player"))
        {
            // If hostile, check if the player is shielding
            PlayerAbilities playerAbilities = other.GetComponent<PlayerAbilities>();
            if (playerAbilities != null && playerAbilities.IsShielding)
            {
                // Blocked! Destroy the bullet (and maybe reflect it later)
                Destroy(gameObject);
                Debug.Log("Bullet Blocked by Shield!");
                return;
            }

            // --- NEW: Player Death Logic ---

            // Find the GameController to initiate respawn sequence
            PlayerStateController gc = FindFirstObjectByType<PlayerStateController>();

            if (gc != null)
            {
                // Since any hit kills both players, call the GameController's respawn/death method.
                gc.PlayerDied(); // <--- We will create this method next!
            }

            // Destroy the bullet after it hits the player (lethal or not)
            Destroy(gameObject);
            return;
        }

        // --- 2. PLAYER BULLET VS PLAYER (Original Friendly Fire Check) ---
        if (!isHostile && other.CompareTag("Player"))
        {
            // Player-fired bullets pass through teammates
            return;
        }

        // --- 3. PLAYER BULLET VS TARGET (Original Enemy/Wall Logic) ---
        Health targetHealth = other.GetComponent<Health>();

        if (targetHealth != null)
        {
            // Only deal damage if *we* (the bullet) are not hostile (i.e., fired by a player)
            if (!isHostile)
            {
                targetHealth.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        // 4. HIT ENVIRONMENT/OBSTACLE
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
