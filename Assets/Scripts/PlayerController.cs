using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    private Rigidbody rb;
    [SerializeField] private bool canJump = false;
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private float move = 0;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent <Rigidbody>();
        canJump = false;
    }

    // Update is called once per frame
    void Update()
    {
        //move = Input.GetAxis("Horizontal") * moveSpeed;

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            canJump = true;
        }
    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector3(moveSpeed, rb.velocity.y, 0);

        if (canJump)
        {
            Debug.Log("Jump");
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            canJump = false;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            // Handle collision with obstacle (e.g., game over)
            Debug.Log("Game Over!");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
