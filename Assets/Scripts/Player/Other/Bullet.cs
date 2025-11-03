using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] public float speed = 20f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] public int damage = 1; 

    [Header("Enemy Bullet Source")]
    public bool isHostile = false;

    private Rigidbody2D rb;
    private int direction = 1; // 1 for right, -1 for left

    private Animator camAnim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }


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
        PufferBehaviour pufferfish = other.GetComponent<PufferBehaviour>();
        if (pufferfish != null)
        {
            // PufferBehaviour uses TakeHit, not TakeDamage
            pufferfish.TakeHit(damage);

            Destroy(gameObject);
            return; // Exit the function after hitting the Pufferfish
        }

        // 1. HIT BOSS
        FinalBoss finalboss = other.GetComponent<FinalBoss>();
        StationaryBoss secondboss = other.GetComponent<StationaryBoss>();

        if (finalboss != null)
        {
            // FIX: Use the 'damage' variable, not a hardcoded 50!
            finalboss.TakeDamage(damage);

            Destroy(gameObject);
            return; // Exit the function after hitting the Boss
        }

        if (secondboss != null)
        {
            // FIX: Use the 'damage' variable, not a hardcoded 50!
            secondboss.TakeDamage(damage);

            Destroy(gameObject);
            return; // Exit the function after hitting the Boss
        }

        // --- 2. PLAYER BULLET VS PLAYER (Original Friendly Fire Check) ---
        if (!isHostile && other.CompareTag("Player") || other.CompareTag("Shield"))
        {
            // Player-fired bullets pass through teammates
            return;
        }

        // --- 3. PLAYER BULLET VS GENERIC HEALTH TARGET (Original Enemy/Wall Logic) ---
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