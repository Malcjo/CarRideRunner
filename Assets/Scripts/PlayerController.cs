using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;  // New Input System

public class PlayerController : MonoBehaviour
{
    private Animator animator;   // Reference to the Animator component
    public GameManager gameManager;
    private Rigidbody rb;
    private PlayerControls controls;  // Reference to the new Input System controls

    public GameObject levelmanager;
    public GameObject startingPosition;

    public GameObject playerVisual;
    public bool isAlive = true;

    [SerializeField] private float standardMaxSpeed = 20f;
    public float maxSpeed = 10f;    // Maximum speed the player can reach
    public float initialSpeed = 3f; // Initial speed at the start of the level
    public float obstacleSpeedReduction = 2f; // Speed reduction when hitting an obstacle
    public float speedIncreaseRate = 0.5f;  // How fast the player increases speed to max
    public float speedDecreaseRate = 0.1f;  // How fast the player's speed decreases after exceeding maxSpeed
    [SerializeField] private float currentSpeed;     // Current running speed

    private bool isRecentlyHit = false;  // Track if the player was recently hit
    public float recentlyHitDuration = 2f;  // Time window before `isRecentlyHit` is set to false again

    public float jumpForce = 10f;  // Initial jump force
    public float maxJumpTime = 0.3f;  // Maximum time the player can hold the jump button to extend the jump
    public float fallMultiplier = 4f;  // Increases gravity when falling
    public float lowJumpMultiplier = 2f;  // Applies when the player releases the jump button early

    public float slopeLimit = 45f;  // Maximum angle the player can walk on
    public float slopeCheckDistance = 1f;  // Distance for checking the slope
    private bool onSlope = false;
    private Vector3 slopeNormal;

    public float slideDuration = 1f;  // Duration of the slide
    public float slopeSlideSpeedMultiplier = 1.5f; // How fast the player accelerates on a downward slope
    public float slopeSlideSpeedReduction = 0.7f;  // Speed reduction on upward slopes
    public float flatSlideSpeedReduction = 0.9f;   // Speed reduction on flat ground
    public float flatSlideBoost = 1.1f;  // Speed boost when sliding on flat ground

    [SerializeField] private bool isSliding = false;  // Track if the player is sliding
    private bool canJumpFromSlide = false;  // Track if the player can jump from sliding
    private float slideTimeCounter = 0f;  // Track how long the player has been sliding



    [SerializeField] private bool isGrounded = false;
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;


    public Camera cam;
    private CameraShake cameraShake;  // Reference to the CameraShake script



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

    // Input System Binding
    private void BindInputActions()
    {
        controls.Movement.Jump.started += ctx => OnJumpStart();
        controls.Movement.Jump.canceled += ctx => OnJumpEnd();
        controls.Movement.Slide.started += ctx => StartSlide();  // Bind sliding to input
    }

    // Player Initialization
    private void InitializePlayer()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();  // Reference the Animator in the child object
        cameraShake = cam.GetComponent<CameraShake>();
        isAlive = true;
        currentSpeed = initialSpeed;  // Set initial speed
        transform.position = startingPosition.transform.position;
    }


    void Update()
    {
        if (isAlive)
        {
            if (isSliding)
            {
                HandleSliding();
            }
            else
            {
                //add additional logic if I need to have different movement apart from sliding
            }
            HandleAnimations();
            UpdateCoyoteTime();
            HandleJumpBuffer();
            // OLD INPUT SYSTEM (COMMENTED OUT)
            /*
            HandleJumpOldInput();  // Call the old input system's jump logic (if needed)
            */
            AdjustPlayersSpeed();

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
    //~~~~~~~~~~~~~~~~~~~~Player Sliding~~~~~~~~~~~~~
    // Handle sliding state and transition back to running
    private void HandleSliding()
    {
        slideTimeCounter += Time.deltaTime;

        if (slideTimeCounter >= slideDuration)
        {
            StopSlide();
        }

        if (canJumpFromSlide && Input.GetButtonDown("Jump"))
        {
            StopSlide();
            Jump();
        }
    }

    // Start sliding
    private void StartSlide()
    {
        if (!isSliding)
        {
            Debug.Log("Start Sliding");
            isSliding = true;
            canJumpFromSlide = true;
            slideTimeCounter = 0f;
            animator.SetBool("isSliding", true);

            if (onSlope)
            {
                if (slopeNormal.y < 1f)  // Slope going down
                {
                    currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed * slopeSlideSpeedMultiplier, Time.deltaTime);
                }
                else if (slopeNormal.y > 1f)  // Slope going up
                {
                    currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed * slopeSlideSpeedReduction, Time.deltaTime);
                }
            }
            else
            {
                currentSpeed *= flatSlideBoost;
                maxSpeed *= flatSlideSpeedReduction;
            }
        }
    }

    // Stop sliding and return to running/idle
    private void StopSlide()
    {
        Debug.Log("Stop Sliding");
        isSliding = false;
        canJumpFromSlide = false;
        //animator.SetBool("isSliding", false); //commented out for now, have no animation
        maxSpeed = standardMaxSpeed;  // Reset to default max speed
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    //~~~~~~~~~~~~~~~~Players Speed and movement~~~~~~~~~~
    public float GetSpeed()
    {
        return currentSpeed;
    }

    // adjust players speed
    private void AdjustPlayersSpeed()
    {
        // Increase the player's current speed up to the max speed
        if (currentSpeed < maxSpeed)
        {
            currentSpeed += speedIncreaseRate * Time.deltaTime;
        }

        // If the current speed exceeds the maxSpeed, reduce it gradually
        if (currentSpeed > maxSpeed)
        {
            currentSpeed -= speedDecreaseRate * Time.deltaTime;
        }

    }

    // Apply Movement Logic
    private void ApplyMovement()
    {
        if (onSlope && isGrounded)
        {
            // Move along the slope
            Vector3 slopeMoveDirection = Vector3.ProjectOnPlane(transform.forward, slopeNormal).normalized;
            rb.velocity = slopeMoveDirection * currentSpeed + new Vector3(0, rb.velocity.y, 0);  // Preserve Y velocity for jumping
        }
        else
        {
            // Normal horizontal movement
            rb.velocity = new Vector3(currentSpeed, rb.velocity.y, 0);
        }
    }

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


    //~~~~~~~~~~~Ground detection logic~~~~~~~~~~~~~
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
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    //~~~~~~~~~~~~~~~~~~~~~Jumping Logic~~~~~~~~~~~~~~~~~~
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
    // Jump logic
    private void Jump()
    {
        if (isGrounded)
        {
            isJumping = true;
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }
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



    private void HandleAnimations()
    {
        // Set animator parameters based on player state
        animator.SetFloat("Speed", Mathf.Abs(currentSpeed));

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
            StartCoroutine(HandlePlayerDeath());
        }
    }

    // Coroutine to reset the `isRecentlyHit` flag after a few seconds
    private IEnumerator ResetRecentlyHit()
    {
        yield return new WaitForSeconds(recentlyHitDuration);
        isRecentlyHit = false;
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
            if (other.CompareTag("Obstacle"))
            {
                if (isRecentlyHit)
                {
                    StartCoroutine(HandlePlayerDeath());  // Player dies if hit again while `isRecentlyHit` is true
                }
                else
                {
                    // Player is hit but doesn't die
                    isRecentlyHit = true;
                    currentSpeed = Mathf.Max(currentSpeed - obstacleSpeedReduction, 0);  // Reduce the player's speed
                    StartCoroutine(ResetRecentlyHit());
                }
            }
        }

    }


    // Coroutine for Player death
    private IEnumerator HandlePlayerDeath()
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