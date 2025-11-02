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

    // Cached animator parameter hashes
    private int isGroundedHash;
    private int isWalkingHash;
    private int isAirborneHash;
    private int isAnchoredHash;
    private int jumpTriggerHash;

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
        isAirborneHash = Animator.StringToHash("isAirborne");
        isAnchoredHash = Animator.StringToHash("isAnchored");
        jumpTriggerHash = Animator.StringToHash("JumpTrigger");
    }

    void Update()
    {
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        if (animator == null || movementController == null) return;

        bool isGrounded = movementController.IsGrounded;
        bool isAnchored = movementController.IsAnchored;
        bool isSwinging = movementController.IsSwinging;
        
        // check and consume the jump flag
        bool jumpedThisFrame = movementController.IsJumpExecuted(); 
        
        Vector2 velocity = rb.linearVelocity;
        float absVelocityX = Mathf.Abs(velocity.x);

        // Horizontal/Grounded States
        // isWalking is true only on ground and not anchored
        bool isWalking = isGrounded && !isAnchored && absVelocityX > walkSpeedThreshold;
        
        // Air State (Jump/Fall/Swing)
        // Airborne is true whenever we are not touching the ground AND not anchored to the ground.
        bool isAirborne = !isGrounded && !isAnchored;

        
        // Set Anchor first, as it's the highest priority state
        animator.SetBool(isAnchoredHash, isAnchored);
        
        // Use a Trigger for the instant Jump
        if (jumpedThisFrame)
        {
            animator.SetTrigger(jumpTriggerHash);
        }
        
        // The Airborne state is used for the sustain/fall animation AFTER the jump
        animator.SetBool(isAirborneHash, isAirborne);
        animator.SetBool(isWalkingHash, isWalking);
    }
}