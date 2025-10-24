using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Targets and Speed")]
    public Transform player1;
    public Transform player2;
    public float verticalOffset = 2.0f;
    public float positionSmoothSpeed = 5.0f;
    public float zoomSmoothSpeed = 3.0f;

    [Header("Zoom Settings")]
    public float baseMinZoom = 7.0f;
    public float zoomPadding = 1.0f;
    public float maxZoom = 12.0f;

    private Camera cam;

    void Awake()
    {
        // Get the Camera component once
        cam = GetComponent<Camera>();

        if (cam == null || !cam.orthographic)
        {
            Debug.LogError("Camera must be set to Orthographic.");
        }

        if (player1 == null || player2 == null)
        {
            Debug.LogError("Player transforms must be assigned in the Inspector.");
        }
    }

    void LateUpdate()
    {
        Vector3 targetPosition = CalculateTargetPosition();
        float targetZoom = CalculateTargetZoom();
        MoveCamera(targetPosition);
        ZoomCamera(targetZoom);
    }

    private Vector3 CalculateTargetPosition()
    {
        Vector3 center = (player1.position + player2.position) / 2f;
        Vector3 offsetCenter = new Vector3(center.x, center.y + verticalOffset, transform.position.z);
        return offsetCenter;
    }

    private float CalculateTargetZoom()
    {
        float distanceX = Mathf.Abs(player1.position.x - player2.position.x);
        float distanceY = Mathf.Abs(player1.position.y - player2.position.y);

        float maxDimension = Mathf.Max(distanceX, distanceY);
        float requiredSize = maxDimension + zoomPadding;

        float aspect = cam.aspect;
        float verticalZoom = (distanceY / 2f) + zoomPadding;
        float requiredZoom = Mathf.Max(verticalZoom, requiredSize / (2f * aspect));
    
        requiredZoom = Mathf.Clamp(requiredZoom, baseMinZoom, maxZoom);

        return requiredZoom;
    }

    private void MoveCamera(Vector3 targetPosition)
    {
        // Lerp to smoothly move the camera's position.
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            positionSmoothSpeed * Time.deltaTime
        );
    }

    private void ZoomCamera(float targetZoom)
    {
        // Lerp to smoothly change the orthographic size (zoom).
        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            zoomSmoothSpeed * Time.deltaTime
        );
    }
}
