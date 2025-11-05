using UnityEngine;
using UnityEngine.InputSystem;

using Vector2 = UnityEngine.Vector2;

public class MovementController : MonoBehaviour
{
    public static MovementController Instance;
    private bool canMove = true;

    [Header("Audio Clips")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] landFootsteps;
    [SerializeField] private AudioClip[] shootSound;
    [SerializeField] private AudioClip[] shieldStartSound;

    [Header("System")]
    private GameController gameController;

    [Header("References")]
    [SerializeField] public Rigidbody2D rb;
    [SerializeField] private CapsuleCollider2D col;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private PlayerAbilities playerAbilities;

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
    [SerializeField] private float grounderDistance = 0.05f;

    [Header("Input Setup")]
    [Tooltip("Choose which action map this player should use (Player1WASD / Player2ArrowKeys).")]
    [SerializeField] public string actionMapName = "Player1WASD";

    [Header("Co-op")]
    [SerializeField] private MovementController otherPlayerController;

    [Header("Swing")]
    [SerializeField] private float swingPumpForce = 30f;

    // --- SWIM SETTINGS ---
    [Header("Swimming")]
    [SerializeField] private float swimForce = 80f; // Force applied when moving in water
    [SerializeField] private float swimMaxSpeed = 7f; // Max speed in water

    private PlayerActions controls;
    private InputAction moveAction;
    private InputAction jumpAction;

    public PlayerActions Controls
    {
        get
        {
            if (controls == null)
            {
                controls = new PlayerActions();
            }
            return controls;
        }
    }

    private Vector2 moveInput;
    private Vector2 frameVelocity;
    private bool cachedQueryStartInColliders;

    // --- WATER STATE TRACKING ---
    private bool inWater = false;
    private float originalGravityScale;
    private float originalDrag;
    private float originalAngularDamping; // Renamed: originalAngularDrag -> originalAngularDamping
    // --------------------------------

    // state tracking
    private bool grounded;
    private bool endedJumpEarly;
    private bool jumpToConsume;
    private bool bufferedJumpUsable;
    private bool coyoteUsable;
    private bool jumpHeld;
    private bool anchorInputHeld;
    private bool isAnchored;
    private bool isSwinging;
    private bool airJumpUsable;
    private float frameLeftGrounded = float.MinValue;
    private float timeJumpWasPressed;
    private float time;
    private bool frameJumpExecuted;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Debug.LogWarning("make sure to hv one Movement Controller only!");

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

        // Store original gravity scale on Awake
        originalGravityScale = rb.gravityScale;

        gameController = FindFirstObjectByType<GameController>();
        if (gameController == null)
        {
            Debug.LogError("MovementController could not find a GameController in the scene!");
        }

