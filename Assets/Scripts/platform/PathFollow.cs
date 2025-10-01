using UnityEngine;
using System.Collections.Generic;

public class PathFollow : MonoBehaviour
{
    public enum MoveDirections { LEFT, RIGHT }

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float minDistanceToPoint = 0.1f;

    public float MoveSpeed => moveSpeed;
    public MoveDirections Direction { get; private set; }
    public List<Vector3> points = new List<Vector3>();

    private bool _playing;
    private bool _moved;
    private int _currentPoint = 0;
    private Vector3 _currentPosition;
    private Vector3 _previousPosition;
    private Vector3 _platformDelta;

    // Players follow the platform
    private Transform _playerOnPlatform;

    private void Start()
    {
        _playing = true;
        _previousPosition = transform.position;
        _currentPosition = transform.position;

        if (points.Count > 0)
        {
            transform.position = _currentPosition + points[0];
            _currentPoint = 1;
            _moved = true;
        }
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        if (!_moved || points.Count == 0) return;

        Vector3 targetPos = _currentPosition + points[_currentPoint];
        Vector3 oldPos = transform.position;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * moveSpeed);
        _platformDelta = transform.position - oldPos; // Platform movement

        // If there is a player on the platform, follow the movement
        if (_playerOnPlatform != null)
        {
            _playerOnPlatform.position += _platformDelta;
        }

        // Determine the direction of movement
        if (_previousPosition != Vector3.zero)
        {
            if (transform.position.x > _previousPosition.x) Direction = MoveDirections.RIGHT;
            else if (transform.position.x < _previousPosition.x) Direction = MoveDirections.LEFT;
        }

        _previousPosition = transform.position;

        // Reach the destination?
        if (Vector3.Distance(transform.position, targetPos) < minDistanceToPoint)
        {
            _currentPoint++;
            if (_currentPoint >= points.Count) _currentPoint = 0;
        }
    }

    // Player enters platform collision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            _playerOnPlatform = collision.collider.transform;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            _playerOnPlatform = null;
        }
    }

    private void OnDrawGizmos()
    {
        if (points != null && points.Count > 0)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_currentPosition + points[i], 0.4f);

                Gizmos.color = Color.black;
                if (i < points.Count - 1)
                    Gizmos.DrawLine(_currentPosition + points[i], _currentPosition + points[i + 1]);
                else
                    Gizmos.DrawLine(_currentPosition + points[i], _currentPosition + points[0]);
            }
        }
    }
}