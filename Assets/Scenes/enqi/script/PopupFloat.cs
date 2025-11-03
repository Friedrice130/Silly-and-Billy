using UnityEngine;

public class PopupFloat : MonoBehaviour
{
    public float floatSpeed = 25f;
    public float lifetime = 2f;

    void Update()
    {
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
