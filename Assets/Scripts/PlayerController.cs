using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;  // New Input System

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
    [SerializeField] private bool isGrounded = false;
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    public GameObject levelmanager;
    public GameObject startingPosition;

    public Camera cam;
    private CameraShake cameraShake;  // Reference to the CameraShake script

    public GameObject playerVisual;
    public bool isAlive = true;

    [SerializeField] private bool isJumping = false;  // Track if the player is currently jumping
    [SerializeField] private bool isFalling = false;  // Track if the player is falling (but not jumping)
    [SerializeField] private bool wasGrounded = true; // Track whether the player was grounded last frame
    [SerializeField] private float jumpTimeCounter;   // Tracks how long the player has been holding the jump button

    // Coyote time and jump buffer variables
    public float coyoteTime = 0.1f;  // Allow jumping shortly after leaving the ground
    private float coyoteTimeCounter;
    public float jumpBufferTime = 0.1f;  // Allow buffering the jump input slightly before landing
    private float jumpBufferCounter;

    // Fall velocity threshold for triggering the camera shake
    public float fallVelocityThreshold = -8f;  // Minimum falling speed to trigger camera shake

    private PlayerControls controls;  // Reference to the new Input System controls
    private bool isJumpButtonHeld = false;  // Track if jump button is being held

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        controls = new PlayerControls();  // Initialize controls

        // Bind input actions for the new Input System
        BindInputActions();
    }

    private void OnEnable()
    {
        controls.Movement.Enable();  // Enable controls
    }

    private void OnDisable()
    {
        controls.Movement.Disable();  // Disable controls
    }

    void Start()
    {
        InitializePlayer();
    }

    void Update()
    {
        if (isAlive)
        {
            HandleAnimations();
            UpdateCoyoteTime();
            HandleJumpBuffer();
            // OLD INPUT SYSTEM (COMMENTED OUT)
            /*
            HandleJumpOldInput();  // Call the old input system's jump logic (if needed)
            */
        }
    }

    private void FixedUpdate()
    {
        if (isAlive)
        {
            CheckSlope();
            ApplyMovement();
            HandleJumping();
            ApplyCustomGravity();
            GroundDetection();
            HandleLanding();

            // Update falling state
            if (!isGrounded && !isJumping)
            {
                isFalling = true;
            }
            else
            {
                isFalling = false;
            }
        }
    }

    // Input System Binding
    private void BindInputActions()
    {
        controls.Movement.Jump.started += ctx => OnJumpStart();
        controls.Movement.Jump.canceled += ctx => OnJumpEnd();
    }

    // Player Initialization
    private void InitializePlayer()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();  // Reference the Animator in the child object
        cameraShake = cam.GetComponent<CameraShake>();
        isAlive = true;
        transform.position = startingPosition.transform.position;
    }

    // Coyote Time Logic
    private void UpdateCoyoteTime()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;  // Reset coyote time when grounded
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;  // Decrease coyote time when in the air
        }
    }

    // Jump Buffer Logic
    private void HandleJumpBuffer()
    {
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0 && (isGrounded || coyoteTimeCounter > 0))
        {
            isJumping = true;
            isFalling = false;  // Not falling since we're jumping
            jumpTimeCounter = maxJumpTime;  // Reset jump time counter
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);  // Initial jump force
            jumpBufferCounter = 0;  // Reset jump buffer once jump is triggered
        }
    }

    // Apply Movement Logic
    private void ApplyMovement()
    {
        if (onSlope && isGrounded)
        {
            // Move along the slope
            Vector3 slopeMoveDirection = Vector3.ProjectOnPlane(transform.forward, slopeNormal).normalized;
            rb.velocity = slopeMoveDirection * moveSpeed + new Vector3(0, rb.velocity.y, 0);  // Preserve Y velocity for jumping
        }
        else
        {
            // Normal horizontal movement
            rb.velocity = new Vector3(moveSpeed, rb.velocity.y, 0);
        }
    }

    public float GetSpeed()
    {
        return moveSpeed;
    }

    // Handle Jumping Logic
    private void HandleJumping()
    {
        if (isJumpButtonHeld && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
                jumpTimeCounter -= Time.fixedDeltaTime;
            }
            else
            {
                isJumping = false;  // Stop extending the jump when time is up
            }
        }
    }

    // Apply Custom Gravity Logic
    private void ApplyCustomGravity()
    {
        if (rb.velocity.y < 0)  // Player is falling
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !isJumping)  // Player released the jump button early
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    // Ground Detection Logic
    private void GroundDetection()
    {
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
    }

    // Landing Logic
    private void HandleLanding()
    {
        if (!wasGrounded && isGrounded && rb.velocity.y <= 0)
        {
            if (rb.velocity.y <= fallVelocityThreshold)
            {
                cameraShake.LightVerticalShake();
            }
        }

        wasGrounded = isGrounded;
    }

    // Slope Check Logic
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

    // Jump Input Handlers (New Input System)
    private void OnJumpStart()
    {
        isJumpButtonHeld = true;
        jumpBufferCounter = jumpBufferTime;  // Start jump buffering
    }

    private void OnJumpEnd()
    {
        isJumpButtonHeld = false;
        isJumping = false;  // Stop extending the jump when button is released
    }

    // OLD INPUT SYSTEM (COMMENTED OUT)
    /*
    private void HandleJumpOldInput()
    {
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
    */

    // Handle animations

    private void HandleAnimations()
    {
        // Set animator parameters based on player state
        animator.SetFloat("Speed", Mathf.Abs(moveSpeed));

        // Update grounded, jumping, and falling states
        animator.SetBool("isGrounded", isGrounded);

        // Update jump and fall conditions
        if (isJumping && !isGrounded)
        {
            animator.SetBool("isJumping", true);
            animator.SetBool("isFalling", false);
        }
        else if (isFalling)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", true);
        }
        else if (isGrounded)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
        }
    }

    /* old way to handle animation bools

    private void HandleAnimations()
    {
        animator.SetFloat("Speed", Mathf.Abs(moveSpeed));
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isFalling", isFalling);
    }
    */
    // Handle collisions with obstacles
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Killbox"))
        {
            StartCoroutine(HandlePlayerHitObstacle());
        }
    }

    //handle triggers
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


    // Coroutine for Player death
    private IEnumerator HandlePlayerHitObstacle()
    {
        gameManager.PlayerDied();
        isAlive = false;
        yield return new WaitForSeconds(0.25f);
        cameraShake.LightHitShake();
        Destroy(playerVisual);
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
