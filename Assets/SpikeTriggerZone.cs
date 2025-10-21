using UnityEngine;

public class SpikeTriggerZone : MonoBehaviour
{
    private FallingSpike parentSpike;

    private void Start()
    {
        parentSpike = GetComponentInParent<FallingSpike>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("player get in  Spike dowm area");
            parentSpike.TriggerDrop();
        }
    }
}
