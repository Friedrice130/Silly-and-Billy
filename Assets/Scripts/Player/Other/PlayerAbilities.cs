using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAbilities : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MovementController movementController;
    [SerializeField] private GameObject bulletPrefab; // Assign your bullet prefab here
    [SerializeField] private Transform attackPoint;    // Set a point for the bullet to spawn
    [SerializeField] private GameObject shieldVisual; // Visual representation of the shield
    [SerializeField] private Collider2D shieldCollider; // Collider for the shield (if needed for hit detection)
    [SerializeField] private Transform shieldPointParent;

    [Header("Attack Settings")]
    [SerializeField] private float fireRate = 0.5f;
    private float nextFireTime = 0f;

    // REMOVED: [Header("Shield Settings")]
    // REMOVED: [Tooltip("Is this player the attacker or the shielder?")]
    // REMOVED: [SerializeField] private bool isAttacker = true; // Use this to differentiate roles

    [Header("Input Setup (from MovementController's action map)")]
    private InputAction attackAction;
    private InputAction shieldAction;

    private bool isShielding = false;
    private bool isPlayer1Attacker; // NEW FIELD to determine role

    private void Awake()
    {
        // NO initialization here, move it to OnEnable for safety

        // Initialize shield state
        if (shieldVisual != null) shieldVisual.SetActive(false);
        if (shieldCollider != null) shieldCollider.enabled = false;
    }

    private void OnEnable()
    {
        // **CRITICAL SAFETY CHECK AND INITIALIZATION:**
        // Ensures 'movementController' is assigned before accessing properties
        if (movementController == null)
        {
            movementController = GetComponent<MovementController>();
        }

        if (movementController == null)
        {
            Debug.LogError("MovementController reference is missing on the PlayerAbilities script. Cannot initialize input.");
            return;
        }

        // Set role based on map name AFTER we confirm movementController is valid
        // This is safe because 'movementController.actionMapName' is a serialized field, not dependent on Awake().
        isPlayer1Attacker = (movementController.actionMapName == "Player1WASD");

        // 1. Get the controls instance from the MovementController
        // This now calls the defensive property, which guarantees 'controls' is not null.
        PlayerActions controls = movementController.Controls;
        string mapName = movementController.actionMapName;

        // 2. ENFORCE ROLES based on the Action Map name
        if (mapName == "Player1WASD")
        {
            attackAction = controls.Player1WASD.Attack; // Line 64 (or similar, depending on formatting)
            shieldAction = null;
        }
        else if (mapName == "Player2ArrowKeys")
        {
            attackAction = null;
            shieldAction = controls.Player2ArrowKeys.Shield; // Line 71 (or similar)
        }

        // 3. SUBSCRIBE ONLY TO THE ASSIGNED ACTION
        // ... (Subscription logic remains the same) ...
        if (attackAction != null)
        {
            attackAction.performed += OnAttack;
            attackAction.Enable();
        }

        if (shieldAction != null)
        {
            shieldAction.performed += OnShieldStart;
            shieldAction.canceled += OnShieldEnd;
            shieldAction.Enable();
        }
    }

    private void OnDisable()
    {
        // 4. UNSUBSCRIBE ONLY FROM ASSIGNED ACTIONS
        if (attackAction != null)
        {
            attackAction.performed -= OnAttack;
            attackAction.Disable();
        }

        if (shieldAction != null)
        {
            shieldAction.performed -= OnShieldStart;
            shieldAction.canceled -= OnShieldEnd;
            shieldAction.Disable();
        }
    }

    // --- Attack Implementation (P1 - Attacker) ---
    private void OnAttack(InputAction.CallbackContext context)
    {
        // No role check needed: only the Attacker player's script subscribes to this method.
        if (Time.time >= nextFireTime)
        {
            // Only allow attack if not anchored
            if (!movementController.IsAnchored)
            {
                Attack();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    private void Attack()
    {
        // Get the direction the player is facing (1 or -1)
        int direction = (int)Mathf.Sign(transform.localScale.x);

        // Instantiate the bullet at the attack point
        GameObject bulletObject = Instantiate(bulletPrefab, attackPoint.position, Quaternion.identity);

        // Get the bullet script and launch it
        Bullet bulletScript = bulletObject.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Launch(direction);
        }

        // TODO: Add VFX and sound for attack
    }

    // --- Shield Implementation (P2 - Shielder) ---
    private void OnShieldStart(InputAction.CallbackContext context)
    {
        // No role check needed: only the Shielder player's script subscribes to this method.
        isShielding = true;
        // Activate the visual and collider
        if (shieldVisual != null) shieldVisual.SetActive(true);
        if (shieldCollider != null) shieldCollider.enabled = true;
    }

    private void OnShieldEnd(InputAction.CallbackContext context)
    {
        // No role check needed
        isShielding = false;
        // Deactivate the visual and collider
        if (shieldVisual != null) shieldVisual.SetActive(false);
        if (shieldCollider != null) shieldCollider.enabled = false;
    }

    // Public property for other scripts (e.g., enemy attacks) to check
    public bool IsShielding => isShielding;
}