        playerAbilities = GetComponent<PlayerAbilities>();
        if (playerAbilities == null)
        {
            Debug.LogWarning("MovementController: PlayerAbilities component not found. Shield check will fail.");
        }
    }

    public void SetMovementState(bool state)
    {
        canMove = state;

        if (!canMove)
        {
            moveInput = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            frameVelocity = Vector2.zero;

        }
        else
        {
            if (isAnchored) 
            {
                
            }
            else
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }
    }
    void OnEnable()
    {
        PlayerActions controlsInstance = Controls;

        if (actionMapName == "Player1WASD")
        {
            controls.Player1WASD.Enable();
            controls.Player2ArrowKeys.Disable();
            moveAction = controls.Player1WASD.Movement;
            jumpAction = controls.Player1WASD.Jump;
        }
        else if (actionMapName == "Player2ArrowKeys")
        {
            controls.Player2ArrowKeys.Enable();
            controls.Player1WASD.Disable();
            moveAction = controls.Player2ArrowKeys.Movement;
            jumpAction = controls.Player2ArrowKeys.Jump;
        }

        moveAction.Enable();
        jumpAction.Enable();

        moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled += ctx => moveInput = Vector2.zero;

        jumpAction.started += ctx => { jumpToConsume = true; timeJumpWasPressed = time; jumpHeld = true; };
        jumpAction.canceled += ctx => jumpHeld = false;
    }

    void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();

        if (actionMapName == "Player1WASD")
        {
            controls?.Player1WASD.Disable();
        }
        else if (actionMapName == "Player2ArrowKeys")
        {
            controls?.Player2ArrowKeys.Disable();
        }
    }
    void OnDestroy()
    {
        controls?.Dispose();
    }
    void Update()
    {
        time += Time.deltaTime;

        if (!canMove)
        {
            anchorInputHeld = false;
            return;
        }

        anchorInputHeld = moveInput.y < -0.5f;
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Use linearVelocity
        if (!inWater)
        {
            frameVelocity = rb.linearVelocity;
        }

        CheckCollisions();

        HandleAnchoring();

        if (isAnchored)
        {
            rb.linearVelocity = frameVelocity; 
            return;
        }

        if (inWater)
        {
            HandleSwimmingMovement();
        }
        else // Normal Land/Air Movement
        {
            HandleJump();
            HandleHorizontal();
            HandleGravity();

            // Use linearVelocity
            rb.linearVelocity = frameVelocity;
        }
    }

    #region Collisions
    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        // ... (Collision logic - Unchanged) ...
        bool groundHit = Physics2D.CapsuleCast(
            col.bounds.center, col.size, col.direction, 0, Vector2.down, grounderDistance, groundLayer
        );
        bool ceilingHit = Physics2D.CapsuleCast(
            col.bounds.center, col.size, col.direction, 0, Vector2.up, grounderDistance, groundLayer
        );

        if (ceilingHit && !inWater)
        {
            frameVelocity.y = Mathf.Min(0, frameVelocity.y);
        }

        if (!grounded && groundHit)
        {
            grounded = true;
            coyoteUsable = true;
            bufferedJumpUsable = true;
            endedJumpEarly = false;
            airJumpUsable = true;

            if (audioSource != null && landFootsteps.Length > 0)
            {
                AudioClip clip = landFootsteps[Random.Range(0, landFootsteps.Length)];
                audioSource.PlayOneShot(clip);
            }
        }
        else if (grounded && !groundHit)
        {
            grounded = false;
            frameLeftGrounded = time;
        }

        Physics2D.queriesStartInColliders = cachedQueryStartInColliders;
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

        bool canAirJump = !grounded && !isAnchored && (airJumpUsable || isSwinging);

        if (grounded || CanUseCoyote || canAirJump)
        {
            ExecuteJump(canAirJump && !isSwinging);
        }

        jumpToConsume = false;
    }

    private void ExecuteJump(bool consumeAirJump = false)
    {
        endedJumpEarly = false;
        timeJumpWasPressed = 0;
        bufferedJumpUsable = false;
        coyoteUsable = false;
        frameJumpExecuted = true;

        if (actionMapName == "Player1WASD") JumpManager.lastPlayer1JumpTime = Time.time;
        else if (actionMapName == "Player2ArrowKeys") JumpManager.lastPlayer2JumpTime = Time.time;

        float finalJumpPower = jumpPower;
        if (JumpManager.IsSynchronized()) finalJumpPower *= 1.2f;

        // Swing Throw logic uses linearVelocity (fixed)
        if (!grounded && !isAnchored && (airJumpUsable || isSwinging))
        {
            float horizontalMomentumTransfer = Mathf.Abs(rb.linearVelocity.x) * 1.0f; // Use linearVelocity
            float verticalBoost = finalJumpPower * 1.3f;
            float throwX = rb.linearVelocity.x + (rb.linearVelocity.x > 0 ? horizontalMomentumTransfer : -horizontalMomentumTransfer); // Use linearVelocity

            frameVelocity.x = throwX;
            frameVelocity.y = Mathf.Max(frameVelocity.y, 0) + verticalBoost;

            if (consumeAirJump) airJumpUsable = false;
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
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero; // Use linearVelocity
                frameVelocity = Vector2.zero;
            }
            rb.linearVelocity = Vector2.zero; // Use linearVelocity
            frameVelocity = Vector2.zero;
        }
        else
        {
            if (isAnchored)
            {
                isAnchored = false;
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }

        if (otherPlayerController != null)
        {
            isSwinging = !isAnchored && otherPlayerController.isAnchored;
        }
        else
        {
            isSwinging = false;
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

        // ... (Existing horizontal movement logic - Unchanged) ...
        if (isSwinging && !grounded)
        {
            if (moveInput.x != 0)
            {
                frameVelocity.x = Mathf.MoveTowards(
                    frameVelocity.x,
                    moveInput.x * maxSpeed,
                    swingPumpForce * Time.fixedDeltaTime
                );
            }
            else
            {
                float decel = airDeceleration * 0.5f;
                frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, 0, decel * Time.fixedDeltaTime);
            }
        }
        else
        {
            if (moveInput.x == 0)
            {
                float decel = grounded ? groundDeceleration : airDeceleration;
                frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, 0, decel * Time.fixedDeltaTime);
            }
            else
            {
                frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, moveInput.x * maxSpeed, acceleration * Time.fixedDeltaTime);
            }
        }

        Flip();
    }
    #endregion

    #region Gravity
    private void HandleGravity()
    {
        if (grounded && frameVelocity.y <= 0f)
        {
            frameVelocity.y = -1.5f;
        }
        else
        {
            float gravity = fallAcceleration;
            if (endedJumpEarly && frameVelocity.y > 0)
                gravity *= jumpEndEarlyGravityModifier;

            frameVelocity.y = Mathf.MoveTowards(frameVelocity.y, -maxFallSpeed, gravity * Time.fixedDeltaTime);
        }
    }
    #endregion

    // --- WATER MOVEMENT LOGIC ---

    private void HandleSwimmingMovement()
    {
        Vector2 totalSwimForce = Vector2.zero;

        // 1. Calculate force from standard movement input
        if (moveInput.magnitude > 0.01f)
        {
            totalSwimForce += moveInput.normalized * swimForce;
        }

        // 2. Add extra upward force if the jump button is held
        if (jumpHeld)
        {
            totalSwimForce += Vector2.up * (swimForce * 0.5f);
        }

        rb.AddForce(totalSwimForce, ForceMode2D.Force);

        if (rb.linearVelocity.magnitude > swimMaxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * swimMaxSpeed;
        }

        Flip();
    }

    // --- WATER STATE METHOD ---
    public void SetInWater(bool state, float waterDrag, float waterAngularDamping)
    {
        if (inWater == state) return;

        inWater = state;

        if (inWater)
        {
            // Store original drag and damping values
            originalDrag = rb.linearDamping;
            originalAngularDamping = rb.angularDamping;

            // Set water drag and damping
            rb.linearDamping = waterDrag;
            rb.angularDamping = waterAngularDamping;

            rb.gravityScale = 0f;

            // This prevents the player from maintaining high sinking speed.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
        else
        {
            // Restore original drag and damping values
            rb.linearDamping = originalDrag;
            rb.angularDamping = originalAngularDamping;

            rb.gravityScale = originalGravityScale;
        }
    }

    private void Flip()
    {
        if (moveInput.x < -0.01f) transform.localScale = new Vector3(-1, 1, 1);
        if (moveInput.x > 0.01f) transform.localScale = new Vector3(1, 1, 1);
    }

    // Public getters for other systems
    public bool IsAnchored => isAnchored;
    public bool IsGrounded => grounded;
    public bool IsSwinging => isSwinging;

    public bool IsJumpExecuted()
    {
        bool result = frameJumpExecuted;
        frameJumpExecuted = false;
        return result;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle"))
        {
            gameController.Die(this);
        }
    }

    public void PlayShootSound()
    {
        if (audioSource != null && shootSound != null)
        {
            AudioClip clip = shootSound[Random.Range(0, shootSound.Length)];
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayShieldStartSound()
    {
        if (audioSource != null && shieldStartSound != null)
        {
            AudioClip clip = shieldStartSound[Random.Range(0, shieldStartSound.Length)];
            audioSource.PlayOneShot(clip);
        }
    }
}