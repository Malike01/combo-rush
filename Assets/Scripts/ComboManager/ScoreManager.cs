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
    public Color defaultColor = Color.white;
    public Color successColor = Color.green;

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
        
        if (scoreText != null) scoreText.color = defaultColor;
        if (comboText != null) comboText.color = defaultColor;
        if (comboTitleText != null) comboTitleText.text = "";
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
                scoreText.color = successColor;
            }
            else
            {
                scoreText.color = defaultColor;
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
                comboText.text = $"{evolutionName} x{currentCombo}"; //[cite: 2]
            }

            // Yapilan en yuksek kombo (maxComboReached) hedefe ulastiysa yesil yap
            if (targetCombo > 0 && maxComboReached >= targetCombo)
            {
                comboText.color = successColor;
            }
            else
            {
                comboText.color = defaultColor;
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

    // --- ANIMATION SECTION ---

    private void TriggerPunchAnimation()
    {
        if (scoreText != null) //[cite: 2]
        {
            if (scoreAnimCoroutine != null) StopCoroutine(scoreAnimCoroutine); //[cite: 2]
            scoreAnimCoroutine = StartCoroutine(PunchScaleRoutine(scoreText.transform, 1.25f, 0.15f)); //[cite: 2]
        }

        if (comboText != null) //[cite: 2]
        {
            if (comboAnimCoroutine != null) StopCoroutine(comboAnimCoroutine); //[cite: 2]
            comboAnimCoroutine = StartCoroutine(PunchScaleRoutine(comboText.transform, 1.35f, 0.15f)); //[cite: 2]
        }
        
        // Eger unvan metni varsa ona da bir sekme (punch) efekti ekliyoruz
        if (comboTitleText != null && !string.IsNullOrEmpty(comboTitleText.text))
        {
            StartCoroutine(PunchScaleRoutine(comboTitleText.transform, 1.15f, 0.15f));
        }
    }

    private IEnumerator PunchScaleRoutine(Transform targetTransform, float scaleFactor, float duration)
    {
        Vector3 originalScale = Vector3.one; //[cite: 2]
        Vector3 punchScale = originalScale * scaleFactor; //[cite: 2]

        float halfDuration = duration / 2f; //[cite: 2]
        float elapsed = 0f; //[cite: 2]

        // 1. Growth Phase[cite: 2]
        while (elapsed < halfDuration) //[cite: 2]
        {
            if(targetTransform == null) yield break;
            targetTransform.localScale = Vector3.Lerp(originalScale, punchScale, elapsed / halfDuration); //[cite: 2]
            elapsed += Time.deltaTime; //[cite: 2]
            yield return null; //[cite: 2]
        }
        if(targetTransform != null) targetTransform.localScale = punchScale; //[cite: 2]

        // 2. Shrinking / Return to Original State Phase[cite: 2]
        elapsed = 0f; //[cite: 2]
        while (elapsed < halfDuration) //[cite: 2]
        {
            if(targetTransform == null) yield break;
            targetTransform.localScale = Vector3.Lerp(punchScale, originalScale, elapsed / halfDuration); //[cite: 2]
            elapsed += Time.deltaTime; //[cite: 2]
            yield return null; //[cite: 2]
        }

        if(targetTransform != null) targetTransform.localScale = originalScale; //[cite: 2]
    }
}