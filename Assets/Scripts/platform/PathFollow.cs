using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class PathFollow : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float minDistanceToPoint = 0.1f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float carryHeight = 0.3f; 

    public List<Vector3> points = new List<Vector3>();

    private int currentPoint = 0;
    private Vector3 startPosition;
    private Vector3 prevPosition;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        startPosition = transform.position;
        prevPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (points.Count == 0) return;

        // 
        Vector3 targetPos = startPosition + points[currentPoint];
        Vector3 newPos = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.fixedDeltaTime);
        Vector3 deltaMove = newPos - transform.position;
        rb.MovePosition(newPos);

        // 
        if (Vector3.Distance(newPos, targetPos) < minDistanceToPoint)
        {
            currentPoint++;
            if (currentPoint >= points.Count) currentPoint = 0;
        }
        //
        
        if (deltaMove != Vector3.zero)
        {
            PushPlayers(deltaMove);
        }

        prevPosition = newPos;
    }

    private void PushPlayers(Vector3 deltaMove)
    {
        // 
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            transform.position + Vector3.up * carryHeight,
            new Vector2(1.2f, 0.5f), 
            0f,
            playerLayer
        );

        foreach (var hit in hits)
        {
            //
            hit.transform.position += deltaMove;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + Vector3.up * carryHeight, new Vector3(1.2f, 0.5f, 0));
    }
}
