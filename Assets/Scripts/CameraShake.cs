using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public bool isShaking = false;  // Flag to prevent multiple shakes

    public void HitShake(float duration, float strength)
    {
        Vector3 direction = new Vector3(1, 1, 0);
        StartCoroutine(Shake(duration, strength, direction, 1.5f));
    }
    public IEnumerator Shake(float duration, float magnitude, Vector3 shakeDirection, float speed)
    {
        Debug.Log("Shake started");
        isShaking = true;
        Vector3 originalPosition = transform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Adjust the shake based on direction and speed
            float x = Random.Range(-1f, 1f) * magnitude * shakeDirection.x;
            float y = Random.Range(-1f, 1f) * magnitude * shakeDirection.y;
            float z = Random.Range(-1f, 1f) * magnitude * shakeDirection.z;

            // Apply the shake offset
            transform.localPosition = new Vector3(
                originalPosition.x + x,
                originalPosition.y + y,
                originalPosition.z + z
            );

            elapsed += Time.deltaTime * speed;  // Speed up or slow down the shake

            yield return null;
        }

        // Reset to the original position after shaking
        transform.localPosition = originalPosition;
        isShaking = false;
        Debug.Log("Shake ended");
    }
}
