using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class RopeVerlet : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private Transform _player1;
    [SerializeField] private Transform _player2;
    [SerializeField] private Rigidbody2D _rb1;
    [SerializeField] private Rigidbody2D _rb2;
    [SerializeField] private float _ropeHeightOffset = 0.5f;

    [Header("Rope")]
    [SerializeField] private int _numOfRopeSegments = 20;
    [SerializeField] private float _ropeSegmentLength = 0.225f;

    [Header("Physics")]
    [SerializeField] private Vector2 _gravityForce = new Vector2(0f, -2f);
    [SerializeField] private float _dampingFactor = 0.98f;
    [SerializeField] private LayerMask _collisionMask;
    [SerializeField] private float _collisionRadius = 0.1f;
    [SerializeField] private float _bounceFactor = 0.1f;
    [SerializeField] private float _correctionClampAmount = 0.1f;

    [Header("Constraints")]
    [SerializeField] private int _numOFConstraintRuns = 50;

    [Header("Optimizations")]
    [SerializeField] private int _collisionSegmentInterval = 2;

    private LineRenderer _lineRenderer;
    private List<RopeSegment> _ropeSegments = new List<RopeSegment>();

    private Vector3 _ropeStartPoint;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = _numOfRopeSegments;

        if (_player1 == null || _player2 == null)
        {
            Debug.LogError("RopeVerlet: Player references not set. Drag the player GameObjects into the fields in the Inspector.");
            return;
        }

        float totalRopeLength = _numOfRopeSegments * _ropeSegmentLength;

        Vector2 startPos = (Vector2)_player1.position + new Vector2(0f, _ropeHeightOffset);
        Vector2 endPos = (Vector2)_player2.position + new Vector2(0f, _ropeHeightOffset);
        Vector2 direction = (endPos - startPos).normalized;

        float stepLength = totalRopeLength / _numOfRopeSegments;
    
        Vector2 currentRopePos = startPos;
        _ropeSegments.Clear();

        for (int i = 0; i < _numOfRopeSegments; i++)
        {
            _ropeSegments.Add(new RopeSegment(currentRopePos));
            currentRopePos += direction * stepLength;
        }

        // _ropeStartPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // for (int i = 0; i < _numOfRopeSegments; i++)
        // {
        //     _ropeSegments.Add(new RopeSegment(_ropeStartPoint));
        //     _ropeStartPoint.y -= _ropeSegmentLength;
        // }
    }

    private void Update()
    {
        DrawRope();
    }

    private void FixedUpdate()
    {
        Simulate();

        for (int i = 0; i < _numOFConstraintRuns; i++)
        {
            ApplyConstraints();

            if (i == _numOFConstraintRuns - 1)
            {
                HandlePlayerTension();
            }

            if (i % _collisionSegmentInterval == 0)
                HandleCollisions();
        }
    }

    private void DrawRope()
    {
        Vector3[] ropePositions = new Vector3[_numOfRopeSegments];
        for (int i = 0; i < _ropeSegments.Count; i++)
        {
            ropePositions[i] = _ropeSegments[i].CurrentPosition;
        }

        _lineRenderer.SetPositions(ropePositions);
    }

    private void Simulate()
    {
        for (int i = 0; i < _ropeSegments.Count; i++)
        {
            RopeSegment segment = _ropeSegments[i];
            Vector2 velocity = (segment.CurrentPosition - segment.OldPosition) * _dampingFactor;

            segment.OldPosition = segment.CurrentPosition;
            segment.CurrentPosition += velocity;
            segment.CurrentPosition += _gravityForce * Time.fixedDeltaTime;
            _ropeSegments[i] = segment;
        }
    }

    private void ApplyConstraints()
    {
        Vector2 offset = new Vector2(0f, _ropeHeightOffset);

        // Pin first point to Player 1's current position
        RopeSegment firstSegment = _ropeSegments[0];
        firstSegment.CurrentPosition = (Vector2)_player1.position + offset;
        // firstSegment.CurrentPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        _ropeSegments[0] = firstSegment;

        // Pin last point to Player 2's current position
        RopeSegment lastSegment = _ropeSegments[_numOfRopeSegments - 1];
        lastSegment.CurrentPosition = (Vector2)_player2.position + offset;
        _ropeSegments[_numOfRopeSegments - 1] = lastSegment;

        for (int i = 0; i < _numOfRopeSegments - 1; i++)
        {
            RopeSegment currentSeg = _ropeSegments[i];
            RopeSegment nextSeg = _ropeSegments[i + 1];

            float dist = (currentSeg.CurrentPosition - nextSeg.CurrentPosition).magnitude;
            float difference = (dist - _ropeSegmentLength);

            Vector2 changeDir = (currentSeg.CurrentPosition - nextSeg.CurrentPosition).normalized;
            Vector2 changeVector = changeDir * difference;

            currentSeg.CurrentPosition -= (changeVector * 0.5f);
            nextSeg.CurrentPosition += (changeVector * 0.5f);

            // if (i != 0)
            // {
            //     currentSeg.CurrentPosition -= (changeVector * 0.5f);
            //     nextSeg.CurrentPosition += (changeVector * 0.5f);
            // }
            // else
            // {
            //     nextSeg.CurrentPosition += changeVector;
            // }

            _ropeSegments[i] = currentSeg;
            _ropeSegments[i + 1] = nextSeg;
        }
    }

    private void HandleCollisions()
    {
        for (int i = 1; i < _ropeSegments.Count; i++)
        {
            RopeSegment segment = _ropeSegments[i];
            Vector2 velocity = segment.CurrentPosition - segment.OldPosition;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(segment.CurrentPosition, _collisionRadius, _collisionMask);

            foreach (Collider2D collider in colliders)
            {
                Vector2 closestPoint = collider.ClosestPoint(segment.CurrentPosition);
                float distance = Vector2.Distance(segment.CurrentPosition, closestPoint);

                // if within the collision radius, resolve
                if (distance < _collisionRadius)
                {
                    Vector2 normal = (segment.CurrentPosition - closestPoint).normalized;
                    if (normal == Vector2.zero)
                    {
                        // fallback method
                        normal = (segment.CurrentPosition - (Vector2)collider.transform.position).normalized;
                    }

                    float depth = _collisionRadius - distance;
                    segment.CurrentPosition += normal * depth;

                    velocity = Vector2.Reflect(velocity, normal) * _bounceFactor;
                }
            }

            segment.OldPosition = segment.CurrentPosition - velocity;
            _ropeSegments[i] = segment;
        }
    }
    
    private void HandlePlayerTension()
    {
        float desiredRopeLength = _numOfRopeSegments * _ropeSegmentLength;
        Vector2 p1Pos = _rb1.position;
        Vector2 p2Pos = _rb2.position;
        
        float currentDistance = Vector2.Distance(p1Pos, p2Pos);
        
        // Check if the players are further apart than the total rope length
        if (currentDistance > desiredRopeLength)
        {
            Vector2 ropeDir = (p1Pos - p2Pos).normalized;
            float stretchAmount = currentDistance - desiredRopeLength;

            Vector2 correctionVector = ropeDir * stretchAmount * 0.5f;

            _rb2.position += correctionVector;
            
            Vector2 separationVelocity = Vector2.Dot(_rb1.linearVelocity - _rb2.linearVelocity, ropeDir) * ropeDir;
            _rb1.linearVelocity -= separationVelocity * 0.5f;
            _rb2.linearVelocity += separationVelocity * 0.5f;
        }
    }

    public struct RopeSegment
    {
        public Vector2 CurrentPosition;
        public Vector2 OldPosition;

        public RopeSegment(Vector2 pos)
        {
            CurrentPosition = pos;
            OldPosition = pos;
        }
    }

}
