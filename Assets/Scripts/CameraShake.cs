using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public bool isShaking = false;  // Flag to prevent multiple shakes
    private CameraFollowTarget cameraFollowTarget;  // Reference to the CameraFollowTarget script
    private Vector3 shakeOffset = Vector3.zero;  // Store the shake offset
    private Vector3 targetShakeOffset = Vector3.zero;  // Store the target shake offset for lerping

    public Vector4 LightVerticalShakeValues;  // Values for LightVerticalShake: (duration, magnitude, speed, lerp speed)

    void Start()
    {
        cameraFollowTarget = GetComponent<CameraFollowTarget>();  // Get the reference to CameraFollowTarget script
    }

    public void LightHitShake()
    {
        Vector3 direction = new Vector3(1, 1, 0);  // Shake in horizontal and vertical direction
        StartCoroutine(Shake(0.5f, 0.05f, direction, 1.5f, 10, Vector3.zero));  // No specific start direction for this shake
    }

    public void LightVerticalShake()
    {
        Vector3 direction = new Vector3(0, 1, 0);  // Shake in vertical direction only
        //StartCoroutine(Shake(0.1f, 10f, direction, 1f, 1f));
        StartCoroutine(Shake(LightVerticalShakeValues.x, LightVerticalShakeValues.y, direction, LightVerticalShakeValues.z, LightVerticalShakeValues.w, Vector3.down));  // Always start moving downward
    }

    public void LightHorizontalShake()
    {
        Vector3 direction = new Vector3(1, 0, 0);  // Shake in horizontal direction only
        StartCoroutine(Shake(0.5f, 0.05f, direction, 1.5f, 10, Vector3.left));  // Always start moving left
    }

    // Add 'startDirection' to control initial shake direction
    public IEnumerator Shake(float duration, float magnitude, Vector3 shakeDirection, float speed, float shakeLerpSpeed, Vector3 startDirection)
    {
        isShaking = true;
        float elapsed = 0.0f;

        // Apply the initial shake movement in the specified start direction
        if (startDirection != Vector3.zero)  // Only apply if a direction is provided
        {
            targetShakeOffset = startDirection * magnitude;
            shakeOffset = targetShakeOffset;
            cameraFollowTarget.SetShakeOffset(shakeOffset);

            // Wait for a frame to apply the initial offset before starting the random shake
            yield return new WaitForEndOfFrame();
        }

        while (elapsed < duration)
        {
            // After the initial movement, calculate the new random shake target offset
            float x = Random.Range(-1f, 1f) * magnitude * shakeDirection.x;
            float y = Random.Range(-1f, 1f) * magnitude * shakeDirection.y;
            float z = Random.Range(-1f, 1f) * magnitude * shakeDirection.z;

            // Set the new target shake offset
            targetShakeOffset = new Vector3(x, y, z);

            // Lerp towards the new shake offset smoothly within the same frame, without extending the overall duration
            shakeOffset = Vector3.Lerp(shakeOffset, targetShakeOffset, Time.deltaTime * shakeLerpSpeed);

            // Apply the smoothed shake offset to the camera
            cameraFollowTarget.SetShakeOffset(shakeOffset);

            elapsed += Time.deltaTime * speed;  // Speed up or slow down the shake based on the provided speed

            yield return null;
        }

        // Smoothly reset the shake offset back to zero after shaking
        while (shakeOffset != Vector3.zero)
        {
            shakeOffset = Vector3.Lerp(shakeOffset, Vector3.zero, Time.deltaTime * shakeLerpSpeed);
            cameraFollowTarget.SetShakeOffset(shakeOffset);
            yield return null;
        }

        isShaking = false;
    }
}
