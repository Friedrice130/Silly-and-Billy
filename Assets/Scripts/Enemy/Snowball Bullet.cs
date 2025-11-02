using UnityEngine;

public class SnowballBullet : MonoBehaviour
{
    [Header("Hostile Settings")]
    [Tooltip("Speed the projectile travels. This is overridden by the Boss.")]
    public float speed = 10f;
    [Tooltip("Damage dealt to players (unused if GameController.Die is called).")]
    public int damage = 20;
    [SerializeField] private float lifeTime = 5f;

    [Header("Knockback Settings")]
    public float knockbackForce = 150f;
    public float knockbackDuration = 0.2f;

    private Rigidbody2D rb;
    private GameController gameController;
    // NEW: Reference to the StationaryBoss for co-op shield check
    private StationaryBoss stationaryBoss;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        gameController = FindFirstObjectByType<GameController>();
        // Find the boss when the projectile is created
        stationaryBoss = FindFirstObjectByType<StationaryBoss>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Use CompareTag for reliable player identification
        if (other.CompareTag("Player"))
        {
            // Check if the projectile hit a Player
            MovementController playerController = other.GetComponent<MovementController>()
                 ?? other.GetComponentInParent<MovementController>();

            if (playerController != null)
            {
                Rigidbody2D playerRb = playerController.rb;

                // --- KNOCKBACK IMPLEMENTATION ---
                if (playerRb != null)
                {
                    Vector2 direction = (playerRb.transform.position - transform.position).normalized;
                    playerRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
                }

                // GLOBAL SHIELD CHECK (Co-op Shield Logic from StationaryBoss)
                bool attackBlocked = false;
                if (stationaryBoss != null)
                {
                    attackBlocked = stationaryBoss.IsAnyPlayerShielding();
                }
                else
                {
                    // Fallback to checking only the hit player's shield if boss reference is missing
                    PlayerAbilities abilities = playerController.GetComponent<PlayerAbilities>();
                    attackBlocked = abilities != null && abilities.IsShielding;
                }

                // 2. APPLY DAMAGE OR BLOCK EFFECT
                if (!attackBlocked)
                {
                    if (gameController != null)
                    {
                        gameController.Die(playerController);
                    }
                }

                // Destroy the bullet 
                Destroy(gameObject);
                return;
            }
        }

        // Check if the projectile hit a wall/obstacle AND it's not a trigger (like a collectible)
        if (!other.isTrigger)
        {
            Destroy(gameObject);
            return;
        }
    }
}