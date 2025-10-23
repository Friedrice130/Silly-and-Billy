using UnityEngine;
using UnityEngine.InputSystem;

using Vector2 = UnityEngine.Vector2;

public class MovementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private CapsuleCollider2D col;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 14f;
    [SerializeField] private float acceleration = 120f;
    [SerializeField] private float groundDeceleration = 60f;
    [SerializeField] private float airDeceleration = 30f;

    [Header("Jumping")]
    [SerializeField] private float jumpPower = 36f;
    [SerializeField] private float maxFallSpeed = 40f;
    [SerializeField] private float fallAcceleration = 110f;
    [SerializeField] private float jumpEndEarlyGravityModifier = 3f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBuffer = 0.2f;

    [Header("Input Setup")]
    [Tooltip("Choose which action map this player should use (Player1WASD / Player2ArrowKeys).")]
    [SerializeField] private string actionMapName = "Player1WASD";

    private PlayerActions controls;
    private InputAction moveAction;
    private InputAction jumpAction;

    private Vector2 moveInput;
    private Vector2 frameVelocity;

    // state tracking
    private bool grounded;
    private bool endedJumpEarly;
    private bool jumpToConsume;
    private bool bufferedJumpUsable;
    private bool coyoteUsable;
    private bool jumpHeld;
    private bool anchorInputHeld;
    private bool isAnchored;
    private bool airJumpUsable;
    private float frameLeftGrounded = float.MinValue;
    private float timeJumpWasPressed;
    private float time;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        controls = new PlayerActions();
    }

    void OnEnable()
    {
        if (actionMapName == "Player1WASD")
        {
            controls.Player1WASD.Enable();
            moveAction = controls.Player1WASD.Movement;
            jumpAction = controls.Player1WASD.Jump;
        }
        else if (actionMapName == "Player2ArrowKeys")
        {
            controls.Player2ArrowKeys.Enable();
            moveAction = controls.Player2ArrowKeys.Movement;
            jumpAction = controls.Player2ArrowKeys.Jump;
        }

        moveAction.Enable();
        jumpAction.Enable();

        // Subscribe to movement
        moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled += ctx => moveInput = Vector2.zero;

        // Subscribe to jump
        jumpAction.started += ctx => { jumpToConsume = true; timeJumpWasPressed = time; jumpHeld = true; };
        jumpAction.canceled += ctx => jumpHeld = false;
    }

    void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
    }

    void Update()
    {
        time += Time.deltaTime;

        if (moveInput.y < -0.5f) // Check if the down direction is pressed
        {
            anchorInputHeld = true;
        }
        else
        {
            anchorInputHeld = false;
        }
    }

    void FixedUpdate()
    {
        frameVelocity = rb.linearVelocity;

        CheckCollisions();

        HandleAnchoring();
        if (isAnchored)
        {
            rb.linearVelocity = frameVelocity;
            return;
        }

        HandleJump();
        HandleHorizontal();
        HandleGravity();

        rb.linearVelocity = frameVelocity;
    }

    #region Collisions
    private void CheckCollisions()
    {
        bool groundHit = Physics2D.OverlapCapsule(
            groundCheck.position,
            new Vector2(1.8f, 0.3f),
            CapsuleDirection2D.Horizontal,
            0,
            groundLayer
        );

        Collider2D playerCollider = Physics2D.OverlapCapsule(
            groundCheck.position,
            new Vector2(1.8f, 0.3f),
            CapsuleDirection2D.Horizontal,
            0,
            LayerMask.GetMask("Player")
        );

        bool playerHit = playerCollider != null && playerCollider.gameObject != this.gameObject;

        bool isOnSomething = groundHit || playerHit;

        if (!grounded && isOnSomething)
        {
            grounded = true;
            coyoteUsable = true;
            bufferedJumpUsable = true;
            endedJumpEarly = false;
            airJumpUsable = true;
        }
        else if (grounded && !isOnSomething)
        {
            grounded = false;
            frameLeftGrounded = time;
        }
    }
    #endregion

    #region Jump
    private bool HasBufferedJump => bufferedJumpUsable && time < timeJumpWasPressed + jumpBuffer;
    private bool CanUseCoyote => coyoteUsable && !grounded && time < frameLeftGrounded + coyoteTime;

    private void HandleJump()
    {
        if (!endedJumpEarly && !grounded && frameVelocity.y > 0 && !jumpHeld)
            endedJumpEarly = true;

        if (!jumpToConsume && !HasBufferedJump) return;

        // NOTE: Assuming the "swinging" player is not anchored. 
        // This allow ANY player to air jump once, if they aren't anchored.
        // In final system, this should be tied to the rope being attached.

        bool canAirJump = !grounded && !isAnchored && airJumpUsable;

        if (grounded || CanUseCoyote || canAirJump)
        {
            if (canAirJump)
            {
                airJumpUsable = false;
            }
            ExecuteJump();
        }

        jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        endedJumpEarly = false;
        timeJumpWasPressed = 0;
        bufferedJumpUsable = false;
        coyoteUsable = false;

        // Track which player jumped and record the time
        if (actionMapName == "Player1WASD")
            JumpManager.lastPlayer1JumpTime = Time.time;
        else if (actionMapName == "Player2ArrowKeys")
            JumpManager.lastPlayer2JumpTime = Time.time;

        float finalJumpPower = jumpPower;

        // Check for synchronized jump
        if (JumpManager.IsSynchronized())
        {
            finalJumpPower *= 1.3f; // 30% boost when both jump together
        }

        // Check if this is a Swing Throw (air jump)
        if (!grounded && !isAnchored)
        {
            Vector2 currentVelocity = rb.linearVelocity;

            float throwX = finalJumpPower * 0.5f * (transform.localScale.x > 0 ? 1 : -1); // Boost forward
            float throwY = finalJumpPower * 0.8f; // Strong vertical boost

            frameVelocity.x += throwX;
            frameVelocity.y = Mathf.Max(frameVelocity.y, 0) + throwY;

            rb.linearVelocity = frameVelocity;
        }
        else
        {
            frameVelocity.y = finalJumpPower;
        }
    }
    #endregion

    #region Anchoring
    private void HandleAnchoring()
    {
        if (grounded && anchorInputHeld)
        {
            if (!isAnchored)
            {
                isAnchored = true;
                rb.isKinematic = true;
                rb.linearVelocity = Vector2.zero;
                frameVelocity = Vector2.zero;
            }
            rb.linearVelocity = Vector2.zero;
            frameVelocity = Vector2.zero;
        }
        else
        {
            if (isAnchored)
            {
                isAnchored = false;
                rb.isKinematic = false;
            }
        }
    }
    #endregion

    #region Horizontal
    private void HandleHorizontal()
    {
        if (isAnchored)
        {
            frameVelocity.x = 0;
            return;
        }
        if (moveInput.x == 0)
        {
            float decel = grounded ? groundDeceleration : airDeceleration;
            frameVelocity.x = Mathf.MoveTowards(rb.linearVelocity.x, 0, decel * Time.fixedDeltaTime);
        }
        else
        {
            frameVelocity.x = Mathf.MoveTowards(rb.linearVelocity.x, moveInput.x * maxSpeed, acceleration * Time.fixedDeltaTime);
        }

        flip();
    }
    #endregion

    #region Gravity
    private void HandleGravity()
    {
        if (grounded && frameVelocity.y <= 0f)
        {
            frameVelocity.y = -1.5f; // grounding force
        }
        else
        {
            float gravity = fallAcceleration;
            if (endedJumpEarly && frameVelocity.y > 0)
                gravity *= jumpEndEarlyGravityModifier;

            frameVelocity.y += -gravity * Time.fixedDeltaTime;

            if (frameVelocity.y < -maxFallSpeed)
                frameVelocity.y = -maxFallSpeed;
        }
    }
    #endregion

    private void flip()
    {
        if (moveInput.x < -0.01f) transform.localScale = new Vector3(-1, 1, 1);
        if (moveInput.x > 0.01f) transform.localScale = new Vector3(1, 1, 1);
    }
}
