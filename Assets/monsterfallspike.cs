using UnityEngine;

public class monsterfallspike : MonoBehaviour
{
    public float dropDelay = 0.5f;      // Delay before falling
    public float dropSpeed = 8f;        // Falling speed
    public float respawnDelay = 5f;     // Time to reset spike after falling

    private Rigidbody2D rb;
    private bool hasFallen = false;
    private Vector3 originalPosition;   // Remember where spike started
    private Quaternion originalRotation;
    private Collider2D spikeCollider;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spikeCollider = GetComponent<Collider2D>();
        rb.isKinematic = true; // Stay still at start
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    public void TriggerDrop()
    {
        if (!hasFallen)
        {
            hasFallen = true;
            Invoke(nameof(DropSpike), dropDelay);
        }
    }

    private void DropSpike()
    {
        rb.isKinematic = false;
        rb.linearVelocity = new Vector2(0, -dropSpeed);
        Debug.Log($"{gameObject.name} falling!");

        // Schedule reset
        Invoke(nameof(ResetSpike), respawnDelay);
    }

    private void ResetSpike()
    {
        // Stop all movement and reset position
        rb.isKinematic = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        transform.position = originalPosition;
        transform.rotation = originalRotation;

        hasFallen = false;
        Debug.Log($"{gameObject.name} reset and ready again!");
    }
}
