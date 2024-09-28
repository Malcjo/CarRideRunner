using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;   // Horizontal movement speed
    public float jumpForce = 10f;  // Initial jump force
    public float maxJumpTime = 0.3f;  // Maximum time the player can hold the jump button to extend the jump
    public float fallMultiplier = 4f;  // Increases gravity when falling
    public float lowJumpMultiplier = 2f;  // Applies when the player releases the jump button early

    private Rigidbody rb;
    private bool isGrounded = false;
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;
    private CameraShake cameraShake;  // Reference to the CameraShake script


    private float move;
    private bool isJumping = false;  // Track if the player is currently jumping
    private float jumpTimeCounter;   // Tracks how long the player has been holding the jump button

    // Coyote time and jump buffer variables
    public float coyoteTime = 0.1f;  // Allow jumping shortly after leaving the ground
    private float coyoteTimeCounter;
    public float jumpBufferTime = 0.1f;  // Allow buffering the jump input slightly before landing
    private float jumpBufferCounter;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraShake = Camera.main.GetComponent<CameraShake>();
    }

    void Update()
    {
        // Horizontal movement
        move = Input.GetAxis("Horizontal") * moveSpeed;

        // Update coyote time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;  // Reset coyote time when grounded
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;  // Decrease coyote time when in the air
        }

        // Update jump buffer time
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;  // Buffer jump input when jump is pressed
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;  // Decrease jump buffer over time
        }

        // Jump logic: allow jump if grounded or within coyote time, and if jump buffer is active
        if (jumpBufferCounter > 0 && (isGrounded || coyoteTimeCounter > 0))
        {
            isJumping = true;
            jumpTimeCounter = maxJumpTime;  // Reset jump counter to maximum jump time
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);  // Apply initial jump force
            jumpBufferCounter = 0;  // Reset jump buffer once the jump is triggered
        }

        // While holding the jump button and still within max jump time
        if (Input.GetButton("Jump") && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                // Continue to apply upward force while holding the button
                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
                jumpTimeCounter -= Time.deltaTime;  // Decrease the jump time counter
            }
            else
            {
                isJumping = false;  // Stop extending the jump when time is up
            }
        }

        // Release jump button: stop upward force and apply gravity
        if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;  // Stop extending the jump when button is released
        }
    }

    private void FixedUpdate()
    {
        // Move player with physics-based velocity in FixedUpdate
        rb.velocity = new Vector3(moveSpeed, rb.velocity.y, 0);

        // Ground detection
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundCheckRadius, groundLayer);

        // Apply custom gravity when falling
        if (rb.velocity.y < 0)  // Player is falling
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !isJumping)  // Player released the jump button early
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
}
