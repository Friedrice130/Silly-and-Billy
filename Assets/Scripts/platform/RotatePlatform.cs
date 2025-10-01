using UnityEngine;

public class RotatePlatform : MonoBehaviour
{
    [SerializeField] float m_MovementSpeed = 0.0f;
    void FixedUpdate()
    {
        transform.Rotate(Vector3.forward, Time.fixedDeltaTime * m_MovementSpeed);
    }

}
