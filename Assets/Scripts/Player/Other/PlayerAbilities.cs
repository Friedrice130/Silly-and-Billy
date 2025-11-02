using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAbilities : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MovementController movementController;
    [SerializeField] private GameObject bulletPrefab; 
    [SerializeField] private Transform attackPoint;    
    [SerializeField] private GameObject shieldVisual; 
    [SerializeField] private Collider2D shieldCollider; 

    [Header("Attack Settings")]
    [SerializeField] private float fireRate = 0.5f;
    private float nextFireTime = 0f;

    [Header("Input Setup (from MovementController's action map)")]
    private InputAction attackAction;
    private InputAction shieldAction;

    private bool isShielding = false;
    private bool isPlayer1Attacker; 

    private void Awake()
    {
        if (shieldVisual != null) shieldVisual.SetActive(false);
        if (shieldCollider != null) shieldCollider.enabled = false;
    }

    private void OnEnable()
    {
        if (movementController == null)
        {
            movementController = GetComponent<MovementController>();
        }

        if (movementController == null)
        {
            Debug.LogError("MovementController reference is missing on the PlayerAbilities script. Cannot initialize input.");
            return;
        }

        isPlayer1Attacker = (movementController.actionMapName == "Player1WASD");

        // 1. Get the controls instance from the MovementController
        PlayerActions controls = movementController.Controls;
        string mapName = movementController.actionMapName;

        // 2. ENFORCE ROLES based on the Action Map name
        if (mapName == "Player1WASD")
        {
            attackAction = controls.Player1WASD.Attack; 
            shieldAction = null;
        }
        else if (mapName == "Player2ArrowKeys")
        {
            attackAction = null;
            shieldAction = controls.Player2ArrowKeys.Shield; 
        }

        // 3. Subscibe only to 1 action
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
        // 4. unsubscribe
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

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (Time.time >= nextFireTime)
        {
            if (!movementController.IsAnchored)
            {
                Attack();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    private void Attack()
    {
        int direction = (int)Mathf.Sign(transform.localScale.x);

        GameObject bulletObject = Instantiate(bulletPrefab, attackPoint.position, Quaternion.identity);

        Bullet bulletScript = bulletObject.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Launch(direction);
        }
    }

    private void OnShieldStart(InputAction.CallbackContext context)
    {
        isShielding = true;
        if (shieldVisual != null) shieldVisual.SetActive(true);
        if (shieldCollider != null) shieldCollider.enabled = true;
    }

    private void OnShieldEnd(InputAction.CallbackContext context)
    {
        isShielding = false;
        if (shieldVisual != null) shieldVisual.SetActive(false);
        if (shieldCollider != null) shieldCollider.enabled = false;
    }

    public bool IsShielding => isShielding;
}