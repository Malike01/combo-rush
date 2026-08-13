using UnityEngine;
using System.Collections;

public class BuildingInteractive : MonoBehaviour
{
    private Vector3 originalScale;
    private bool isAnimating = false;

    [Header("Animasyon Ayarları")]
    public float bounceHeight = 1.2f; 
    public float animSpeed = 0.2f;    
    void Start()
    {
        originalScale = transform.localScale;
    }

    void OnMouseDown()
    {
        DioramaItem item = GetComponent<DioramaItem>();
        if (item != null && !item.IsUnlocked) return; 
        if (!isAnimating)
        {
            StartCoroutine(BounceRoutine());
        }
    }

    private IEnumerator BounceRoutine()
    {
        isAnimating = true;

        Vector3 targetScale = new Vector3(originalScale.x * 0.9f, originalScale.y * bounceHeight, originalScale.z * 0.9f);
        float elapsed = 0f;

        while (elapsed < animSpeed / 2f)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / (animSpeed / 2f));
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < animSpeed / 2f)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / (animSpeed / 2f));
            yield return null;
        }

        transform.localScale = originalScale;
        isAnimating = false;
    }
}