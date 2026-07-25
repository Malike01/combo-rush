using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    /// <summary>
    /// Triggers a screen shake effect.
    /// </summary>
    /// <param name="duration">How long the shake lasts</param>
    /// <param name="magnitude">How violent the shake is</param>
    public void TriggerShake(float duration = 0.15f, float magnitude = 0.1f)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Create a random offset for the camera
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Return the camera strictly to its original position
        transform.localPosition = originalPos;
    }
}