using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // Added for List manipulation

public class Boss : MonoBehaviour
{
    // --- CONSTANTS & STATS ---
    [Header("Stats")]
    public int health;
    public const int DAMAGE = 999;
    public float attackCooldown = 1.5f;
    public bool isDead = false;

    // --- AI RANGES (IMPROVED) ---
    [Header("AI Ranges")]
    public float sightRange = 10f;
    public float attackRange = 1.5f; // ADJUST THIS IN INSPECTOR for visual attack range!
    public float moveSpeed = 3f;
    public float disengageRange = 15f;
    public float reEngageRange = 8f; // Distance player must be within to stop a Reset
    private const float ARRIVAL_THRESHOLD = 0.1f;

    // --- REFERENCES ---
    [Header("References")]
    public Animator camAnim;
    public Slider healthBar;
    private Animator anim;
    private Rigidbody2D rb;
    private GameController gameController;

    // --- STATE & TARGETING ---
    private enum BossState { Idle, Run, Attack, Reset }
    private BossState currentState = BossState.Idle;
    private Transform targetPlayer; // Always holds the NEAREST valid player
    private List<Transform> allPlayers = new List<Transform>();
    private Vector3 startPosition;
    private float lastAttackTime = 0f;

    // ------------------------------------
    // ## START & SETUP
    // ------------------------------------
    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        gameController = FindFirstObjectByType<GameController>();

        // Set Rigidbody2D for non-pushable movement
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        else
        {
            Debug.LogError("Boss: Rigidbody2D component missing! Boss will be pushable.");
        }

        startPosition = transform.position;
        FindAllPlayers();

        if (gameController == null) Debug.LogError("Boss: GameController not found in scene!");

