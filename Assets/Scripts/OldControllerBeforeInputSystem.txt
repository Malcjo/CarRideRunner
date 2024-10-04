using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;   // Horizontal movement speed
    public float jumpForce = 10f;  // Initial jump force
    public float maxJumpTime = 0.3f;  // Maximum time the player can hold the jump button to extend the jump
    public float fallMultiplier = 4f;  // Increases gravity when falling
    public float lowJumpMultiplier = 2f;  // Applies when the player releases the jump button early

    public float slopeLimit = 45f;  // Maximum angle the player can walk on
    public float slopeCheckDistance = 1f;  // Distance for checking the slope
    private bool onSlope = false;
    private Vector3 slopeNormal;

    private Animator animator;   // Reference to the Animator component
    public GameManager gameManager;

    private Rigidbody rb;
    private bool isGrounded = false;
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    public GameObject levelmanager;
    public GameObject startingPosition;

    public Camera cam;
    private CameraShake cameraShake;  // Reference to the CameraShake script

    public GameObject playerVisual;
    public bool isAlive = true;

    private bool isJumping = false;  // Track if the player is currently jumping
    private bool isFalling = false;  // Track if the player is falling (but not jumping)
    private bool wasGrounded = true; // Track whether the player was grounded last frame
    private float jumpTimeCounter;   // Tracks how long the player has been holding the jump button

    // Coyote time and jump buffer variables
    public float coyoteTime = 0.1f;  // Allow jumping shortly after leaving the ground
    private float coyoteTimeCounter;
    public float jumpBufferTime = 0.1f;  // Allow buffering the jump input slightly before landing
    private float jumpBufferCounter;

    // Fall velocity threshold for triggering the camera shake
    public float fallVelocityThreshold = -8f;  // Minimum falling speed to trigger camera shake

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();  // Reference the Animator in the child object
        cameraShake = cam.GetComponent<CameraShake>();
        isAlive = true;
        transform.position = startingPosition.transform.position;
    }
    public float GetSpeed()
    {
        return moveSpeed;
    }
    void Update()
    {
        if (isAlive)
        {
            HandleAnimations();
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
                isFalling = false;  // Not falling since we're jumping
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
            // If the player walks off the platform without jumping
            if (!isGrounded && !isJumping)
            {
                isFalling = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isAlive)
        {
            // Check for slopes and ground detection
            CheckSlope();

            // If on a slope, adjust movement to follow the slope normal
            if (onSlope && isGrounded)
            {
                // Move along the slope
                Vector3 slopeMoveDirection = Vector3.ProjectOnPlane(transform.forward, slopeNormal).normalized;
                rb.velocity = slopeMoveDirection * moveSpeed + new Vector3(0, rb.velocity.y, 0);  // Preserve Y velocity for jumping
            }
            else
            {
                // Normal horizontal movement if not on slope
                rb.velocity = new Vector3(moveSpeed, rb.velocity.y, 0);
            }

            // Ground detection
            RaycastHit hit;
            if (Physics.Raycast(groundCheck.position, Vector3.down, out hit, groundCheckRadius, LayerMask.GetMask("Hitbox")))
            {
                if (hit.collider.CompareTag("Ground"))
                {
                    isFalling = false;
                    isGrounded = true;
                }
                else
                {
                    isGrounded = false;
                }
            }
            else
            {
                isGrounded = false;
            }
            //isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundCheckRadius, groundLayer);

            // Apply custom gravity when falling
            if (rb.velocity.y < 0)  // Player is falling
            {
                rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (rb.velocity.y > 0 && !isJumping)  // Player released the jump button early
            {
                rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }

            // Trigger the shake when the player lands after falling
            if (!wasGrounded && isGrounded && rb.velocity.y <= 0)
            {
                // Check if the player was falling fast enough to warrant a camera shake
                if (rb.velocity.y <= fallVelocityThreshold)
                {
                    // Player has landed after falling fast enough
                    cameraShake.LightVerticalShake();  // Call your shake method for landing
                }
            }

            // Track whether the player was grounded in the last frame
            wasGrounded = isGrounded;
        }
    }

    // Check if the player is on a slope
    private void CheckSlope()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, slopeCheckDistance))
        {
            slopeNormal = hit.normal;
            float angle = Vector3.Angle(slopeNormal, Vector3.up);
            onSlope = angle > 0 && angle <= slopeLimit;
        }
        else
        {
            onSlope = false;
        }
    }

    // Handle collisions with obstacles
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Killbox"))
        {
            // Trigger the shake and handle the destruction and restart process
            StartCoroutine(HandlePlayerHitObstacle());
        }
    }

    // Coroutine to shake the camera, destroy the player, and restart the game
    private IEnumerator HandlePlayerHitObstacle()
    {
        gameManager.PlayerDied();
        isAlive = false;
        yield return new WaitForSeconds(0.25f);
        // Step 1: Shake the camera
        cameraShake.LightHitShake();  // Full shake for obstacle collision

        // Step 3: Destroy the player
        Destroy(playerVisual);  // Destroy the player object

        // Step 4: Pause again for 1 second before restarting
        yield return new WaitForSeconds(2f);

        // Step 5: Restart the game (reload the scene)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Hitbox"))
        {
            if (other.CompareTag("StartGame"))
            {
                Debug.Log("Start Spawning");
                levelmanager.GetComponent<LevelGenerator>().StartSpawning();
            }
        }

    }

    private void HandleAnimations()
    {
        // Set animator parameters based on player state
        animator.SetFloat("Speed", Mathf.Abs(moveSpeed));
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isFalling", isFalling);
    }
}
