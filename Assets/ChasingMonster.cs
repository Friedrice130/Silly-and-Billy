using System.Collections;
using UnityEngine;

public class ChasingMonster : MonoBehaviour
{
    [Header("Player Target")]
    public Transform player;
    public float moveSpeed = 3f;
    public float jumpForce = 8f;
    public float jumpCooldown = 0.5f;
    public LayerMask groundLayer;

    [Header("Ground Check Settings")]
    public Transform groundCheck;          // Empty object below monster
    public float groundCheckRadius = 0.2f; // Radius for checking ground

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool isJumping = false;
    private bool canJump = true;
    private bool isChasing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Create ground check dynamically if missing
        if (groundCheck == null)
        {
            GameObject checker = new GameObject("GroundCheck");
            checker.transform.parent = transform;
            checker.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = checker.transform;
        }
    }

    void Update()
    {
        CheckGrounded();

        if (player != null)
        {
            if (!isChasing)
            {
                isChasing = true;
                Debug.Log("Monster started chasing the player!");
            }

            float distX = player.position.x - transform.position.x;
            float distY = player.position.y - transform.position.y;

            // Move horizontally toward the player
            rb.linearVelocity = new Vector2(Mathf.Sign(distX) * moveSpeed, rb.linearVelocity.y);

            // Flip sprite to face the player
            if (distX != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(distX);
                transform.localScale = scale;
            }

            // Jump if player is higher and grounded
            if (distY > 1.5f && isGrounded && canJump)
            {
                StartCoroutine(JumpToPlayer());
            }
        }
        else
        {
            // Stop chasing when no player found
            if (isChasing)
            {
                Debug.Log("Monster lost the player, stopping chase.");
                isChasing = false;
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
    }

    private void CheckGrounded()
    {
        // Circle check below monster
        Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isGrounded = (hit != null);
    }

    private IEnumerator JumpToPlayer()
    {
        canJump = false;
        isJumping = true;

        Vector2 dir = (player.position - transform.position).normalized;
        if (Mathf.Abs(dir.x) < 0.2f)
            dir.x = transform.localScale.x;

        // Reset vertical velocity before jumping
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(new Vector2(dir.x * moveSpeed * 0.8f, jumpForce), ForceMode2D.Impulse);

        yield return new WaitUntil(() => isGrounded);
        yield return new WaitForSeconds(jumpCooldown);

        isJumping = false;
        canJump = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Monster hit the player! Triggering spikes!");
            TriggerAllSpikes();
        }
    }

    private void TriggerAllSpikes()
    {
        GameObject[] spikes = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject spike in spikes)
        {
            monsterfallspike spikeScript = spike.GetComponent<monsterfallspike>();
            if (spikeScript != null)
            {
                spikeScript.TriggerDrop();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
