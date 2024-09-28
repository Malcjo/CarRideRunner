using UnityEngine;

public class CameraFollowTarget : MonoBehaviour
{
    public Transform cameraTarget;   // The empty GameObject the camera will follow
    public float smoothSpeed = 0.125f;  // Speed of camera smoothing
    public Vector3 offset = new Vector3(0, 5, -10);  // Offset from the camera target
    public Transform player;  // Reference to the player for speed-based zoom

    private Camera cam;
    private Rigidbody playerRb;
    private Vector3 velocity = Vector3.zero;  // For SmoothDamp smoothing

    public float minZoom = 60f;  // Minimum FOV/Zoom
    public float maxZoom = 80f;  // Maximum FOV/Zoom
    public float zoomSpeed = 10f;  // Speed of zoom interpolation
    public float longJumpThreshold = 0.5f;  // Time in air considered a "long jump"
    public float maxSpeed = 20f;  // Maximum player speed to normalize zoom

    private float timeInAir = 0f;  // Track how long the player is in the air
    private bool isGrounded = true;  // Track if the player is grounded (simplified)

    private Vector3 shakeOffset = Vector3.zero;  // Offset for camera shake

    private void Start()
    {
        cam = Camera.main;
        playerRb = player.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (player.GetComponent<PlayerController>().isAlive == true)
        {
            // Smoothly move the camera to the target position using SmoothDamp
            Vector3 desiredPosition = cameraTarget.position + offset;
            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

            // Apply the shake offset to the smoothed follow position
            transform.position = smoothedPosition + shakeOffset;

            // Update zoom based on player speed and jumping
            AdjustZoom();
        }
    }

    // Method to receive shake offset from the CameraShake script
    public void SetShakeOffset(Vector3 offset)
    {
        shakeOffset = offset;
    }

    void AdjustZoom()
    {
        float playerSpeed = Mathf.Clamp(playerRb.velocity.magnitude, 0, maxSpeed);  // Clamp player speed
        float playerVerticalSpeed = playerRb.velocity.y;  // Calculate vertical speed (for jumps)

        // Track how long the player is in the air
        if (Mathf.Abs(playerVerticalSpeed) > 0.1f)  // Player is jumping or falling
        {
            timeInAir += Time.deltaTime;  // Increase the time in the air
            isGrounded = false;
        }
        else if (!isGrounded)  // If the player lands (vertical speed is near zero)
        {
            timeInAir = 0f;  // Reset time in air when the player lands
            isGrounded = true;
        }

        // Gradual zoom adjustment based on speed percentage
        float speedFactor = playerSpeed / maxSpeed;  // Normalized speed (0 to 1)
        float jumpFactor = Mathf.Clamp01(timeInAir / longJumpThreshold);  // Normalized jump factor (0 to 1)

        // Combine speed and jump influence on zoom (weight them equally)
        float zoomFactor = Mathf.Clamp01(speedFactor + jumpFactor);  // Limit the combined factor to 1

        // Gradually approach the target zoom based on speedFactor (closer to maxZoom as speed increases)
        float targetZoom = Mathf.Lerp(minZoom, maxZoom, zoomFactor);

        // Smoothly interpolate between the current zoom and the target zoom
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetZoom, Time.deltaTime * zoomSpeed);
    }
}
