using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class Coin : MonoBehaviour
{
    [Header("Grid Coordinates")]
    public int x;
    public int y;

    [Header("Coin Data")]
    public int colorIndex;
    public int pointValue; 
    public bool isMysteryCoin;
    
    [Header("Combo Progression Sprites")]
    public Sprite[] progressionSprites; 
    
    // Shortcut for BoardManager algorithms to access color seamlessly
    public int themeIndex => colorIndex; 

    private SpriteRenderer spriteRenderer;
    private GameObject selectionGlowObject;
    private SpriteRenderer selectionGlowRenderer;
    private GameObject mysteryValueObject;
    private TextMeshPro mysteryValueText;
    private static readonly Color selectionGlowColor = new Color(1f, 0.4f, 0.85f, 0.45f);
    private const float selectionGlowScaleFactor = 1.2f;
    
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

    public void SetupCoin(Sprite sprite, int index, int points, bool isMystery = false)
    {
        InitScale();
        colorIndex = index;
        pointValue = points; 
        isMysteryCoin = isMystery;

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }

        if (!isMysteryCoin)
        {
            HideMysteryValueDisplay();
        }

        UpdateSelectionGlowSprite();
    }

    public void UpdateFaceByCombo(int comboLevel)
    {
        // Eğer dizi atanmamışsa veya boşsa işlem yapma
        if (progressionSprites == null || progressionSprites.Length == 0) return;

        int safeIndex = Mathf.Clamp(comboLevel, 0, progressionSprites.Length - 1);

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = progressionSprites[safeIndex];
            
            UpdateSelectionGlowSprite();
        }
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
        DisableSelectionGlow();

        if (activeScaleRoutine != null) StopCoroutine(activeScaleRoutine);

        activeScaleRoutine = StartCoroutine(AnimateReturnToOriginal(0.1f));
    }

    // Force Reset to Original Scale safely
    public void ResetScale()
    {
        StopIdleWiggle();
        DisableSelectionGlow();
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

    public void EnableSelectionGlow()
    {
        if (selectionGlowObject == null)
        {
            CreateSelectionGlow();
        }

        if (selectionGlowObject != null)
        {
            UpdateSelectionGlowSprite();
            selectionGlowObject.SetActive(true);
        }
    }

    public void DisableSelectionGlow()
    {
        if (selectionGlowObject != null)
        {
            selectionGlowObject.SetActive(false);
        }
    }

    private void CreateSelectionGlow()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;

        selectionGlowObject = new GameObject("SelectionGlow");
        selectionGlowObject.transform.SetParent(transform);
        selectionGlowObject.transform.localPosition = Vector3.zero;
        selectionGlowObject.transform.localRotation = Quaternion.identity;
        selectionGlowObject.transform.localScale = Vector3.one * selectionGlowScaleFactor;

        selectionGlowRenderer = selectionGlowObject.AddComponent<SpriteRenderer>();
        selectionGlowRenderer.sprite = spriteRenderer.sprite;
        selectionGlowRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
        selectionGlowRenderer.color = selectionGlowColor;
        selectionGlowRenderer.material = spriteRenderer.sharedMaterial;
        selectionGlowRenderer.maskInteraction = UnityEngine.SpriteMaskInteraction.None;
        selectionGlowRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        selectionGlowRenderer.receiveShadows = false;
    }

    private void UpdateSelectionGlowSprite()
    {
        if (selectionGlowRenderer == null || spriteRenderer == null) return;
        if (selectionGlowRenderer.sprite != spriteRenderer.sprite)
        {
            selectionGlowRenderer.sprite = spriteRenderer.sprite;
        }
    }

    private void EnsureMysteryValueDisplay()
    {
        if (mysteryValueObject != null) return;

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        mysteryValueObject = new GameObject("MysteryValueText");
        mysteryValueObject.transform.SetParent(transform, false);
        mysteryValueObject.transform.localPosition = Vector3.zero;
        mysteryValueObject.transform.localRotation = Quaternion.identity;
        mysteryValueObject.transform.localScale = Vector3.one;

        mysteryValueText = mysteryValueObject.AddComponent<TextMeshPro>();
        mysteryValueText.alignment = TextAlignmentOptions.Center;
        mysteryValueText.raycastTarget = false;
        mysteryValueText.enableAutoSizing = false;
        mysteryValueText.fontSize = 20f;
        mysteryValueText.fontSizeMin = 6f;
        mysteryValueText.fontSizeMax = 12f;
        mysteryValueText.color = Color.white;
        mysteryValueText.colorGradient = new VertexGradient(
            new Color(1f, 0.95f, 0.35f, 1f),
            new Color(1f, 0.95f, 0.35f, 1f),
            new Color(1f, 0.95f, 0.35f, 1f),
            new Color(1f, 0.95f, 0.35f, 1f));
        mysteryValueText.sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder + 5 : 100;
        mysteryValueText.text = string.Empty;
    }

    private void ShowMysteryValueDisplay(string value)
    {
        EnsureMysteryValueDisplay();
        if (mysteryValueObject != null)
        {
            mysteryValueObject.SetActive(true);
        }

        if (mysteryValueText != null)
        {
            mysteryValueText.text = value;
            mysteryValueText.color = new Color(1f, 0.95f, 0.35f, 1f);
        }
    }

    private void HideMysteryValueDisplay()
    {
        if (mysteryValueObject != null)
        {
            mysteryValueObject.SetActive(false);
        }
    }

    private void SetMysteryValueAlpha(float alpha)
    {
        if (mysteryValueText == null) return;

        Color color = mysteryValueText.color;
        color.a = alpha;
        mysteryValueText.color = color;
    }

    private IEnumerator FadeMysteryValueRoutine(float targetAlpha, float duration)
    {
        if (mysteryValueText == null) yield break;

        float startAlpha = mysteryValueText.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (this == null || gameObject == null) yield break;

            float t = elapsed / duration;
            Color color = mysteryValueText.color;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            mysteryValueText.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (mysteryValueText != null)
        {
            Color finalColor = mysteryValueText.color;
            finalColor.a = targetAlpha;
            mysteryValueText.color = finalColor;
        }
    }

    public IEnumerator RevealMysteryCoinRoutine(Vector3 targetPosition, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        if (this == null || gameObject == null) yield break;

        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        float duration = 0.9f;
        float elapsed = 0f;

        EnsureMysteryValueDisplay();
        ShowMysteryValueDisplay(pointValue.ToString());
        SetMysteryValueAlpha(0f);

        while (elapsed < duration)
        {
            if (this == null || gameObject == null) yield break;

            float t = elapsed / duration;
            float eased = Mathf.SmoothStep(0f, 1f, t);
            float spinAngle = Mathf.Lerp(0f, 720f, eased);

            transform.position = Vector3.Lerp(startPos, targetPosition, eased);
            transform.localScale = Vector3.Lerp(startScale, originalScale * 2.8f, eased);
            transform.rotation = Quaternion.Euler(0f, 0f, spinAngle);

            float alphaT = Mathf.Clamp01((t - 0.6f) / 0.4f);
            SetMysteryValueAlpha(alphaT);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (this != null && gameObject != null)
        {
            transform.position = targetPosition;
            transform.localScale = originalScale * 2.8f;
            transform.rotation = Quaternion.identity;
            SetMysteryValueAlpha(1f);
        }

        yield return new WaitForSeconds(0.65f);

        if (this != null && gameObject != null)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.7f);
            }

            StartCoroutine(FadeMysteryValueRoutine(0f, 0.25f));
            yield return new WaitForSeconds(0.25f);

            if (this != null && gameObject != null)
            {
                Destroy(gameObject);
            }
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

    public void DropToTargetPosition(Vector3 targetPosition, float duration = 0.4f)
    {
        StopIdleWiggle();
        if (activeScaleRoutine != null) StopCoroutine(activeScaleRoutine);
        transform.DOKill(); 

        transform.localScale = originalScale;

        transform.DOMove(targetPosition, duration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => 
            {
                Sequence squashSeq = DOTween.Sequence();
                
                squashSeq.Append(transform.DOScale(new Vector3(originalScale.x * 1.25f, originalScale.y * 0.75f, originalScale.z), 0.1f));
                
                squashSeq.Append(transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutBack));
            });
    }
}