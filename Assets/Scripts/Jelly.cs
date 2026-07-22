using System.Collections;
using UnityEngine;

public class Jelly : MonoBehaviour
{
    [Header("Grid Koordinatları")]
    public int x;
    public int y;

    [Header("Renk Indeksi")]
    public int colorIndex;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Coroutine activeTween;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    public void SetSprite(Sprite sprite, int index)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        
        spriteRenderer.sprite = sprite;
        colorIndex = index;
    }

    // Input Manager methods for visual feedback
    public void SelectSquish()
    {
        if (activeTween != null) StopCoroutine(activeTween);
        Vector3 targetScale = originalScale * 1.1f;
        activeTween = StartCoroutine(AnimateScale(targetScale, 0.1f));
    }

    public void ResetShape()
    {
        if (activeTween != null) StopCoroutine(activeTween);
        activeTween = StartCoroutine(AnimateScale(originalScale, 0.1f));
    }

    private IEnumerator AnimateScale(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }

    public IEnumerator ShakeJelly(float duration = 0.12f, float magnitude = 0.05f)
    {
        Vector3 basePos = transform.position;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            transform.position = new Vector3(basePos.x + offsetX, basePos.y + offsetY, basePos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = basePos;
    }

    public IEnumerator AnimateMergeTo(Vector3 centerTarget, float delay)
    {
        yield return new WaitForSeconds(delay);

        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t;

            transform.position = Vector3.Lerp(startPos, centerTarget, t);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = centerTarget;
        transform.localScale = Vector3.zero;
    }
}