        if (reEngageRange >= sightRange)
        {
            Debug.LogWarning("Boss: reEngageRange should be smaller than sightRange. Adjusting to sightRange - 1.");
            reEngageRange = sightRange - 1f;
        }
    }

    // --- UTILITY SETUP ---
    private void FindAllPlayers()
    {
        allPlayers.Clear();
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        if (playerObjects.Length > 0)
        {
            foreach (GameObject obj in playerObjects) allPlayers.Add(obj.transform);
        }
        else
        {
            Debug.LogWarning("Boss: Could not find any active objects tagged 'Player'.");
        }
    }

    // ------------------------------------
    // ## UPDATE & TARGETING
    // ------------------------------------
    private void Update()
    {
        if (isDead) return;
        if (healthBar != null) healthBar.value = health;

        HandleStates();
    }

    /// <summary>
    /// Finds the nearest player, cleans up the player list, and returns the distance.
    /// </summary>
    private float FindNearestTarget()
    {
        float nearestDistance = float.MaxValue;
        Transform currentNearest = null;

        // CRITICAL FIX: Remove destroyed player references from the list
        allPlayers.RemoveAll(item => item == null);

        // If the list is empty, try to refresh it (in case players were recently respawned/created)
        if (allPlayers.Count == 0)
        {
            FindAllPlayers();
            if (allPlayers.Count == 0)
            {
                targetPlayer = null;
                return float.MaxValue;
            }
        }

        foreach (Transform player in allPlayers)
        {
            // Safety check, although RemoveAll should handle most cases
            if (player == null) continue;

            float distance = Vector2.Distance(transform.position, player.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                currentNearest = player;
            }
        }

        targetPlayer = currentNearest;

        return nearestDistance;
    }

    // ------------------------------------
    // ## AI STATE MACHINE (IMPROVED DISENGAGE LOGIC)
    // ------------------------------------
    private void HandleStates()
    {
        float distanceToNearestPlayer = FindNearestTarget();

        // 1. Handle Reset State
        if (currentState == BossState.Reset)
        {
            anim.SetBool("Run", true);
            FlipSprite(startPosition);
            RunToTarget(startPosition);

            // CONDITION A: Boss has arrived at the start position (Default Reset)
            if (Vector2.Distance(transform.position, startPosition) < ARRIVAL_THRESHOLD)
            {
                SetState(BossState.Idle);
            }
            // CONDITION B: Player interrupts the reset by moving very close again
            else if (distanceToNearestPlayer <= reEngageRange)
            {
                SetState(BossState.Run);
            }
            return;
        }

        // 2. Handle Combat States
        if (targetPlayer == null)
        {
            SetState(BossState.Idle); // No valid target, go idle
            return;
        }

        FlipSprite(targetPlayer.position);

        switch (currentState)
        {
            case BossState.Idle:
                anim.SetBool("Run", false);
                if (distanceToNearestPlayer <= sightRange)
                {
                    SetState(BossState.Run);
                }
                break;

            case BossState.Run:
                anim.SetBool("Run", true);

                // DISENGAGE CHECK: Player is too far away
                if (distanceToNearestPlayer > disengageRange)
                {
                    SetState(BossState.Reset);
                    break;
                }

                // Attack Condition
                if (distanceToNearestPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
                {
                    anim.SetBool("Run", false);
                    SetState(BossState.Attack);
                }
                else
                {
                    // Chase the nearest player
                    RunToTarget(targetPlayer.position);
                }
                break;

            case BossState.Attack:
                break;
        }
    }

    private void SetState(BossState newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        StopAllCoroutines();

        if (newState == BossState.Attack) StartCoroutine(AttackPlayer());
        anim.SetBool("Run", newState == BossState.Run || newState == BossState.Reset);
        if (newState == BossState.Idle) anim.SetBool("Run", false);
    }

    // ------------------------------------
    // ## MOVEMENT & FLIP LOGIC
    // ------------------------------------
    private void RunToTarget(Vector3 targetPos)
    {
        Vector2 newPosition = Vector2.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        if (rb != null)
        {
            rb.MovePosition(newPosition);
        }
        else
        {
            transform.position = newPosition;
        }
    }

    private void FlipSprite(Vector3 targetPos)
    {
        Vector3 currentScale = transform.localScale;
        bool targetIsToTheRight = targetPos.x > transform.position.x;

        // Flips the sprite based on target position
        float targetScaleX = targetIsToTheRight ? -Mathf.Abs(currentScale.x) : Mathf.Abs(currentScale.x);

        if (currentScale.x != targetScaleX)
        {
            currentScale.x = targetScaleX;
            transform.localScale = currentScale;
        }
    }

    // ------------------------------------
    // ## ATTACK SEQUENCE (CO-OP SHIELD)
    // ------------------------------------
    private IEnumerator AttackPlayer()
    {
        // 0. Initial Wind-up
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f);
        if (camAnim != null) camAnim.SetTrigger("shake");

        // Safety check immediately before damage is applied
        if (targetPlayer == null)
        {
            SetState(BossState.Run);
            yield break;
        }

        MovementController targetedPlayerController = targetPlayer.GetComponent<MovementController>()
            ?? targetPlayer.GetComponentInParent<MovementController>();

        if (targetedPlayerController == null)
        {
            SetState(BossState.Run);
            yield break;
        }


        // 1. CHECK IF *ANY* PLAYER IS SHIELDING
        bool attackBlocked = false;
        foreach (Transform playerTransform in allPlayers)
        {
            if (playerTransform == null) continue;

            PlayerAbilities abilities = playerTransform.GetComponent<PlayerAbilities>()
                ?? playerTransform.GetComponentInParent<PlayerAbilities>();

            if (abilities != null && abilities.IsShielding)
            {
                attackBlocked = true;
                break;
            }
        }

        // 2. APPLY DAMAGE OR BLOCK EFFECT
        if (!attackBlocked)
        {
            if (gameController != null)
            {
                gameController.Die(targetedPlayerController);
            }
        }

        // 3. Finish and reset
        yield return new WaitForSeconds(attackCooldown);
        lastAttackTime = Time.time;
        SetState(BossState.Run);
    }

    // ------------------------------------
    // ## HEALTH AND DAMAGE LOGIC
    // ------------------------------------

    /// <summary>
    /// Reduces boss health and checks for death condition.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (isDead) return;
        health -= amount;

        if (healthBar != null)
        {
            healthBar.value = health;
        }

        if (health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Handles the Boss death sequence.
    /// </summary>
    private void Die()
    {
        isDead = true;

        // 1. Stop all AI logic and movement
        StopAllCoroutines();
        SetState(BossState.Idle);

        // 2. Disable collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 3. Destroy after animation time
        float deathAnimationTime = 2.0f;
        Destroy(gameObject, deathAnimationTime);
    }

    /// <summary>
    /// Forces the boss to stop chasing and return to the start position.
    /// </summary>
    public void ResetBossState()
    {
        StopAllCoroutines();
        SetState(BossState.Reset);
    }
}