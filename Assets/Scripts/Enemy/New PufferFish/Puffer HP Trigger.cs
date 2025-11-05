using UnityEngine;
public class HealthBarProximityTrigger : MonoBehaviour
{
    [SerializeField] private PufferBehaviour targetPuffer;

    void Start()
    {

        if (targetPuffer == null)
        {
            enabled = false;
        }

        if (targetPuffer != null)
        {
            targetPuffer.Healthbar.SetVisible(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (targetPuffer != null && other.CompareTag("Player"))
        {
            targetPuffer.Healthbar.SetVisible(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (targetPuffer != null && other.CompareTag("Player"))
        {
            if (targetPuffer.Hitpoints > 0)
            {
                targetPuffer.Healthbar.SetVisible(false);
            }
        }
    }
}