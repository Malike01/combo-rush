using System.Collections;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;      // Text (TMP) inside TopPanel[cite: 2]
    public TextMeshProUGUI comboText;      // Text (TMP) inside BottomPanel[cite: 2]
    public TextMeshProUGUI comboTitleText; // YENI: "Good Run", "Legendary Run"
    public TextMeshProUGUI endComboText;   // Text (TMP) for the end-of-level summary[cite: 2]

    [Header("Target Visual Settings")]
    public Color defaultColor = new Color(1f, 0.95f, 0.65f, 1f);
    public VertexGradient defaultGradient = new VertexGradient(
        new Color(0.9f, 0.1f, 0.4f, 1f),   
        new Color(0.95f, 0.35f, 0.0f, 1f), 
        new Color(1f, 0.75f, 0.0f, 1f),   
        new Color(1f, 0.85f, 0.1f, 1f)     
    );

    public Color successColor = new Color(0.28f, 1f, 0.44f, 1f);

    // --- OYUNCU VERILERI ---
    public int currentScore { get; private set; } = 0; // Win/Lose kontrolu icin disariya acik, ama sadece buradan degisebilir[cite: 2]
    private int currentCombo = 1;                      //[cite: 2]
    public int maxComboReached { get; private set; } = 1; // YENI: Sure bittiginde hedef kombo icin ulasilan "en yuksek" komboyu baz alacagiz.

    // To track animation statuses
    private Coroutine scoreAnimCoroutine;  //[cite: 2]
    private Coroutine comboAnimCoroutine;  //[cite: 2]

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); //[cite: 2]
        }
    }

    private void Start()
    {
        ResetLevelData();
        UpdateUI(); //[cite: 2]
    }

    // Yeni level basladiginda tum statlari ve renkleri sifirlar
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
        if (coinCount < 4) return; //[cite: 2]

        float lengthBonus = 1f + ((coinCount - 4) * 0.2f); //[cite: 2]
        
        int gainedScore = Mathf.RoundToInt(totalCoinValue * lengthBonus * currentCombo); //[cite: 2]

        currentScore += gainedScore; //[cite: 2]
        currentCombo++;              //[cite: 2]

        // Rekor komboyu guncelle
        if (currentCombo > maxComboReached)
        {
            maxComboReached = currentCombo;
        }

        UpdateUI(); //[cite: 2]
        TriggerPunchAnimation(); //[cite: 2]
    }

    public void ResetCombo()
    {
        currentCombo = 1; //[cite: 2]
        UpdateUI(); //[cite: 2]
    }

    private void UpdateUI()
    {
        int targetScore = 0;
        int targetCombo = 0;
        
        if (BoardManager.Instance != null && BoardManager.Instance.currentLevel != null)
        {
            targetScore = BoardManager.Instance.currentLevel.targetScore;
            targetCombo = BoardManager.Instance.currentLevel.targetCombo;
        }

        if (scoreText != null) 
        {
            scoreText.text = currentScore.ToString(); //[cite: 2]
            
            if (targetScore > 0 && currentScore >= targetScore)
            {
                ApplySolidColor(scoreText, successColor);
            }
            else
            {
                ApplyDefaultGradient(scoreText);
            }
        }

        if (comboText != null) 
        {
            // Apply the evolution naming rules based on the current combo[cite: 2]
            string evolutionName = GetComboEvolutionName(currentCombo); //[cite: 2]
            
            if (currentCombo < 5) //[cite: 2]
            {
                // Default text for low combos[cite: 2]
                comboText.text = $"Combo x{currentCombo}"; //[cite: 2]
            }
            else //[cite: 2]
            {
                // Evolved text for high combos[cite: 2]
                comboText.text = $"{evolutionName}"; //[cite: 2]
            }

            // Yapilan en yuksek kombo (maxComboReached) hedefe ulastiysa yesil yap
            if (targetCombo > 0 && maxComboReached >= targetCombo)
            {
                ApplySolidColor(comboText, successColor);
            }
            else
            {
                ApplyDefaultGradient(comboText);
            }
        }

        // 4. Yeni Unvan Guncellemesi (Good Run, Legendary Run vb.)
        if (comboTitleText != null)
        {
            comboTitleText.text = GetComboTitleName(currentCombo);
        }
    }

    // --- COMBO EVOLUTION RULES (Materyal/Tas Evrimi) ---
    private string GetComboEvolutionName(int combo)
    {
        if (combo >= 100) return "Unbreakable Core"; //[cite: 2]
        if (combo >= 75)  return "Divine Core";      //[cite: 2]
        if (combo >= 50)  return "Immortal Core";    //[cite: 2]
        if (combo >= 30)  return "Legendary Core";   //[cite: 2]
        if (combo >= 20)  return "Perfect Core";     //[cite: 2]
        if (combo >= 10)  return "Alive Core";       //[cite: 2]
        if (combo >= 5)   return "Combo Core";       //[cite: 2]

        return "Combo"; // Fallback[cite: 2]
    }

    // --- YENI: COMBO TITLE RULES (Oyuncu Gidisat Unvani) ---
    private string GetComboTitleName(int combo)
    {
        if (combo >= 50) return "Immortal Run";
        if (combo >= 40) return "Legendary Run";
        if (combo >= 30) return "Amazing Run";
        if (combo >= 20) return "Great Run";
        if (combo >= 10) return "Good Run";
        
        return ""; // 10'dan kucukse ekranda unvan gorunmez
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
            if (targetScoreReached())
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
            if (targetComboReached())
            {
                ApplySolidColor(textComponent, successColor);
            }
            else
            {
                ApplyDefaultGradient(textComponent);
            }
        }
        else if (textComponent == comboTitleText)
        {
            textComponent.color = new Color(1f, 0.95f, 0.65f, 1f);
        }
    }

    private bool targetScoreReached()
    {
        int targetScore = 0;
        if (BoardManager.Instance != null && BoardManager.Instance.currentLevel != null)
        {
            targetScore = BoardManager.Instance.currentLevel.targetScore;
        }

        return targetScore > 0 && currentScore >= targetScore;
    }

    private bool targetComboReached()
    {
        int targetCombo = 0;
        if (BoardManager.Instance != null && BoardManager.Instance.currentLevel != null)
        {
            targetCombo = BoardManager.Instance.currentLevel.targetCombo;
        }

        return targetCombo > 0 && maxComboReached >= targetCombo;
    }
}