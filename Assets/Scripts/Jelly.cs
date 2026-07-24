using System.Collections;
using UnityEngine;

public class Jelly : MonoBehaviour
{
    [Header("Grid Coordinates")]
    public int x;
    public int y;

    [Header("Color Index")]
    public int colorIndex;
    
    // Shortcut for BoardManager algorithms to access color seamlessly
    public int themeIndex => colorIndex; 

    private Sprite[] evolutionSprites;
    private SpriteRenderer spriteRenderer;
    
    // Store the initial scale reference safely
    public Vector3 originalScale { get; private set; } = Vector3.one;
    private bool isScaleInitialized = false;

    private Coroutine activeScaleRoutine;
    private Coroutine wiggleCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        InitScale();
    }

    private void InitScale()
    {
        if (!isScaleInitialized)
        {
            // Lock the original baseline scale only once
            if (transform.localScale != Vector3.zero)
            {
                originalScale = transform.localScale;
            }
            else
            {
                originalScale = Vector3.one;
            }
            isScaleInitialized = true;
        }
    }

    public void SetupEvolution(Sprite[] sprites, int index)
    {
        InitScale();
        evolutionSprites = sprites;
        colorIndex = index;

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (sprites != null && sprites.Length > 0)
        {
            spriteRenderer.sprite = sprites[0];
        }
    }

    public void UpdateExpression(int comboCount)
    {
        if (evolutionSprites == null || evolutionSprites.Length == 0) return;

        int targetIndex = Mathf.Clamp(comboCount, 0, evolutionSprites.Length - 1);
        Sprite targetSprite = evolutionSprites[targetIndex];

        if (spriteRenderer.sprite != targetSprite)
        {
            spriteRenderer.sprite = targetSprite;
        }

        TriggerSquishImpact();
    }

    public void SelectSquish()
    {
        TriggerSquishImpact();
    }

    private void TriggerSquishImpact()
    {
        StopIdleWiggle();

        if (activeScaleRoutine != null) StopCoroutine(activeScaleRoutine);
        activeScaleRoutine = StartCoroutine(AnimateSquishRoutine());
    }

    private IEnumerator AnimateSquishRoutine()
    {
        float boostMultiplier = 1.05f;
        // Always calculate target scale relative to the static originalScale baseline
        Vector3 targetScale = new Vector3(originalScale.x * boostMultiplier, originalScale.y * (boostMultiplier - 0.05f), originalScale.z);
        float duration = 0.08f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            if (this == null || gameObject == null) yield break;

            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (this != null && gameObject != null)
        {
            transform.localScale = targetScale;
        }
        activeScaleRoutine = null;
    }

    public void ResetShape()
    {
        StopIdleWiggle();

        if (activeScaleRoutine != null) StopCoroutine(activeScaleRoutine);

        if (evolutionSprites != null && evolutionSprites.Length > 0)
        {
            if (spriteRenderer != null) spriteRenderer.sprite = evolutionSprites[0];
        }

        activeScaleRoutine = StartCoroutine(AnimateReturnToOriginal(0.1f));
    }

    // Force Reset to Original Scale safely
    public void ResetScale()
    {
        StopIdleWiggle();
        if (activeScaleRoutine != null) StopCoroutine(activeScaleRoutine);
        
        transform.localScale = originalScale;
    }

    private IEnumerator AnimateReturnToOriginal(float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (this == null || gameObject == null) yield break;

            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, originalScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (this != null && gameObject != null)
        {
            transform.localScale = originalScale;
        }
        activeScaleRoutine = null;
    }

    // --- MOUSE HOVER EFFECTS ---
    private void OnMouseEnter()
    {
        if (BoardManager.Instance != null && BoardManager.Instance.IsProcessing) return;

        // Scale up slightly relative to originalScale
        SetScaleAnimated(originalScale * 1.12f, 0.1f);
    }

    private void OnMouseExit()
    {
        if (BoardManager.Instance != null && BoardManager.Instance.IsProcessing) return;

        // Return strictly back to originalScale
        SetScaleAnimated(originalScale, 0.1f);
    }

    public void SetScaleAnimated(Vector3 targetScale, float duration = 0.1f)
    {
        if (activeScaleRoutine != null) StopCoroutine(activeScaleRoutine);
        activeScaleRoutine = StartCoroutine(ScaleToRoutine(targetScale, duration));
    }

    private IEnumerator ScaleToRoutine(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (this == null || gameObject == null) yield break;

            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (this != null && gameObject != null)
        {
            transform.localScale = targetScale;
        }
        activeScaleRoutine = null;
    }

    // --- IDLE WIGGLE ---
    public void StartIdleWiggle()
    {
        if (wiggleCoroutine == null && activeScaleRoutine == null)
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
        }
        transform.localScale = originalScale; 
    }

    private IEnumerator WiggleRoutine()
    {
        float elapsed = 0f;
        float duration = 1.0f;

        // Lock baseline scale locally to prevent compounding scaling issues
        Vector3 baseScale = originalScale;

        while (elapsed < duration)
        {
            if (this == null || gameObject == null) yield break;

            float progress = elapsed / duration;
            float pulse = Mathf.Sin(progress * Mathf.PI * 2f);
            
            // Subtle breathing/pulse effect (3%)
            float scaleX = baseScale.x + (pulse * 0.03f);
            float scaleY = baseScale.y + (pulse * 0.03f);

            transform.localScale = new Vector3(scaleX, scaleY, baseScale.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (this != null && gameObject != null)
        {
            transform.localScale = baseScale;
        }
        wiggleCoroutine = null;
    }

    public IEnumerator ShakeJelly(float duration = 0.12f, float magnitude = 0.04f)
    {
        Vector3 basePos = transform.position;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Abort coroutine instantly if destroyed mid-air
            if (this == null || gameObject == null) yield break;

            float offsetX = Random.Range(-magnitude, magnitude);
            float offsetY = Random.Range(-magnitude, magnitude);
            transform.position = basePos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (this != null && gameObject != null)
        {
            transform.position = basePos;
        }
    }

    public IEnumerator AnimateMergeTo(Vector3 centerTarget, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (this == null || gameObject == null) yield break;

        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (this == null || gameObject == null) yield break;

            float t = elapsed / duration;
            t = t * t;

            transform.position = Vector3.Lerp(startPos, centerTarget, t);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (this != null && gameObject != null)
        {
            transform.position = centerTarget;
            transform.localScale = Vector3.zero;
        }
    }
}