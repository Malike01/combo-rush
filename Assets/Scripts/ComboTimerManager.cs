using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ComboTimerManager : MonoBehaviour
{
    public static ComboTimerManager Instance;

    [Header("Timer Settings")]
    public float maxTime = 5.0f;
    private float currentTime;
    private bool isTimerRunning = false;

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
        timerFillImage.fillAmount = 1f;
        barWidth = timerFillImage.rectTransform.rect.width;
    }

   private void Update()
    {
        if (!isTimerRunning) return;

        currentTime -= Time.deltaTime;
        float fillRatio = currentTime / maxTime;
        timerFillImage.fillAmount = fillRatio;

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

    public void StartOrResetTimer()
    {
        currentTime = maxTime;
        isTimerRunning = true;
        
        if (panicRoutine != null) StopCoroutine(panicRoutine);
        timerUIRoot.localScale = Vector3.one;
        timerFillImage.color = safeColor;

        StartCoroutine(PopTimerUI());

        if (dropletParticles != null && !dropletParticles.isPlaying) 
        {
            dropletParticles.Play();
        }
    }

    public void StopTimer()
    {
        if (panicRoutine != null) StopCoroutine(panicRoutine);
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
                if (panicRoutine == null)
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
                timerFillImage.color = Color.white;
            }
        }

    private void TimeOut()
    {
        isTimerRunning = false;
        currentTime = 0;
        timerFillImage.fillAmount = 0;

        StopTimer();
        timerUIRoot.localScale = Vector3.one;

        Debug.Log("SÜRE BİTTİ! OYUN SONU.");
        
    }


    private IEnumerator PopTimerUI()
    {
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
        Vector3 originalPos = timerUIRoot.anchoredPosition;
        
        while (isTimerRunning && currentTime <= 2.0f)
        {
            float offsetX = Random.Range(-5f, 5f);
            float offsetY = Random.Range(-5f, 5f);
            
            timerUIRoot.anchoredPosition = originalPos + new Vector3(offsetX, offsetY, 0);
            
            timerFillImage.enabled = !timerFillImage.enabled;

            yield return new WaitForSeconds(0.05f);
        }

        timerUIRoot.anchoredPosition = originalPos;
        timerFillImage.enabled = true;
        panicRoutine = null;
    }
}