using UnityEngine;
using UnityEngine.UI;

public class HealthbarBehaviour : MonoBehaviour
{
    public Slider Slider;
    public Color Low;
    public Color High;
    public Vector3 Offset;

    void Start()
    {
        Slider.gameObject.SetActive(false);
    }

    void Update()
    {

    }

    public void SetVisible(bool visible)
    {
        Slider.gameObject.SetActive(visible);
    }


    public void SetHealth(float health, float maxHealth)
    {
        Slider.value = health;
        Slider.maxValue = maxHealth;

        Slider.fillRect.GetComponentInChildren<Image>().color = Color.Lerp(Low, High, Slider.normalizedValue);
    }
}