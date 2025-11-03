using UnityEngine;

public class Level5Background : MonoBehaviour
{
    [Header("Camera Reference")]
    [SerializeField] private Transform cameraTransform;

    [Header("Parallax Settings")]
    [SerializeField] private float baseParallax = 0.2f; // ???????
    [SerializeField] private float layerStep = 0.1f;    // ??????
    [SerializeField] private float smoothing = 1f;      // ????1~2???

    private Transform[] layers;
    private float[] parallaxScales;
    private Vector3 previousCamPos;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        int count = transform.childCount;
        layers = new Transform[count];
        parallaxScales = new float[count];

        for (int i = 0; i < count; i++)
        {
            layers[i] = transform.GetChild(i);
            parallaxScales[i] = baseParallax + i * layerStep;
        }

        previousCamPos = cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 delta = cameraTransform.position - previousCamPos;

        for (int i = 0; i < layers.Length; i++)
        {
            float parallaxX = delta.x * parallaxScales[i];
            float parallaxY = delta.y * parallaxScales[i];

            Vector3 targetPos = new Vector3(
                layers[i].position.x + parallaxX,
                layers[i].position.y + parallaxY,
                layers[i].position.z
            );

            layers[i].position = Vector3.Lerp(
                layers[i].position,
                targetPos,
                smoothing * Time.deltaTime
            );
        }

        previousCamPos = cameraTransform.position;
    }
}
