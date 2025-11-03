using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class HealthbarBehaviour : MonoBehaviour
{
    public Slider Slider;
    public Color Low;
    public Color High;
    public Vector3 Offset;

    void Update()
    {
        // Continuously position the slider above the parent object
        Slider.transform.position = Camera.main.WorldToScreenPoint(transform.parent.position + Offset);
    }

    public void SetHealth(float health, float maxHealth)
    {
        // *** THE FIX IS HERE ***
        // This line ensures the health bar is active only when damage has been taken.
        Slider.gameObject.SetActive(health < maxHealth);

        Slider.value = health;
        Slider.maxValue = maxHealth;

        // Update the color based on the current health percentage
        Slider.fillRect.GetComponentInChildren<Image>().color = Color.Lerp(Low, High, Slider.normalizedValue);
    }
}