using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    Rigidbody2D rb;
	
	[SerializeField] int speed;
	float speedMultiplier;
	
	[Range(1,10)]
	[SerializeField] float acceleration;
	
	bool btnPressed;
	
	bool isWallTouch;
	public LayerMask wallLayer;
	public Transform wallCheckPoint;
	
	Vector2 relativeTransform;
	
	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
	}
	
	private void Start()
	{
		UpdateRelativeTransform();
	}
	
	private void FixedUpdate()
	{
		UpdateSpeedMultiplier();
		
		float targetSpeed = speed * speedMultiplier * relativeTransform.x;
		rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
		
		isWallTouch = Physics2D.OverlapBox(wallCheckPoint.position, new Vector2(0.06f, 0.96f), 0, wallLayer);
		
		if (isWallTouch)
		{
			Flip();
		}
	}
	
	public void Flip()
	{
		transform.Rotate(0, 180, 0);
		UpdateRelativeTransform();
	}
	
	void UpdateRelativeTransform()
	{
		relativeTransform = transform.InverseTransformVector(Vector2.one);
	}
	
	public void Move(InputAction.CallbackContext value)
	{
		if (value.started)
		{
			btnPressed = true;
		}
		else if (value.canceled)
		{
			btnPressed = false;
		}
	}
	
	void UpdateSpeedMultiplier()
	{
		if (btnPressed && speedMultiplier < 1)
		{
			speedMultiplier += Time.deltaTime * acceleration;
		}
		else if (!btnPressed && speedMultiplier > 0)
		{
			speedMultiplier -= Time.deltaTime * acceleration;
			if (speedMultiplier < 0) speedMultiplier = 0;
		}
	}
}