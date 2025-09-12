using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    Rigidbody2D rb;
	public int speed;
	//public int jumpPower;
	
	Vector2 vecMove;
	
	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
	}
	
	void Update()
	{
		
	}
	
	//public void Jump(InputAction.CallbackContext value)
	//{
		//if (value.started)
		//{
			//rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
		//}
	//}

	
	public void Move(InputAction.CallbackContext value)
	{
		vecMove = value.ReadValue<Vector2>();
		flip();
	}
	
	private void FixedUpdate()
	{
		rb.linearVelocity = new Vector2(vecMove.x * speed, rb.linearVelocity.y);
	}
	
	void flip()
	{
		if (vecMove.x < -0.01f) transform.localScale = new Vector3(-1, 1, 1);
		if (vecMove.x > 0.01f) transform.localScale = new Vector3(1, 1, 1);
	}
}