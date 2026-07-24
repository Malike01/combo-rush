using System.Collections;
using UnityEngine;

public class Jelly : MonoBehaviour
{
    [Header("Grid Coordinates")]
    public int x;
    public int y;

    [Header("Color Index")]
    public int colorIndex;
    
    // BoardManager algoritmasının sorunsuz erişebilmesi için kısayol
    public int themeIndex => colorIndex; 

    private Sprite[] evolutionSprites;
    private SpriteRenderer spriteRenderer;
    
    private Vector3 originalScale;
    private bool isScaleInitialized = false;

    private Coroutine activeScaleRoutine;
    private Coroutine wiggleCoroutine;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        InitScale();
    }

    private void InitScale()
    {
        if (!isScaleInitialized)
        {
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
        Vector3 targetScale = new Vector3(originalScale.x * boostMultiplier, originalScale.y * (boostMultiplier - 0.05f), originalScale.z);
        float duration = 0.08f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = targetScale;
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

    private IEnumerator AnimateReturnToOriginal(float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, originalScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
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
        while (true)
        {
            float elapsed = 0f;
            float duration = 1.5f; 

            while (elapsed < duration)
            {
                float progress = elapsed / duration;
                float pulse = Mathf.Sin(progress * Mathf.PI * 2f);
                
                float scaleX = originalScale.x + (pulse * 0.025f);
                float scaleY = originalScale.y - (pulse * 0.025f);

                transform.localScale = new Vector3(scaleX, scaleY, originalScale.z);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localScale = originalScale;
            yield return null;
        }
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