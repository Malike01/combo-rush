using System.Collections;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;   // Text (TMP) inside TopPanel
    public TextMeshProUGUI comboText;   // Text (TMP) inside BottomPanel
    public TextMeshProUGUI endComboText; // Text (TMP) for the end-of-level summary

    [Header("Score Settings")]
    public int baseJellyScore = 100;

    private int currentScore = 0;
    private int currentCombo = 1;

    // To track animation statuses
    private Coroutine scoreAnimCoroutine;
    private Coroutine comboAnimCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddScore(int jellyCount)
    {
        if (jellyCount < 3) return;

        float lengthBonus = 1f + ((jellyCount - 3) * 0.2f);
        int gainedScore = Mathf.RoundToInt((jellyCount * baseJellyScore) * lengthBonus * currentCombo);

        currentScore += gainedScore;
        currentCombo++;

        UpdateUI();

        // Trigger the punch scale animation for the score and combo text
        TriggerPunchAnimation();
    }

    public void ResetCombo()
    {
        currentCombo = 1;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null) 
        {
            scoreText.text = currentScore.ToString();
        }

        if (comboText != null) 
        {
            // Apply the evolution naming rules based on the current combo
            string evolutionName = GetComboEvolutionName(currentCombo);
            
            if (currentCombo < 5)
            {
                // Default text for low combos
                comboText.text = $"Combo x{currentCombo}";
            }
            else
            {
                // Evolved text for high combos
                comboText.text = $"{evolutionName} x{currentCombo}";
            }
        }
    }

    // --- COMBO EVOLUTION RULES ---
    /// <summary>
    /// Returns the specific naming tier based on the current combo count.
    /// Checked in descending order to ensure the highest threshold is met first.
    /// </summary>
    private string GetComboEvolutionName(int combo)
    {
        if (combo >= 100) return "Unbreakable Core";
        if (combo >= 75)  return "Divine Core";
        if (combo >= 50)  return "Immortal Core";
        if (combo >= 30)  return "Legendary Core";
        if (combo >= 20)  return "Perfect Core";
        if (combo >= 10)  return "Alive Core";
        if (combo >= 5)   return "Combo Core";

        return "Combo"; // Fallback
    }

    // --- ANIMATION SECTION ---

    private void TriggerPunchAnimation()
    {
        if (scoreText != null)
        {
            if (scoreAnimCoroutine != null) StopCoroutine(scoreAnimCoroutine);
            scoreAnimCoroutine = StartCoroutine(PunchScaleRoutine(scoreText.transform, 1.25f, 0.15f));
        }

        if (comboText != null)
        {
            if (comboAnimCoroutine != null) StopCoroutine(comboAnimCoroutine);
            comboAnimCoroutine = StartCoroutine(PunchScaleRoutine(comboText.transform, 1.35f, 0.15f));
        }
    }

    /// <summary>
    /// Quickly scales the object up to the target size and smoothly returns it to its original size.
    /// </summary>
    private IEnumerator PunchScaleRoutine(Transform targetTransform, float scaleFactor, float duration)
    {
        Vector3 originalScale = Vector3.one;
        Vector3 punchScale = originalScale * scaleFactor;

        float halfDuration = duration / 2f;
        float elapsed = 0f;

        // 1. Growth Phase
        while (elapsed < halfDuration)
        {
            targetTransform.localScale = Vector3.Lerp(originalScale, punchScale, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        targetTransform.localScale = punchScale;

        // 2. Shrinking / Return to Original State Phase
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            targetTransform.localScale = Vector3.Lerp(punchScale, originalScale, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        targetTransform.localScale = originalScale;
    }
}