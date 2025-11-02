using UnityEngine;

public class TestPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    public float waitTime = 0f; //

    private Vector3 target;
    private float fixedY;
    private bool goingToB = true;
    private float waitTimer = 0f;

    void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("PointA or PointB not assigned!");
            enabled = false;
            return;
        }

        target = pointB.position;
        fixedY = transform.position.y;
    }

    void Update()
    {
        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        Vector3 newPos = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        newPos.y = fixedY;
        transform.position = newPos;


        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            // 
            goingToB = !goingToB;
            target = goingToB ? pointB.position : pointA.position;
            waitTimer = waitTime;
        }
    }
}