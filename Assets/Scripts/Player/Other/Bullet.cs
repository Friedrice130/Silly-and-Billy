using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private int damage = 1;

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
        float visualFlipDirection = -direction;
        float scaleValue = 0.6f; // Use your desired scale value here

        transform.localScale = new Vector3(visualFlipDirection * scaleValue, scaleValue, scaleValue);
    }

    // In Bullet.cs

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. IGNORE PLAYERS
        if (other.CompareTag("Player"))
        {
            return;
        }

        // 2. HIT DAMAGEABLE TARGET (Enemy/Target)
        // Try to get the Health component from the collided object
        Health targetHealth = other.GetComponent<Health>();

        if (targetHealth != null)
        {
            // Found a target! Deal damage and destroy the bullet.
            targetHealth.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        // 3. HIT ENVIRONMENT/OBSTACLE (If the bullet hits anything else that is solid)
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
