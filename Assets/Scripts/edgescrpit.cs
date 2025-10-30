using UnityEngine;

public class edgescrpit : MonoBehaviour
{
    public float speed = 2f;          // speed of the platform
    public int startingPoint = 0;     // index of starting point
    public Transform[] points;        // waypoints

    private int i; // current target index

    void Start()
    {
        // Start from chosen point
        transform.position = points[startingPoint].position;
        i = startingPoint;
    }

    void Update()
    {
        // Move towards current target
        transform.position = Vector2.MoveTowards(
            transform.position,
            points[i].position,
            speed * Time.deltaTime
        );

        // If very close to target, switch to next point
        if (Vector2.Distance(transform.position, points[i].position) < 0.02f)
        {
            i++;
            if (i >= points.Length) // loop back if out of bounds
            {
                i = 0;
            }
        }
    }
}
