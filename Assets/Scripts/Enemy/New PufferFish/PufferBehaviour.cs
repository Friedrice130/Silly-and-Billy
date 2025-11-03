using UnityEngine;

public class PufferBehaviour : MonoBehaviour
{
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
            Destroy(gameObject);
        }

    }


}
