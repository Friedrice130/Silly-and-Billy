using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(MovementController))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private MovementController movementController;

    [Header("Animation Settings")]
    [SerializeField] private float walkSpeedThreshold = 0.1f;
    [SerializeField] private float jumpVelocityThreshold = 0.5f; // Must be moving upward to show jump

    // Cached animator parameter hashes (more efficient than strings)
    private int isGroundedHash;
    private int isWalkingHash;
    private int isJumpingHash;
    private int isAnchoredHash;

    private Rigidbody2D rb;

    void Awake()
    {
        // Get components
        animator = GetComponent<Animator>();
        movementController = GetComponent<MovementController>();
        rb = GetComponent<Rigidbody2D>();

        // Cache parameter hashes for better performance
        isGroundedHash = Animator.StringToHash("isGrounded");
        isWalkingHash = Animator.StringToHash("isWalking");
        isJumpingHash = Animator.StringToHash("isJumping");
        isAnchoredHash = Animator.StringToHash("isAnchored");
    }

    void Update()
    {
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        if (animator == null || movementController == null) return;

        // Get current velocity
        Vector2 velocity = rb.linearVelocity;
        float absVelocityX = Mathf.Abs(velocity.x);

        // Get states from movement controller
        bool isGrounded = movementController.IsGrounded;
        bool isAnchored = movementController.IsAnchored;
        bool isSwinging = movementController.IsSwinging;

        // Determine if walking (moving horizontally while grounded)
        bool isWalking = isGrounded && !isAnchored && absVelocityX > walkSpeedThreshold;

        // Determine if jumping - only show jump when:
        // 1. Not grounded AND
        // 2. Not anchored AND
        // 3. Either moving upward OR not swinging (if swinging and not jumping up, show idle)
        bool isJumping = !isGrounded && !isAnchored && (velocity.y > jumpVelocityThreshold || !isSwinging);

        // Update animator parameters
        animator.SetBool(isGroundedHash, isGrounded);
        animator.SetBool(isWalkingHash, isWalking);
        animator.SetBool(isJumpingHash, isJumping);
        animator.SetBool(isAnchoredHash, isAnchored);

        // Debug logging (remove after testing)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log($"Jump pressed! isGrounded: {isGrounded}, isJumping: {isJumping}, isSwinging: {isSwinging}, velocity.y: {velocity.y}");
        }
    }
}