using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class NewFinalBoss : MonoBehaviour
{
    // --- CONSTANTS & STATS ---
    [Header("Stats")]
    public int health = 5;
    public const int DAMAGE = 999;
    public bool isDead = false;

    [Header("Audio Control")]
    public AudioClip attackSoundClip;
    private AudioSource audioSource;

    [Header("Final Boss UI")]
    [SerializeField] private GameObject healthBarUI;

    // --- STATIONARY BOSS ABILITIES & RANGES ---
    [Header("Wave Attack Settings")]
    public float sightRange = 15f;
    public float projectileSpawnOffset = 1.0f;

    // Projectile Attack
    public GameObject projectilePrefab;
    public float waveCooldown = 5.0f;
    private float lastWaveTime = 0f;

    // Wave Control
    public int waveProjectileCount = 7;
    public float waveSpreadAngle = 90f;

    // Projectile Size/Speed Control 
    public float minWaveScale = 0.5f;
    public float maxWaveScale = 1.5f;
    public float waveSpeed = 12f;

    [Header("Door Control")]
    [SerializeField] private BossDoorController exitDoorController;

    // --- REFERENCES ---
    [Header("References")]
    public Animator camAnim;
    public Slider healthBar;
    private Animator anim;
    private Rigidbody2D rb;
    private GameController gameController;

    // Added SpriteRenderer reference
    private SpriteRenderer spriteRenderer;

    // --- STATE & TARGETING ---
    private enum BossState { Idle, Run, Attack }
    private BossState currentState = BossState.Idle;
    private Transform targetPlayer;
    private List<Transform> allPlayers = new List<Transform>();
    private Vector3 startPosition;

    private const string BULLET_COMPONENT_NAME = "Bullet";


    // ## START & SETUP
    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        audioSource = GetComponent<AudioSource>();

        spriteRenderer = GetComponent<SpriteRenderer>();
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

        // Update the health bar value every frame
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

        // The FlipSprite method is updated below to use SpriteRenderer.flipX
        FlipSprite(targetPlayer.position);

        switch (currentState)
        {
            case BossState.Idle:
                if (Time.time >= lastWaveTime + waveCooldown)
                {
                    StartCoroutine(WaveAttack());
                }
                break;

            case BossState.Run:
                SetState(BossState.Idle);
                break;

            case BossState.Attack:
                break;
        }
    }

    private void SetState(BossState newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        anim.SetBool("Idle", newState == BossState.Idle);
        anim.SetBool("Run", newState == BossState.Run);
        anim.SetBool("Attack", newState == BossState.Attack);
    }

    // ## UTILITY FOR PROJECTILES

    public bool IsAnyPlayerShielding()
    {
        allPlayers.RemoveAll(item => item == null);

        foreach (Transform playerTransform in allPlayers)
        {
            if (playerTransform == null) continue;

            PlayerAbilities abilities = playerTransform.GetComponent<PlayerAbilities>()
                ?? playerTransform.GetComponentInParent<PlayerAbilities>();

            if (abilities != null && abilities.IsShielding)
            {
                return true;
            }
        }
        return false;
    }


    // ## ATTACK SEQUENCE (Wave Projectiles)
    private IEnumerator WaveAttack()
    {
        if (isDead || targetPlayer == null)
        {
            SetState(BossState.Idle);
            yield break;
        }

        SetState(BossState.Attack);
        lastWaveTime = Time.time;

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        if (audioSource != null && attackSoundClip != null)
        {
            audioSource.PlayOneShot(attackSoundClip);
        }

        yield return new WaitForSeconds(0.5f);

        Vector3 spawnPosition = transform.position;
        float facingDirectionX = transform.localScale.x > 0 ? -1 : 1;
        spawnPosition += new Vector3(facingDirectionX * projectileSpawnOffset, 0, 0);

        Vector2 directionToTarget = (targetPlayer.position - spawnPosition).normalized;
        float baseAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

        float startAngle = baseAngle - (waveSpreadAngle / 2f);
        float angleStep = waveProjectileCount > 1 ? waveSpreadAngle / (waveProjectileCount - 1) : 0;

        for (int i = 0; i < waveProjectileCount; i++)
        {
            if (isDead) break;

            float currentAngle = startAngle + (i * angleStep);
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);

            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, rotation);

            float currentScale = Mathf.Lerp(minWaveScale, maxWaveScale, (float)i / (waveProjectileCount - 1));
            projectile.transform.localScale = new Vector3(currentScale, currentScale, currentScale);

            WaveProjectile wave = projectile.GetComponent<WaveProjectile>();
            Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();

            if (projRb != null && wave != null)
            {
                wave.speed = waveSpeed;
                projRb.linearVelocity = rotation * Vector2.right * waveSpeed;
            }

            Collider2D projectileCollider = projectile.GetComponent<Collider2D>();
            Collider2D bossCollider = GetComponent<Collider2D>();

            if (projectileCollider != null && bossCollider != null)
            {
                Physics2D.IgnoreCollision(projectileCollider, bossCollider, true);
            }
        }

        yield return new WaitForSeconds(0.5f);

        SetState(BossState.Idle);
    }

    // Using SpriteRenderer.flipX
    private void FlipSprite(Vector3 targetPos)
    {
        bool targetIsToTheRight = targetPos.x > transform.position.x;
 
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = targetIsToTheRight;
        }

        Vector3 currentScale = transform.localScale;

        float targetScaleX = targetIsToTheRight ? -Mathf.Abs(currentScale.x) : Mathf.Abs(currentScale.x);

        if (currentScale.x != targetScaleX)
        {
            currentScale.x = targetScaleX;
            transform.localScale = currentScale;
        }

        if (spriteRenderer == null && currentScale.x != targetScaleX)
        {
            Debug.LogWarning("SpriteRenderer not found! Using transform scale for visual flip, which may cause scaling issues.");
        }
    }

    // ## TRIGGER LOGIC (COMBINED)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ToggleHealthBar(true);
        }

        if (collision.TryGetComponent<Bullet>(out Bullet playerBullet))
        {
            int damageAmount = playerBullet.damage > 0 ? playerBullet.damage : 1;

            TakeDamage(damageAmount);
            Destroy(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

        if (collision.CompareTag("Player") && !isDead)
        {
            ToggleHealthBar(false);
        }
    }


    // ## HEALTH AND DAMAGE LOGIC 

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

    private void Die()
    {
        isDead = true;
        StopAllCoroutines();
        SetState(BossState.Idle);

        if (exitDoorController != null)
        {
            exitDoorController.OpenDoor();
        }
        else
        {
            Debug.LogWarning("Boss Door Controller not assigned. Exit path may remain blocked.");
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

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