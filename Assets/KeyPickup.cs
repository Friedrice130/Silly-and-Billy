using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public Door door; // drag scene  Door Tilemap object

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            door.OpenDoor();  // use door function
            Destroy(gameObject);  // delete key
        }
    }
}
