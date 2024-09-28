using UnityEngine;

public class CameraTargetFollow : MonoBehaviour
{
    public Transform player;      // Reference to the player object
    public float minFollowSpeed = 0.05f;  // Minimal smoothing time for slow movement
    public float maxFollowSpeed = 0.3f;   // Maximum smoothing time for fast movement or jumping
    public Vector3 offset = new Vector3(0, 2, -10);  // Offset of the camera target relative to the player
    public float verticalDelay = 1.2f;
    public float HorizontalDelay = 5f;

    private Vector3 velocity = Vector3.zero;  // Reference for smooth damping
    private Rigidbody playerRb;  // To get the player's speed

    private float currentFollowSpeed;  // Store the current follow speed

    void Start()
    {
        playerRb = player.GetComponent<Rigidbody>();
        currentFollowSpeed = minFollowSpeed;  // Start with minimal delay
    }

    void FixedUpdate()
    {
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
            currentFollowSpeed = Mathf.Lerp(currentFollowSpeed, minFollowSpeed, Time.deltaTime * HorizontalDelay);
        }

        // Smoothly move the CameraTarget towards the target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, currentFollowSpeed);
    }
}
