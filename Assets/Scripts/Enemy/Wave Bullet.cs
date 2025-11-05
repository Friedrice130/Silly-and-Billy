using UnityEngine;

public class WaveProjectile : MonoBehaviour
{
    [Header("Hostile Settings")]
    public float speed = 10f;
    [Tooltip("Damage dealt to players (unused if GameController.Die is called).")]
    public int damage = 20;
    [SerializeField] private float lifeTime = 5f;

    [Header("Knockback Settings")]
    public float knockbackForce = 150f;
    public float knockbackDuration = 0.2f;

    private Rigidbody2D rb;
    private GameController gameController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Use FindObjectOfType<T>() or FindFirstObjectByType<T>() for singletons like GameController
        gameController = FindFirstObjectByType<GameController>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("WaveProjectile touched: " + other.name + " | Tag: " + other.tag);

        //  BLOCKED BY SHIELD
        if (other.CompareTag("Shield") || other.name.Contains("Shield"))
        {
            Debug.Log("WaveProjectile blocked by shield!");
            Destroy(gameObject); // bullet destroyed when shield hit
            return;
        }

        // HITS PLAYER
        if (other.CompareTag("Player") || other.GetComponentInParent<MovementController>() != null)
        {
            MovementController playerController = other.GetComponent<MovementController>()
                ?? other.GetComponentInParent<MovementController>();

            if (playerController != null && gameController != null)
            {
                Debug.Log("--- GUARANTEED KILL ATTEMPTED --- " + playerController.gameObject.name);
                gameController.Die(playerController);
            }

            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Debug.Log("WaveProjectile hit the floor — staying until lifetime ends.");
            return;
        }

        // HITS ANYTHING ELSE (walls, etc.)
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}