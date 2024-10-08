using UnityEngine;

public class CameraFollowTarget : MonoBehaviour
{
    public Transform cameraTarget;   // The empty GameObject the camera will follow
    public float smoothSpeed = 0.125f;  // Speed of camera smoothing
    public Vector3 offset = new Vector3(0, 5, -10);  // Default offset from the camera target
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

    // Toggles for controlling behavior
    public bool enableDynamicZoom = true;  // Toggle to enable/disable dynamic zoom and follow
    public bool enableRotationInsteadOfZoom = false;  // Toggle to enable rotation instead of zoom and follow

    public float rotationSpeed = 50f;  // Speed at which the camera rotates around the player
    public float verticalRotationLimit = 80f;  // Limit the vertical rotation to prevent the camera from flipping

    // Variables for camera rotation
    public float horizontalRotationSpeed = 100f;  // Speed of horizontal rotation
    public float verticalRotationSpeed = 50f;     // Speed of vertical rotation
    private float horizontalRotation = 0f;        // Track horizontal rotation
    private float verticalRotation = 0f;          // Track vertical rotation (up and down)

    // New variable for static camera position offset
    public Vector3 additionalOffset = new Vector3(0, 0, 0);  // Custom additional offset for camera position
    private Vector3 tempOffset;
    [SerializeField] private Vector3 LookatOffset;

    private void Start()
    {
        cam = Camera.main;
        playerRb = player.GetComponent<Rigidbody>();

        // Initialize rotation values to the current camera angles
        horizontalRotation = transform.eulerAngles.y;
        verticalRotation = transform.eulerAngles.x;
    }

    private void FixedUpdate()
    {
        if (player.GetComponent<PlayerController>().isAlive == true)
        {
            if (enableRotationInsteadOfZoom)
            {
                // Rotate around the player and look at them
                RotateAndLookAtPlayer();
            }
            else
            {
                if (enableDynamicZoom)
                {
                    // Smoothly move the camera to the target position using SmoothDamp
                    Vector3 desiredPosition = cameraTarget.position + offset;  // Apply the combined static offset
                    Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

                    // Apply the shake offset to the smoothed follow position
                    transform.position = smoothedPosition + shakeOffset;

                    // Rotate the camera to smoothly follow the cameraTarget
                    RotateTowardsTarget();

                    // Update zoom based on player speed and jumping
                    AdjustZoom();
                }
            }
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

    // Rotate the camera smoothly towards the cameraTarget
    void RotateTowardsTarget()
    {
        // Calculate the direction from the camera to the target
        Vector3 directionToTarget = cameraTarget.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        // Smoothly rotate the camera towards the target
        transform.rotation = Quaternion.Slerp(transform.rotation, (targetRotation), rotationSpeed * Time.deltaTime);
    }

    // New Method: Rotate around the player and use LookAt to keep the player in focus
    /* old rotation method
    void RotateAndLookAtPlayer()
    {
        // Clamp the vertical rotation to prevent flipping
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalRotationLimit, verticalRotationLimit);

        // Calculate the new rotation
        Quaternion rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0);

        // Use the player's position plus the static offset for positioning the camera
        //Vector3 newPosition = player.position + additionalOffset;  // Use the offset as a static position offset
        Vector3 newPosition = new Vector3(player.position.x + additionalOffset.x, transform.position.y, transform.position.y + additionalOffset.z);  // Use the offset as a static position offset

        Vector3 lookAtPosition = cameraTarget.position + LookatOffset;

        // Apply the new position to the camera (with static offset)
        transform.position = newPosition;

        // Make sure the camera always looks at the player
        //transform.LookAt(player);
        transform.LookAt(lookAtPosition);
    }
    */
    void RotateAndLookAtPlayer()
    {
        // Clamp the vertical rotation to prevent flipping
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalRotationLimit, verticalRotationLimit);

        // Calculate the new rotation target based on the vertical and horizontal rotation
        Quaternion targetRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0);

        // Use the player's position plus the static offset for positioning the camera
        Vector3 newPosition = new Vector3(player.position.x + additionalOffset.x, transform.position.y, transform.position.y + additionalOffset.z);  // Use the offset as a static position offset

        // Calculate the look at position (with the additional offset)
        Vector3 lookAtPosition = cameraTarget.position + LookatOffset;

        // Apply the new position to the camera (with static offset)
        transform.position = newPosition;

        // Smoothly rotate the camera towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // Make sure the camera always looks at the look-at position smoothly
        Vector3 directionToLookAt = lookAtPosition - transform.position;
        Quaternion lookAtRotation = Quaternion.LookRotation(directionToLookAt);

        // Smoothly interpolate the camera's rotation towards the look-at target
        transform.rotation = Quaternion.Slerp(transform.rotation, lookAtRotation, Time.deltaTime * rotationSpeed);
    }
    

}
