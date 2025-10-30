using UnityEngine;
using System.Collections;

public class ChasingMonster : MonoBehaviour
{
    [Header("????")]
    public Transform player;
    public float moveSpeed = 3f;
    public float jumpForce = 10f;
    public float groundCheckDistance = 2f;
    public LayerMask platformLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isJumping;
    private int monsterLayer;
    private int platformLayerIndex;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        monsterLayer = LayerMask.NameToLayer("Monster");
        platformLayerIndex = LayerMask.NameToLayer("Platform");
        Debug.Log("?? Monster started!");
    }

    void Update()
    {
        if (!player) return;

        // ????????????
        isGrounded = CheckGrounded();

        float distanceX = player.position.x - transform.position.x;
        float distanceY = player.position.y - transform.position.y;

        // ????
        if (distanceX != 0)
            transform.localScale = new Vector3(Mathf.Sign(distanceX), 1, 1);

        // ? ??????
        rb.linearVelocity = new Vector2(Mathf.Sign(distanceX) * moveSpeed, rb.linearVelocity.y);

        // ? ?????? ? ???
        if (distanceY > 1.5f && isGrounded && !isJumping)
        {
            StartCoroutine(JumpToPlayer());
        }

        // ? ???????
        if (isGrounded && isJumping)
        {
            Physics2D.IgnoreLayerCollision(monsterLayer, platformLayerIndex, false);
            isJumping = false;
        }

        // ?? ??????????0??????????
        if (isJumping && Mathf.Abs(rb.linearVelocity.y) < 0.05f)
        {
            Physics2D.IgnoreLayerCollision(monsterLayer, platformLayerIndex, false);
        }

        Debug.Log($"Grounded: {isGrounded}, DistX: {distanceX:F2}, DistY: {distanceY:F2}, Vel: {rb.linearVelocity}");
    }

    private IEnumerator JumpToPlayer()
    {
        isJumping = true;

        // ?? ?????????
        Physics2D.IgnoreLayerCollision(monsterLayer, platformLayerIndex, true);

        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(dir.x * moveSpeed, jumpForce), ForceMode2D.Impulse);

        yield return new WaitForSeconds(2.5f);

        // ????
        Physics2D.IgnoreLayerCollision(monsterLayer, platformLayerIndex, false);
        isJumping = false;
    }

    private bool CheckGrounded()
    {
        // ????????????
        Vector2 pos = transform.position;
        Vector2[] offsets = new Vector2[]
        {
            Vector2.zero,
            new Vector2(0.4f, 0f),
            new Vector2(-0.4f, 0f)
        };

        foreach (var offset in offsets)
        {
            RaycastHit2D hit = Physics2D.Raycast(pos + offset, Vector2.down, groundCheckDistance, platformLayer);
            if (hit.collider != null)
                return true;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 pos = transform.position;
        Gizmos.DrawLine(pos, pos + Vector2.down * groundCheckDistance);
        Gizmos.DrawLine(pos + new Vector2(0.4f, 0f), pos + new Vector2(0.4f, -groundCheckDistance));
        Gizmos.DrawLine(pos + new Vector2(-0.4f, 0f), pos + new Vector2(-0.4f, -groundCheckDistance));
    }
}
