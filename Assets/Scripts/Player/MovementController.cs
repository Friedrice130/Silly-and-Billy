using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    Rigidbody2D rb;
    public int speed;

    [Header("Jump System")]
    [SerializeField] float jumpTime;
    [SerializeField] int jumpPower;
    [SerializeField] float fallMultiplier;
    [SerializeField] float jumpMultiplier;

    public Transform groundCheck;
    public LayerMask groundLayer;
    Vector2 vecGravity;

    bool isJumping;
    float jumpCounter;

    Vector2 vecMove;

    void Start()
    {
        vecGravity = new Vector2(0, -Physics2D.gravity.y);
        rb = GetComponent<Rigidbody2D>();
    }

    public void Jump(InputAction.CallbackContext value)
    {
        if (value.started && isGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
            isJumping = true;
            jumpCounter = 0;
        }
        if (value.canceled)
        {
            isJumping = false;
            jumpCounter = 0;

            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.6f);
            }
        }
    }

    public void Move(InputAction.CallbackContext value)
    {
        vecMove = value.ReadValue<Vector2>();
        flip();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(vecMove.x * speed, rb.linearVelocity.y);

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity -= vecGravity * fallMultiplier * Time.deltaTime;
        }

        if (rb.linearVelocity.y > 0 && isJumping)
        {
            jumpCounter += Time.deltaTime;
            if (jumpCounter > jumpTime) isJumping = false;

            float t = jumpCounter / jumpTime;
            float currentJumpM = jumpMultiplier;

            if (t > 0.5f)
            {
                currentJumpM = jumpMultiplier * (1 - t);
            }
            rb.linearVelocity += vecGravity * currentJumpM * Time.deltaTime;
        }
    }

    bool isGrounded()
    {
        return Physics2D.OverlapCapsule(groundCheck.position, new Vector2(1.8f, 0.3f), CapsuleDirection2D.Horizontal, 0, groundLayer);
    }

    void flip()
    {
        if (vecMove.x < -0.01f) transform.localScale = new Vector3(-1, 1, 1);
        if (vecMove.x > 0.01f) transform.localScale = new Vector3(1, 1, 1);
    }
}
