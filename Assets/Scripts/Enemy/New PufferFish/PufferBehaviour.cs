using UnityEngine;

public class PufferBehaviour : MonoBehaviour
{
    [SerializeField] private float damageToPlayer = 1f;
    public float Hitpoints;
    public float MaxHitpoints = 5;
    public HealthbarBehaviour Healthbar;

    // patrol
    public float speed = 2f;
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;
    public float rotationSpeed = 5f;

    void Start()
    {
        Hitpoints = MaxHitpoints;
        Healthbar.SetHealth(Hitpoints, MaxHitpoints);
    }

    void Update()
    {
        if (waypoints.Length == 0) return;

        Vector3 targetPosition = waypoints[currentWaypointIndex].position;
        targetPosition.z = transform.position.z;
        float directionX = targetPosition.x - transform.position.x;

        if (Mathf.Abs(directionX) > 0.01f) // Check if moving left/right
        {
            // Get the current local scale
            Vector3 localScale = transform.localScale;

            // If moving right (positive X direction), ensure scale X is positive
            if (directionX > 0)
            {
                localScale.x = -Mathf.Abs(localScale.x);
            }
            // If moving left (negative X direction), ensure scale X is negative (flipped)
            else
            {
                localScale.x = Mathf.Abs(localScale.x);
            }

            // Apply the new scale (flip)
            transform.localScale = localScale;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }


    public void TakeHit(float damage)
    {
        Hitpoints -= damage;
        Healthbar.SetHealth(Hitpoints, MaxHitpoints);

        if (Hitpoints <= 0)
        {
            if (Healthbar != null)
            {
                Healthbar.gameObject.SetActive(false);
            }

            Destroy(gameObject);
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Check if the collision is with the Player
        // Assuming your player has the tag "Player"
        if (collision.CompareTag("Player"))
        {
            // 2. Get the PlayerAbilities component from the player
            PlayerAbilities playerAbilities = collision.GetComponent<PlayerAbilities>();

            // 3. Check for the shield!
            if (playerAbilities != null && playerAbilities.IsShielding)
            {
                // Player is shielding! The puffer fish attack is blocked.
                Debug.Log("Puffer attack blocked by shield!");
                return; // **Stop the attack/damage logic here!**
            }

            // 4. If not shielding, proceed with the attack (player death)
            Debug.Log("Player hit! Not shielding.");

            // Find the GameController to call the Die method
            MovementController playerMovementController = collision.GetComponent<MovementController>();
            if (playerMovementController != null)
            {
                // Assuming GameController.Die takes the MovementController as an argument,
                // like in your MovementController's OnTriggerEnter2D: gameController.Die(this);
                // You'll need to make sure the GameController is accessible, e.g., through a static instance or FindObjectOfType.
                GameController gameController = FindFirstObjectByType<GameController>();
                if (gameController != null)
                {
                    gameController.Die(playerMovementController);
                }
            }

            // Alternatively, if the puffer fish should only die/damage itself on collision,
            // you might want to call TakeHit on the puffer fish here too: TakeHit(damageToPlayer);
        }
    }
}

  
