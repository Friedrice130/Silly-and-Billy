using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 20f;
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
