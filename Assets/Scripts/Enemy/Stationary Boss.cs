using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class StationaryBoss : MonoBehaviour
{
    // --- CONSTANTS & STATS ---
    [Header("Stats")]
    public int health = 5;
    public const int DAMAGE = 999;
    public bool isDead = false;

    [Header("Second Boss UI")]
    [SerializeField] private GameObject healthBarUI;

    // --- STATIONARY BOSS ABILITIES & RANGES ---
    [Header("Snowball Attack Settings")]
    public float sightRange = 15f;
    public float projectileSpawnOffset = 1.0f;

    // Projectile Attack
    public GameObject projectilePrefab;
    public float projectileCooldown = 5.0f;
    private float lastProjectileTime = 0f;

    // Burst Control
    public int burstCount = 5;
    public float interShotDelay = 0.15f;

    // Projectile Size/Speed Control
    public float minSnowballScale = 0.5f;
    public float maxSnowballScale = 1.5f;
    public float minSnowballSpeed = 8f;
    public float maxSnowballSpeed = 15f;

    // --- REFERENCES ---
    [Header("References")]
    public Animator camAnim;
    public Slider healthBar;
    private Animator anim;
    private Rigidbody2D rb;
    private GameController gameController;

    // --- STATE & TARGETING ---
    private enum BossState { Idle, Attack }
    private BossState currentState = BossState.Idle;
    private Transform targetPlayer;
    private List<Transform> allPlayers = new List<Transform>();
    private Vector3 startPosition;

    // Added a constant for the bullet component as a guardrail.
    private const string BULLET_COMPONENT_NAME = "Bullet";



    // ## START & SETUP
    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        gameController = FindFirstObjectByType<GameController>();

        if (healthBarUI != null)
            healthBarUI.SetActive(false);

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = 1000;
        }

        startPosition = transform.position;
        FindAllPlayers();

        // Initialize Health Bar MaxValue and current Value
        if (healthBar != null)
        {
            healthBar.maxValue = health;
            healthBar.value = health;
        }
    }

    private void FindAllPlayers()
    {
        allPlayers.Clear();
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        if (playerObjects.Length > 0)
        {
            foreach (GameObject obj in playerObjects) allPlayers.Add(obj.transform);
        }
    }


    // ## UPDATE & TARGETING
    private void Update()
    {
        if (isDead) return;

        // Update the health bar value every frame (similar to FinalBoss)
        if (healthBar != null) healthBar.value = health;

        HandleStates();
    }

    private float FindNearestTarget()
    {
        float nearestDistance = float.MaxValue;
        Transform currentNearest = null;

        allPlayers.RemoveAll(item => item == null);

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

    // ## AI STATE MACHINE

    private void HandleStates()
    {
        float distanceToNearestPlayer = FindNearestTarget();

        if (targetPlayer == null || distanceToNearestPlayer > sightRange)
        {
            SetState(BossState.Idle);
            return;
        }

        FlipSprite(targetPlayer.position);

        switch (currentState)
        {
            case BossState.Idle:
                if (Time.time >= lastProjectileTime + projectileCooldown)
                {
                    StartCoroutine(ProjectileAttack());
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

        if (newState == BossState.Idle)
        {
            anim.SetBool("Attack", false);
        }
    }

    // ## UTILITY FOR PROJECTILES (NEW CO-OP SHIELD CHECK)

    public bool IsAnyPlayerShielding()
    {
        // Clean up the list just in case
        allPlayers.RemoveAll(item => item == null);

        foreach (Transform playerTransform in allPlayers)
        {
            if (playerTransform == null) continue;

            // Try to get the PlayerAbilities component from the player object or its parent
            PlayerAbilities abilities = playerTransform.GetComponent<PlayerAbilities>()
                ?? playerTransform.GetComponentInParent<PlayerAbilities>();

            if (abilities != null && abilities.IsShielding)
            {
                return true; // Attack is blocked if ANY player is shielding
            }
        }
        return false; // No players are shielding
    }


    // ## ATTACK SEQUENCE (Snowball Projectiles)
    private IEnumerator ProjectileAttack()
    {
        SetState(BossState.Attack);
        lastProjectileTime = Time.time;

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < burstCount; i++)
        {
            if (targetPlayer == null || isDead) break;

            // Calculate progressive size and speed
            float normalizedProgress = (float)i / (burstCount > 1 ? burstCount - 1 : 1);
            float currentScale = Mathf.Lerp(minSnowballScale, maxSnowballScale, normalizedProgress);
            float scaledSpeed = Mathf.Lerp(maxSnowballSpeed, minSnowballSpeed, normalizedProgress);

            // Calculate Spawn Position (Based on target direction)
            bool targetIsToTheRight = targetPlayer.position.x > transform.position.x;
            float spawnXOffset = targetIsToTheRight ? projectileSpawnOffset : -projectileSpawnOffset;
            Vector3 spawnPosition = transform.position + new Vector3(spawnXOffset, 0, 0);

            // Targeting and Instantiation
            float angleOffset = (i - (burstCount / 2)) * 8f;
            Vector2 directionToTarget = (targetPlayer.position - spawnPosition).normalized; // Aim from spawn point
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle + angleOffset);

            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, rotation);

            projectile.transform.localScale = new Vector3(currentScale, currentScale, currentScale);

            SnowballBullet snowball = projectile.GetComponent<SnowballBullet>();
            Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();

            if (projRb != null && snowball != null)
            {
                snowball.speed = scaledSpeed;
                // Note: Using velocity is better for instant movement than linearVelocity in some Unity versions/setups
                projRb.linearVelocity = rotation * Vector2.right * scaledSpeed;
            }

            Collider2D projectileCollider = projectile.GetComponent<Collider2D>();
            Collider2D bossCollider = GetComponent<Collider2D>();

            if (projectileCollider != null && bossCollider != null)
            {
                // Tell Unity to ignore collisions between the boss and the new snowball
                Physics2D.IgnoreCollision(projectileCollider, bossCollider, true);
            }

            yield return new WaitForSeconds(interShotDelay);
        }

        SetState(BossState.Idle);
    }

    // ## MOVEMENT & FLIP LOGIC
    private void FlipSprite(Vector3 targetPos)
    {
        Vector3 currentScale = transform.localScale;
        bool targetIsToTheRight = targetPos.x > transform.position.x;
        float targetScaleX = targetIsToTheRight ? -Mathf.Abs(currentScale.x) : Mathf.Abs(currentScale.x);

        if (currentScale.x != targetScaleX)
        {
            currentScale.x = targetScaleX;
            transform.localScale = currentScale;
        }
    }

    // ## HEALTH AND DAMAGE LOGIC


    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Use TryGetComponent for safer and more performant component retrieval
        if (collision.TryGetComponent<Bullet>(out Bullet playerBullet))
        {
            // Use the damage value from the bullet, defaulting to 1 if not set
            int damageAmount = playerBullet.damage > 0 ? playerBullet.damage : 1;

            TakeDamage(damageAmount);
            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        health -= amount;

        // Health bar value update 
        if (healthBar != null)
        {
            healthBar.value = health;
        }

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        StopAllCoroutines();
        SetState(BossState.Idle);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Disable health bar on death
        if (healthBar != null)
        {
            StartCoroutine(DisableHealthBarAfterDelay(1f, healthBar.gameObject));
        }

        ToggleHealthBar(false);

        float deathAnimationTime = 2.0f;
        Destroy(gameObject, deathAnimationTime);
    }

    private IEnumerator DisableHealthBarAfterDelay(float delay, GameObject healthBarObject)
    {
        yield return new WaitForSeconds(delay);

        if (healthBarObject != null)
        {
            healthBarObject.SetActive(false);
        }
    }


    public void ResetBossState()
    {
        StopAllCoroutines();
        SetState(BossState.Idle);
    }

    public void ToggleHealthBar(bool show)
    {
        if (healthBarUI != null)
            healthBarUI.SetActive(show);
    }
}