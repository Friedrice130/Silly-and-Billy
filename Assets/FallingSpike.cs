using UnityEngine;

public class FallingSpike : MonoBehaviour
{
    public float dropDelay = 0.5f;
    public float dropSpeed = 8f;
    private Rigidbody2D rb;
    private bool hasFallen = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true; // ?????
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
        Debug.Log("Spike ???");
    }
}
