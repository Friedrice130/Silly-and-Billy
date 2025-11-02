using UnityEngine;
using System.Collections;

public class ChasingMonster : MonoBehaviour
{
    [Header("Player Target")]
    public Transform player;
    public float moveSpeed = 3f;
    public float jumpForce = 8f;
    public float jumpCooldown = 0.5f;
    public LayerMask groundLayer;

    [Header("Feet Detector")]
    public MonsterFeet feet;

    private Rigidbody2D rb;
    private bool isJumping = false;
    private bool canJump = true;
    private bool isChasing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (feet == null)
            feet = GetComponentInChildren<MonsterFeet>();
    }

    void Update()
    {
        if (!feet) return;

        bool isGrounded = feet.isGrounded;

        // ? If the monster has a target (player), chase it
        if (player != null)
        {
            if (!isChasing)
            {
                isChasing = true;
                Debug.Log(" Monster started chasing the player!");
            }

            float distX = player.position.x - transform.position.x;
            float distY = player.position.y - transform.position.y;

            // Move horizontally toward the player
            rb.linearVelocity = new Vector2(Mathf.Sign(distX) * moveSpeed, rb.linearVelocity.y);

            // Flip to face the player
            if (distX != 0)
                transform.localScale = new Vector3(Mathf.Sign(distX), 1, 1);

            // Jump if player is higher and grounded
            if (distY > 1.5f && isGrounded && canJump)
            {
                StartCoroutine(JumpToPlayer());
            }

            Debug.Log($"Chasing: {isChasing}, Grounded: {isGrounded}, Jumping: {isJumping}, DistY: {distY:F2}");
        }
        else
        {
            // Stop chasing when player leaves detection zone
            if (isChasing)
            {
                Debug.Log(" Monster lost the player, stopping chase.");
                isChasing = false;
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
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

        yield return new WaitUntil(() => feet.isGrounded);
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
        GameObject[] spikes = GameObject.FindGameObjectsWithTag("spike");
        foreach (GameObject spike in spikes)
        {
            monsterfallspike spikeScript = spike.GetComponent<monsterfallspike>();
            if (spikeScript != null)
            {
                spikeScript.TriggerDrop();
            }
        }
    }
}
