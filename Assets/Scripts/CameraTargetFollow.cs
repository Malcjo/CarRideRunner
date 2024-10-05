using UnityEngine;

public class CameraTargetFollow : MonoBehaviour
{
    public Transform player;      // Reference to the player object
    public float minFollowSpeed = 0.05f;  // Minimal smoothing time for slow movement
    public float maxFollowSpeed = 0.3f;   // Maximum smoothing time for fast movement or jumping
    public Vector3 offset = new Vector3(0, 2, -10);  // Offset of the camera target relative to the player
    public float verticalDelay = 1.2f;
    public float horizontalDelay = 5f;

    // New variables for controlling subtle vertical follow
    public float verticalSmoothingSpeed = 0.05f;  // Speed for subtle vertical follow
    public float verticalThreshold = 0.2f;  // Minimum movement in the Y-axis before following
    private float targetYPosition;  // Target Y position for subtle vertical follow

    private Vector3 velocity = Vector3.zero;  // Reference for smooth damping
    private Rigidbody playerRb;  // To get the player's speed

    private float currentFollowSpeed;  // Store the current follow speed

    // Toggle to enable or disable camera follow
    public bool enableCameraFollow = true;  // Toggle to disable/enable camera follow

    void Start()
    {
        playerRb = player.GetComponent<Rigidbody>();
        currentFollowSpeed = minFollowSpeed;  // Start with minimal delay
        targetYPosition = transform.position.y;  // Set the initial target Y position
    }

    void FixedUpdate()
    {
        if (!enableCameraFollow)
        {
            // Camera follow is disabled, do nothing
            return;
        }

        // Calculate the target position based on player position + offset
        Vector3 targetPosition = player.position + offset;

        // Get the player's horizontal speed (magnitude of velocity on the x-axis)
        float playerSpeed = Mathf.Abs(playerRb.velocity.x);  // Horizontal speed
        float playerVerticalSpeed = playerRb.velocity.y;  // Vertical speed

        // Adjust smoothing time based on player state (moving/jumping)
        if (Mathf.Abs(playerVerticalSpeed) > 0.1f)  // If player is jumping or falling
        {
            // Increase the follow delay when the player is jumping or falling
            currentFollowSpeed = Mathf.Lerp(currentFollowSpeed, maxFollowSpeed, Time.deltaTime * verticalDelay);
        }
        else
        {
            // Smoothly reduce the follow delay back to the minimal value when the player is on the ground
            currentFollowSpeed = Mathf.Lerp(currentFollowSpeed, minFollowSpeed, Time.deltaTime * horizontalDelay);
        }

        // Handle subtle vertical movement
        if (Mathf.Abs(player.position.y - targetYPosition) > verticalThreshold)
        {
            targetYPosition = Mathf.Lerp(targetYPosition, player.position.y + offset.y, verticalSmoothingSpeed);
        }

        // Apply the target Y position to the follow object
        targetPosition.y = targetYPosition;

        // Smoothly move the CameraTarget towards the target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, currentFollowSpeed);
    }
}
