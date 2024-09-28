using UnityEngine;

public class CameraFollowTarget : MonoBehaviour
{
    public Transform cameraTarget;   // The empty GameObject the camera will follow
    public float smoothSpeed = 0.125f;  // Speed of camera smoothing
    public Vector3 offset = new Vector3(0, 5, -10);  // Offset from the camera target
    public Transform player;  // Reference to the player for speed-based zoom

    public float minZoom = 60f;  // Minimum FOV/Zoom
    public float maxZoom = 80f;  // Maximum FOV/Zoom
    public float zoomSpeed = 10f;  // How quickly the zoom adjusts

    private Camera cam;
    private Rigidbody playerRb;
    private Vector3 velocity = Vector3.zero;  // For SmoothDamp smoothing

    private void Start()
    {
        cam = Camera.main;
        playerRb = player.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // Smoothly move the camera to the target position using SmoothDamp
        Vector3 desiredPosition = cameraTarget.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

        // Adjust zoom based on player speed and vertical movement
        AdjustZoom();
    }

    void AdjustZoom()
    {
        float playerSpeed = playerRb.velocity.magnitude;  // Calculate the player's speed
        float playerVerticalSpeed = playerRb.velocity.y;  // Calculate vertical speed (for jumps)

        // Dynamic zoom based on player speed
        float targetZoom = Mathf.Lerp(minZoom, maxZoom, playerSpeed / zoomSpeed);

        // Zoom out more if the player is jumping upwards
        if (playerVerticalSpeed > 0.1f)
        {
            targetZoom += 5f;  // Increase zoom slightly when jumping
        }

        // Smoothly interpolate between the current zoom and the target zoom
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetZoom, Time.deltaTime);
    }
}
