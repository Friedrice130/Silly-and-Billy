using UnityEngine;
using UnityEngine.UI; // Needed for Slider and Image
using System.Collections; // Often useful, included for completeness

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("The starting and maximum health value.")]
    [SerializeField] private float maxHealth = 3f;

    [Header("Health Bar UI References")]
    [Tooltip("Assign the root Slider component of the health bar here.")]
    [SerializeField] private Slider healthSlider;
    [Tooltip("The Image component of the Slider's fill area.")]
    [SerializeField] private Image fillImage;
    [Tooltip("Color for low health (e.g., Red).")]
    [SerializeField] private Color lowColor = Color.red;
    [Tooltip("Color for high health (e.g., Green).")]
    [SerializeField] private Color highColor = Color.green;
    [Tooltip("Offset for the health bar's world position.")]
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 0.5f, 0);

    private float currentHealth;

    // Public property to allow the Enemy.cs to set the starting health
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }

    private void Awake()
    {
        currentHealth = maxHealth;

        // Initial setup for the health bar
        if (healthSlider != null)
        {
            SetHealthUI(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// Updates the health bar's value, max value, and color.
    /// </summary>
    private void SetHealthUI(float health, float max)
    {
        // Toggle visibility: only show the bar if health is not at max
        healthSlider.gameObject.SetActive(health < max);

        healthSlider.maxValue = max;
        healthSlider.value = health;

        // Lerp color based on health percentage (normalizedValue)
        if (fillImage != null)
        {
            fillImage.color = Color.Lerp(lowColor, highColor, healthSlider.normalizedValue);
        }
    }

    // New Update method to make the health bar follow the enemy in world space
    private void Update()
    {
        if (healthSlider != null && Camera.main != null)
        {
            // Position the Slider above the object using WorldToScreenPoint
            healthSlider.transform.position = Camera.main.WorldToScreenPoint(transform.position + uiOffset);
        }
    }

    /// <summary>
    /// Reduces health and checks for death.
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damageAmount;

        // Update the health bar UI immediately after taking damage
        if (healthSlider != null)
        {
            SetHealthUI(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has been destroyed!");

        // OPTIONAL: Hide the health bar immediately upon death
        if (healthSlider != null) healthSlider.gameObject.SetActive(false);

        Destroy(gameObject);
    }
}