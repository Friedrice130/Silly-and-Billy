using System.Collections;
using UnityEngine;

public class ChasingMonster : MonoBehaviour
{
    [Header("Player Target")]
    public Transform player;
    public float moveSpeed = 3f;
    public float jumpForce = 8f;
    public float jumpCooldown = 1f;

    [Header("Feet Detector")]
    public MonsterFeet feet;

    private Rigidbody2D rb;
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
        if (player == null || feet == null) return;

        bool isGrounded = feet.isGrounded;
        float distX = player.position.x - transform.position.x;
        float distY = player.position.y - transform.position.y;

        // Start chasing
        if (!isChasing)
        {
            isChasing = true;
            Debug.Log("Monster started chasing the player!");
        }

        // Move horizontally
        rb.linearVelocity = new Vector2(Mathf.Sign(distX) * moveSpeed, rb.linearVelocity.y);

        // Face player
        if (distX != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(distX);
            transform.localScale = scale;
        }

        // Only jump if:
        // - player is higher
        // - monster is on the ground
        // - not already in cooldown
        if (distY > 1.5f && isGrounded && canJump)
        {
            StartCoroutine(JumpToPlayer());
        }
    }

    private IEnumerator JumpToPlayer()
    {
        canJump = false;

        // Add vertical force
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Wait until grounded again
        yield return new WaitUntil(() => feet.isGrounded);

        // Wait small delay before next jump
        yield return new WaitForSeconds(jumpCooldown);

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
}
