using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ComboTimerManager : MonoBehaviour
{
    public static ComboTimerManager Instance;

    [Header("Timer Settings")]
    [Tooltip("Maximum fill cap for the timer bar (in seconds)")]
    public float maxTime = 30.0f;
    
    [Tooltip("Time added on every combo hit instead of full reset (in seconds)")]
    public float timeBonusPerCombo = 0.02f;

    private float currentTime;
    private bool isTimerRunning = false;
    public bool IsLevelFinished { get; private set; } = false;

    [Header("UI References")]
    public Image timerFillImage;
    public RectTransform timerUIRoot; 

    [Header("Colors")]
    public Color safeColor = Color.white;
    public Color warningColor = new Color(1f, 0.5f, 0f);
    public Color panicColor = Color.red;

    private Coroutine panicRoutine;

    [Header("Droplet Effects")]
    public RectTransform dropletPoint;
    public ParticleSystem dropletParticles;
    private float barWidth;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (timerFillImage != null)
        {
            timerFillImage.fillAmount = 1f;
            barWidth = timerFillImage.rectTransform.rect.width;
        }
        
        currentTime = maxTime;
    }

    private void Update()
    {
        if (!isTimerRunning) return;

        currentTime -= Time.deltaTime;
        float fillRatio = Mathf.Clamp01(currentTime / maxTime);
        
        if (timerFillImage != null)
        {
            timerFillImage.fillAmount = fillRatio;
        }

        if (dropletPoint != null)
        {
            float currentWidth = barWidth * fillRatio;
            dropletPoint.anchoredPosition = new Vector2(currentWidth, dropletPoint.anchoredPosition.y);
        }

        UpdateTimerVisuals();

        if (currentTime <= 0)
        {
            TimeOut();
        }
    }

    public void ResetTimerFull()
    {
        StopTimer();

        IsLevelFinished = false;
        currentTime = maxTime;
        isTimerRunning = true; 

        if (timerFillImage != null)
        {
            timerFillImage.enabled = true;
            timerFillImage.fillAmount = 1f;
            timerFillImage.color = safeColor;
        }

        if (timerUIRoot != null)
        {
            timerUIRoot.localScale = Vector3.one;
        }

        if (dropletPoint != null)
        {
            if (barWidth <= 0 && timerFillImage != null)
            {
                barWidth = timerFillImage.rectTransform.rect.width;
            }
            dropletPoint.anchoredPosition = new Vector2(barWidth, dropletPoint.anchoredPosition.y);
        }

        if (dropletParticles != null)
        {
            dropletParticles.Stop();
        }
    }
    /// <summary>
    /// Starts the timer or adds extra bonus time to the active timer without resetting completely.
    /// </summary>
    public void StartOrResetTimer()
    {
        if (IsLevelFinished) return;

        if (!isTimerRunning)
        {
            // First hit: initialize with full or default base time and start running
            currentTime = maxTime;
            isTimerRunning = true;
        }
        else
        {
            // Add bonus time incrementally on each combo, capped at maxTime
            currentTime = Mathf.Min(currentTime + timeBonusPerCombo, maxTime);
        }

        // If time recovers above the panic threshold, stop panic state safely
        if (currentTime > 2.0f && panicRoutine != null)
        {
            StopCoroutine(panicRoutine);
            panicRoutine = null;
            if (timerUIRoot != null) timerUIRoot.localScale = Vector3.one;
            if (timerFillImage != null)
            {
                timerFillImage.enabled = true;
                timerFillImage.color = safeColor;
            }
            if (AudioManager.Instance != null) AudioManager.Instance.StopPanicSound();
        }

        StartCoroutine(PopTimerUI());

        if (dropletParticles != null && !dropletParticles.isPlaying) 
        {
            dropletParticles.Play();
        }
    }

    public void StopTimer()
    {
        isTimerRunning = false;
        
        if (panicRoutine != null)
        {
            StopCoroutine(panicRoutine);
            panicRoutine = null;
        }
        
        if (AudioManager.Instance != null) AudioManager.Instance.StopPanicSound();
        
        if (dropletParticles != null)
        {
            dropletParticles.Stop();
        }
    }

    private void UpdateTimerVisuals()
    {
        if (currentTime <= 2.0f)
        {
            if (panicRoutine == null && isTimerRunning)
            {
                panicRoutine = StartCoroutine(PanicShakeRoutine());
                
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.StartPanicSound();
                }
            }
        }
        else
        {
            if (timerFillImage != null)
            {
                timerFillImage.color = safeColor;
            }
        }
    }

    private void TimeOut()
    {
        if (IsLevelFinished) return;

        IsLevelFinished = true;
        isTimerRunning = false;
        currentTime = 0;
        
        if (timerFillImage != null)
        {
            timerFillImage.fillAmount = 0;
        }

        StopTimer();
        
        if (timerUIRoot != null)
        {
            timerUIRoot.localScale = Vector3.one;
        }

       if (LevelEndManager.Instance != null)
       {
          LevelEndManager.Instance.ShowLevelEndScreen();
       }

        Debug.Log("TIME OVER! GAME OVER.");
    }

    private IEnumerator PopTimerUI()
    {
        if (timerUIRoot == null) yield break;

        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = originalScale * 1.2f;
        float elapsed = 0f;
        float duration = 0.1f;

        while (elapsed < duration)
        {
            timerUIRoot.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        elapsed = 0f;
        while (elapsed < duration)
        {
            timerUIRoot.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        timerUIRoot.localScale = originalScale;
    }

    private IEnumerator PanicShakeRoutine()
    {
        if (timerUIRoot == null) yield break;

        Vector3 originalPos = timerUIRoot.anchoredPosition;
        
        while (isTimerRunning && currentTime <= 2.0f)
        {
            float offsetX = Random.Range(-5f, 5f);
            float offsetY = Random.Range(-5f, 5f);
            
            timerUIRoot.anchoredPosition = originalPos + new Vector3(offsetX, offsetY, 0);
            
            if (timerFillImage != null)
            {
                timerFillImage.enabled = !timerFillImage.enabled;
            }

            yield return new WaitForSeconds(0.05f);
        }

        timerUIRoot.anchoredPosition = originalPos;
        if (timerFillImage != null)
        {
            timerFillImage.enabled = true;
        }
        panicRoutine = null;
    }
}