using System.Collections;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;      // Text (TMP) inside TopPanel
    public TextMeshProUGUI comboText;      // Text (TMP) inside BottomPanel
    public TextMeshProUGUI comboTitleText; // e.g., "Good Run", "Legendary Run"
    public TextMeshProUGUI endComboText;   // Text (TMP) for the end-of-level summary

    [Header("Target Visual Settings")]
    public Color defaultColor = new Color(1f, 0.95f, 0.65f, 1f);
    public VertexGradient defaultGradient = new VertexGradient(
        new Color(0.9f, 0.1f, 0.4f, 1f),   
        new Color(0.95f, 0.35f, 0.0f, 1f), 
        new Color(1f, 0.75f, 0.0f, 1f),   
        new Color(1f, 0.85f, 0.1f, 1f)     
    );

    public Color successColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);

    // --- PLAYER DATA ---
    public int currentScore { get; private set; } = 0; 
    private int currentCombo = 1;                      
    public int maxComboReached { get; private set; } = 1; // Tracked to show on the Win/Lose screen

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
        ResetLevelData();
    }

    // Resets all stats and colors when a new level starts
    public void ResetLevelData()
    {
        currentScore = 0;
        currentCombo = 1;
        maxComboReached = 1;
        
        if (scoreText != null) ApplyDefaultGradient(scoreText);
        if (comboText != null) ApplyDefaultGradient(comboText);
        if (comboTitleText != null) comboTitleText.text = "";

        UpdateUI();
    }

    public void AddScore(int totalCoinValue, int coinCount) 
    {
        if (coinCount < 4) return; 

        float lengthBonus = 1f;
        if (coinCount > 3)
        {
            lengthBonus += (coinCount - 3) * 0.5f; 
        }

        // Kombo 1 = 1.0x | Kombo 2 = 1.2x | Kombo 3 = 1.4x
        float comboMultiplier = 1f + ((currentCombo - 1) * 0.2f); 
        
        int gainedScore = Mathf.RoundToInt(totalCoinValue * lengthBonus * comboMultiplier); 

        currentScore += gainedScore; 
        currentCombo++;              

        if (currentCombo > maxComboReached)
        {
            maxComboReached = currentCombo;
        }

        UpdateUI(); 
        TriggerPunchAnimation();
    }

    public void ResetCombo()
    {
        currentCombo = 1; 
        UpdateUI(); 
    }

    private void UpdateUI()
    {
        int targetScore = 0;
        
        if (BoardManager.Instance != null && BoardManager.Instance.currentLevel != null)
        {
            targetScore = BoardManager.Instance.currentLevel.targetScore;
        }

        // 1. Update Score UI
        if (scoreText != null) 
        {
            scoreText.text = currentScore.ToString(); 
            
            // Turn green if the target score is reached
            if (targetScore > 0 && currentScore >= targetScore)
            {
                ApplySolidColor(scoreText, successColor);
            }
            else
            {
                ApplyDefaultGradient(scoreText);
            }
        }

        // 2. Update Combo UI (No longer tied to a level win condition)
        if (comboText != null) 
        {
            string evolutionName = GetComboEvolutionName(currentCombo); 
            
            if (currentCombo < 5) 
            {
                comboText.text = $"Combo x{currentCombo}"; 
            }
            else 
            {
                comboText.text = $"{evolutionName}"; 
            }

            // Always use default gradient, as combo is just a visual/score multiplier now
            ApplyDefaultGradient(comboText);
        }

        // 3. Update Combo Title (e.g., "Good Run", "Legendary Run")
        if (comboTitleText != null)
        {
            comboTitleText.text = GetComboTitleName(currentCombo);
        }
    }

    // --- COMBO EVOLUTION RULES (Material/Stone Evolution) ---
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

    // --- COMBO TITLE RULES (Player Progression Title) ---
    private string GetComboTitleName(int combo)
    {
        if (combo >= 50) return "Immortal Run";
        if (combo >= 40) return "Legendary Run";
        if (combo >= 30) return "Amazing Run";
        if (combo >= 20) return "Great Run";
        if (combo >= 10) return "Good Run";
        
        return ""; // Do not show title if combo is less than 10
    }

    private void ApplyDefaultGradient(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;

        textComponent.color = defaultColor;
        textComponent.colorGradient = defaultGradient;
    }

    private void ApplySolidColor(TextMeshProUGUI textComponent, Color color)
    {
        if (textComponent == null) return;

        textComponent.color = color;
        textComponent.colorGradient = new VertexGradient(color, color, color, color);
    }

    // --- ANIMATION SECTION ---

    private void TriggerPunchAnimation()
    {
        if (scoreText != null)
        {
            if (scoreAnimCoroutine != null) StopCoroutine(scoreAnimCoroutine);
            scoreAnimCoroutine = StartCoroutine(PulseTextColorRoutine(scoreText, new Color(1f, 0.95f, 0.25f, 1f), 0.16f));
        }

        if (comboText != null)
        {
            if (comboAnimCoroutine != null) StopCoroutine(comboAnimCoroutine);
            comboAnimCoroutine = StartCoroutine(PulseTextColorRoutine(comboText, new Color(1f, 0.95f, 0.25f, 1f), 0.16f));
        }
        
        if (comboTitleText != null && !string.IsNullOrEmpty(comboTitleText.text))
        {
            StartCoroutine(PulseTextColorRoutine(comboTitleText, new Color(0.95f, 0.75f, 1f, 1f), 0.16f));
        }
    }

    private IEnumerator PulseTextColorRoutine(TextMeshProUGUI textComponent, Color pulseColor, float duration)
    {
        if (textComponent == null) yield break;

        float halfDuration = duration / 2f;
        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            if (textComponent == null) yield break;

            float t = elapsed / halfDuration;
            textComponent.color = Color.Lerp(textComponent.color, pulseColor, t);
            textComponent.colorGradient = new VertexGradient(
                Color.Lerp(textComponent.color, pulseColor, t),
                Color.Lerp(textComponent.color, pulseColor, t),
                Color.Lerp(textComponent.color, pulseColor, t),
                Color.Lerp(textComponent.color, pulseColor, t));

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (textComponent != null)
        {
            RestoreTextVisualState(textComponent);
        }
    }

    private void RestoreTextVisualState(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;

        if (textComponent == scoreText)
        {
            if (TargetScoreReached())
            {
                ApplySolidColor(textComponent, successColor);
            }
            else
            {
                ApplyDefaultGradient(textComponent);
            }
        }
        else if (textComponent == comboText)
        {
            // Combo is no longer tied to level requirements, always return to default gradient
            ApplyDefaultGradient(textComponent);
        }
        else if (textComponent == comboTitleText)
        {
            textComponent.color = new Color(1f, 0.95f, 0.65f, 1f);
        }
    }

    private bool TargetScoreReached()
    {
        int targetScore = 0;
        if (BoardManager.Instance != null && BoardManager.Instance.currentLevel != null)
        {
            targetScore = BoardManager.Instance.currentLevel.targetScore;
        }

        return targetScore > 0 && currentScore >= targetScore;
    }
}