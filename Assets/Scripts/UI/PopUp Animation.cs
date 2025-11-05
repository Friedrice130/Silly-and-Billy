using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PopupAnimation : MonoBehaviour
{
    public float floatSpeed = 25f;
    public float lifetime = 2f;
    public float fadeOutDuration = 0.5f;

    public float popInDuration = 0.4f;
    public float bounceStrength = 1.2f;

    private CanvasGroup canvasGroup;
    private float timeElapsed;
    private Vector3 initialScale;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        initialScale = transform.localScale;

        transform.localScale = Vector3.one * 0.01f;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed < popInDuration)
        {
            float t = timeElapsed / popInDuration;

            float scaleValue = 1f + bounceStrength * Mathf.Sin(t * Mathf.PI) * (1f - t);

            transform.localScale = initialScale * scaleValue;
        }
        else if (transform.localScale != initialScale)
        {
            transform.localScale = initialScale;
        }

        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);

        float fadeStartTime = lifetime - fadeOutDuration;

        if (timeElapsed >= fadeStartTime)
        {
            float timeSinceFadeStart = timeElapsed - fadeStartTime;
            float alpha = 1f - (timeSinceFadeStart / fadeOutDuration);

            canvasGroup.alpha = Mathf.Clamp01(alpha);
        }
    }
}