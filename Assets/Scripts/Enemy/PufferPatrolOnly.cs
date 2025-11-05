using UnityEngine;

public class PufferPatrolOnly : MonoBehaviour
{
    [SerializeField] private float damageToPlayer = 1f;

    public float speed = 2f;
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;
    public float rotationSpeed = 5f;

    void Start()
    {
    }

    void Update()
    {
        if (waypoints.Length == 0) return;

        Vector3 targetPosition = waypoints[currentWaypointIndex].position;
        targetPosition.z = transform.position.z;
        float directionX = targetPosition.x - transform.position.x;

        if (Mathf.Abs(directionX) > 0.01f)
        {
            Vector3 localScale = transform.localScale;

            if (directionX > 0)
            {
                localScale.x = -Mathf.Abs(localScale.x);
            }
            else
            {
                localScale.x = Mathf.Abs(localScale.x);
            }

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




    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerAbilities playerAbilities = collision.GetComponent<PlayerAbilities>();

            if (playerAbilities != null && playerAbilities.IsShielding)
            {
                Debug.Log("Puffer attack blocked by shield!");
                return;
            }

            Debug.Log("Player hit! Not shielding.");

            MovementController playerMovementController = collision.GetComponent<MovementController>();
            if (playerMovementController != null)
            {
                GameController gameController = FindFirstObjectByType<GameController>();
                if (gameController != null)
                {
                    gameController.Die(playerMovementController);
                }
            }
        }
    }
}