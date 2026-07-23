using System.Collections;
using UnityEngine;

public class Jelly : MonoBehaviour
{
    [Header("Grid Coordinates")]
    public int x;
    public int y;

    [Header("Color Index")]
    public int colorIndex;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Coroutine activeTween;
    private Coroutine wiggleCoroutine;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    // --- SMOOTH & CUTE IDLE WIGGLE ---
    public void StartIdleWiggle()
    {
        if (wiggleCoroutine == null)
        {
            wiggleCoroutine = StartCoroutine(WiggleRoutine());
        }
    }

    public void StopIdleWiggle()
    {
        if (wiggleCoroutine != null)
        {
            StopCoroutine(wiggleCoroutine);
            wiggleCoroutine = null;
            
            // Smoothly return to selection scale or original scale
            if (activeTween == null)
            {
                transform.localScale = originalScale;
            }
        }
    }

  private IEnumerator WiggleRoutine()
    {
        float elapsed = 0f;
        float duration = 1.8f; // Slow and gentle breathing cycle
        Vector3 baseScale = transform.localScale;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            
            // Soft sine wave oscillation
            float pulse = Mathf.Sin(progress * Mathf.PI * 2f);
            
            // Very subtle squash & stretch (~3.5% scale change)
            float scaleX = baseScale.x + (pulse * 0.035f);
            float scaleY = baseScale.y - (pulse * 0.025f);

            transform.localScale = new Vector3(scaleX, scaleY, baseScale.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Smoothly reset scale after wiggle cycle completes
        transform.localScale = baseScale;
        wiggleCoroutine = null;
    }


    public void SetSprite(Sprite sprite, int index)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        colorIndex = index;
    }

    public void SelectSquish()
    {
        if (activeTween != null) StopCoroutine(activeTween);
        Vector3 targetScale = originalScale * 1.18f;
        activeTween = StartCoroutine(AnimateScale(targetScale, 0.12f));
    }

    public void ResetShape()
    {
        StopIdleWiggle();
        if (activeTween != null) StopCoroutine(activeTween);
        activeTween = StartCoroutine(AnimateScale(originalScale, 0.12f));
    }

    private IEnumerator AnimateScale(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // SmoothStep transition for soft touch feedback
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
        activeTween = null;
    }

    public IEnumerator ShakeJelly(float duration = 0.12f, float magnitude = 0.04f)
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