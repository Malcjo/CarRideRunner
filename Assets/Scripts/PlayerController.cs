using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;   // Horizontal movement speed
    public float jumpForce = 7f;   // Force applied to jump
    private Rigidbody rb;
    [SerializeField] private bool isGrounded = false;  // Set to false by default
    public Transform groundCheck;  // Empty object to define the bottom of the player
    public float groundCheckRadius = 0.3f;  // Increased Radius for ground check
    public LayerMask groundLayer;  // Layer that defines what is considered "ground"
    private float move;
    private CameraShake cameraShake; // Reference to the CameraShake script

    private bool wasGrounded = false;  // Track if the player was grounded in the previous frame

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraShake = Camera.main.GetComponent<CameraShake>();  // Find the CameraShake script on the camera
    }

    void Update()
    {
        // Horizontal movement (only along the X-axis)
        //move = Input.GetAxis("Horizontal") * moveSpeed;

        // Jump when space is pressed and the player is grounded
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }
    }

    private void FixedUpdate()
    {
        // Store previous frame's grounded state
        wasGrounded = isGrounded;

        // Perform raycast to check if grounded
        RaycastHit hit;
        if (Physics.Raycast(groundCheck.position, Vector3.down, out hit, groundCheckRadius, groundLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        // Trigger camera shake when landing after a jump
        if (wasGrounded && !isGrounded && rb.velocity.y < 0 && !cameraShake.isShaking)
        {
            StartCoroutine(cameraShake.Shake(0.1f, 0.1f));
        }

        // Set the player velocity for horizontal movement (keep Z-axis at 0 for side-scrolling)
        rb.velocity = new Vector3(moveSpeed, rb.velocity.y, 0);
    }

    void OnDrawGizmos()
    {
        // Visualize the Raycast in the Scene view
        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckRadius);
    }
}
