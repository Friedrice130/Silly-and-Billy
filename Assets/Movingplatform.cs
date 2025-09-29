using UnityEngine;

public class Movingplatform : MonoBehaviour
{
    public Transform pointA;   // empty GameObject for start point
    public Transform pointB;   // empty GameObject for end point
    public float speed = 2f;

    private Transform target;

    void Start()
    {
        target = pointB;
    }

    void Update()
    {
        // move platform
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // switch target when reaching
        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            target = (target == pointA) ? pointB : pointA;
        }
    }
}
