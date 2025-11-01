using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private int damage = 1;

    private Rigidbody2D rb;
    private int direction = 1; // 1 for right, -1 for left

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Destroy the bullet after a set time to prevent memory leaks
        Destroy(gameObject, lifeTime);
    }

    // This method is called by PlayerAbilities when the bullet is spawned
    public void Launch(int launchDirection)
    {
        direction = launchDirection;
        // Apply velocity in the set direction
        rb.linearVelocity = new Vector2(speed * direction, 0f); 

        // Optional: Flip the sprite visually if needed
        transform.localScale = new Vector3(direction, -1, 1);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we hit an enemy (you'll need an Enemy component/tag)
        if (other.CompareTag("Enemy"))
        {
            // TODO: Call a damage method on the enemy
            // Example: other.GetComponent<EnemyHealth>().TakeDamage(damage);

            Destroy(gameObject); // Destroy the bullet on impact
            return;
        }

        // Check if we hit a wall/ground (using the same layer as player ground)
        // Adjust the LayerMask check based on your scene setup
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject); // Bullet breaks on a wall
        }
    }
}