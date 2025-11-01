using UnityEngine;

[RequireComponent(typeof(Health))] // Ensure the enemy always has a Health component
public class Enemy : MonoBehaviour
{
    [Header("Enemy Type Settings")]
    [Tooltip("Health specific to this enemy type.")]
    [SerializeField] private int enemyMaxHealth = 5;

    // Optional: Add other settings here (e.g., score value, speed, attack damage)

    private void Awake()
    {
        // Get the Health component and set its max health based on this enemy's type setting
        Health healthComponent = GetComponent<Health>();
        if (healthComponent != null)
        {
            healthComponent.MaxHealth = enemyMaxHealth;
        }
    }
}