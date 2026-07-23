using System.Collections;
using UnityEngine;
using TMPro;

public class ComboUIManager : MonoBehaviour
{
    public static ComboUIManager Instance;

    [Header("UI Text References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI comboText;

    [Header("Core Evolution Colors")]
    public Color normalColor = Color.white;
    public Color aliveCoreColor = new Color(0.3f, 0.9f, 0.3f);     // x10 Alive Core (Green)
    public Color perfectCoreColor = new Color(0.2f, 0.7f, 1f);     // x20 Perfect Core (Blue)
    public Color legendaryCoreColor = new Color(0.8f, 0.3f, 1f);   // x30 Legendary Core (Purple)
    public Color immortalCoreColor = new Color(1f, 0.5f, 0f);      // x50 Immortal Core (Orange/Gold)

    private Coroutine comboPunchCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void UpdateTimerUI(float remainingTime)
    {
        if (timerText == null) return;

        int seconds = Mathf.CeilToInt(remainingTime);
        timerText.text = seconds.ToString();

        // If the remaining time is 5 seconds or less, change the text color to red and apply a shake effect
        if (remainingTime <= 5f)
        {
            timerText.color = Color.red;
            float shakeX = Random.Range(-2f, 2f);
            float shakeY = Random.Range(-2f, 2f);
            timerText.rectTransform.anchoredPosition += new Vector2(shakeX, shakeY);
        }
        else
        {
            timerText.color = Color.white;
        }
    }

    // Combos UI Update and Punch Animation
    public void UpdateComboUI(int currentCombo)
    {
        if (comboText == null) return;

        comboText.text = $"COMBO x{currentCombo}";

        // Combo Core Evolution Colors (According to Documentation)[cite: 1]
        Color targetColor = normalColor;
        if (currentCombo >= 50) targetColor = immortalCoreColor;      // x50+ Immortal Core[cite: 1]
        else if (currentCombo >= 30) targetColor = legendaryCoreColor; // x30+ Legendary Core[cite: 1]
        else if (currentCombo >= 20) targetColor = perfectCoreColor;   // x20+ Perfect Core[cite: 1]
        else if (currentCombo >= 10) targetColor = aliveCoreColor;     // x10+ Alive Core[cite: 1]

        comboText.color = targetColor;

        // Punch Animation for Combo Text
        if (comboPunchCoroutine != null) StopCoroutine(comboPunchCoroutine);
        comboPunchCoroutine = StartCoroutine(AnimateComboPunch());
    }

    private IEnumerator AnimateComboPunch()
    {
        RectTransform rect = comboText.rectTransform;
        Vector3 defaultScale = Vector3.one;
        Vector3 targetPunchScale = new Vector3(1.35f, 1.35f, 1f); 

        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            rect.localScale = Vector3.Lerp(defaultScale, targetPunchScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            rect.localScale = Vector3.Lerp(targetPunchScale, defaultScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.localScale = defaultScale;
    }
}