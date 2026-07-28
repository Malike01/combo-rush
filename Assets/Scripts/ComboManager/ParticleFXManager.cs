using System.Collections;
using UnityEngine;

public class ParticleFXManager : MonoBehaviour
{
    public static ParticleFXManager Instance;

    [Header("Particle Prefab")]
    [SerializeField] private ParticleSystem burstParticlePrefab;

    [Header("Camera Shake")]
    [SerializeField] private bool enableCameraShake = true;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayBurst(Vector3 position, Color color, int jellyCount = 3)
    {
        if (burstParticlePrefab == null) return;

        ParticleSystem fx = Instantiate(burstParticlePrefab, position, Quaternion.identity);
        
        var main = fx.main;
        main.startColor = color;

        var emission = fx.emission;
        var burst = emission.GetBurst(0);
        short extraParticles = (short)(jellyCount * 5);
        burst.count = new ParticleSystem.MinMaxCurve(30 + extraParticles, 45 + extraParticles);
        emission.SetBurst(0, burst);

        fx.Play();

        if (enableCameraShake && Camera.main != null)
        {
            StartCoroutine(ShakeCamera(0.1f, Mathf.Clamp(0.04f + (jellyCount * 0.01f), 0.04f, 0.12f)));
        }

        Destroy(fx.gameObject, main.duration + main.startLifetime.constantMax);
    }

    private IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Transform camTransform = Camera.main.transform;
        Vector3 originalPos = camTransform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            camTransform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        camTransform.localPosition = originalPos;
    }
}