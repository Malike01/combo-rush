using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using DG.Tweening; 

public class LevelEndManager : MonoBehaviour
{
    public static LevelEndManager Instance;

    [Header("Panels")]
    public GameObject levelEndPanel; 
    public GameObject winContentGroup; 
    public GameObject loseContentGroup;

    [Header("Delay & Visual Effects")]
    public GameObject gameOverTexture;   
    [Tooltip("Target scale size for the Game Over texture popup (e.g., 0.6 or 0.8)")]
    public float gameOverTextureScale = 0.5f; // NEW: Easy slider for texture size
    public GameObject[] otherUIElements; 

    [Header("Win UI")]
    public TextMeshProUGUI winTitleText;          
    public TextMeshProUGUI winStarsText;          
    public TextMeshProUGUI winScoreAndComboText;   
    public TextMeshProUGUI winNextLevelTargetText; 

    [Header("Lose UI")]
    public TextMeshProUGUI loseMotivationText;     
    public TextMeshProUGUI loseScoreAndComboText;  
    public TextMeshProUGUI loseTargetInfoText;    

    [Header("References")]
    public ComboTimerManager timerManager; 
    public BoardManager boardManager; 

    public bool IsLevelEnded { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        IsLevelEnded = false;
        
        if (levelEndPanel != null) levelEndPanel.SetActive(false);
    }

    public void ShowLevelEndScreen()
    {
        if (IsLevelEnded) return;

        Debug.Log("Game Over! Level End Panel sequence started.");
        IsLevelEnded = true;

        // Wake up the GameObject to allow Coroutines
        this.gameObject.SetActive(true);
        if (levelEndPanel != null) levelEndPanel.SetActive(true);

        transform.SetAsLastSibling(); 
        
        StartCoroutine(ShowLevelEndSequence());
    }

    private IEnumerator ShowLevelEndSequence()
    {
        // 1. Ensure any ongoing animations on the game over texture are killed
        if (gameOverTexture != null) gameOverTexture.transform.DOKill();

        // 2. Hide the purple background image temporarily
        Image panelBgImage = null;
        if (levelEndPanel != null)
        {
            panelBgImage = levelEndPanel.GetComponent<Image>();
            if (panelBgImage != null) panelBgImage.enabled = false;
        }

        // 3. Hide inner contents
        if (winContentGroup != null) winContentGroup.SetActive(false);
        if (loseContentGroup != null) loseContentGroup.SetActive(false);
        
        foreach (GameObject uiElement in otherUIElements)
        {
            if (uiElement != null && uiElement != levelEndPanel) 
            {
                uiElement.SetActive(false);
            }
        }

        // 4. ANIMATION: Show Game Over Texture with an adjustable bouncy scale
        if (gameOverTexture != null) 
        {
            gameOverTexture.transform.localScale = Vector3.zero; // Start from size 0
            gameOverTexture.SetActive(true);
            
            // Pop out to the adjustable target scale over 0.6 seconds
            Vector3 targetScale = Vector3.one * gameOverTextureScale;
            gameOverTexture.transform.DOScale(targetScale, 0.6f).SetEase(Ease.OutBounce);
        }

        // 5. Wait for the delay (3 seconds)
        yield return new WaitForSeconds(3f);

        // 6. ANIMATION: Hide Game Over Texture shrinking it back down
        if (gameOverTexture != null) 
        {
            // Shrink with a snappy pull-back effect over 0.3 seconds
            gameOverTexture.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
            
            // Wait for the shrinking animation to finish before proceeding
            yield return new WaitForSeconds(0.3f); 
            gameOverTexture.SetActive(false);
        }

        // 7. Bring back the purple background image
        if (panelBgImage != null) panelBgImage.enabled = true;

        // 8. Restore buttons and common UI
        foreach (GameObject uiElement in otherUIElements)
        {
            if (uiElement != null && uiElement != levelEndPanel) 
            {
                uiElement.SetActive(true);
            }
        }

        // 9. Evaluate and show Win/Lose content
        EvaluateLevelResults();
    }

    private void EvaluateLevelResults()
    {
        if (LevelManager.Instance == null) return;
        if (LevelManager.Instance.allLevels.Count == 0 || LevelManager.Instance.currentLevelIndex >= LevelManager.Instance.allLevels.Count) return;
        if (ScoreManager.Instance == null) return;

        LevelData activeLevel = LevelManager.Instance.allLevels[LevelManager.Instance.currentLevelIndex];
        
        int currentScore = ScoreManager.Instance.currentScore;
        int maxCombo = ScoreManager.Instance.maxComboReached;

        int targetScore = activeLevel.targetScore;
        bool isTaskCompleted = LevelManager.Instance.isExtraTaskCompleted;

        // WIN CONDITION: Target score reached AND extra task completed
        if (currentScore >= targetScore && isTaskCompleted)
        {
            LevelManager.Instance.LevelCompletedSuccess(); // Unlocks next level
            SetupWinScreen(currentScore, maxCombo, activeLevel);
        }
        else
        {
            SetupLoseScreen(targetScore, currentScore, maxCombo, activeLevel, isTaskCompleted);
        }
    }

    private void SetupWinScreen(int currentScore, int maxCombo, LevelData activeLevel)
    {
        if (winContentGroup != null) winContentGroup.SetActive(true);
        if (loseContentGroup != null) loseContentGroup.SetActive(false);
        
        int stars = 1;
        if (currentScore >= activeLevel.targetScore * 2) stars = 3; 
        else if (currentScore >= activeLevel.targetScore + (activeLevel.targetScore / 2f)) stars = 2; 

        if (winTitleText != null)
        {
            if (stars == 3) winTitleText.text = "MÜKEMMEL GEÇİŞ!";
            else if (stars == 2) winTitleText.text = "BAŞARILI GEÇİŞ!";
            else winTitleText.text = "GÖREV TAMAMLANDI!";
        }

        if (winStarsText != null) winStarsText.text = $"{stars} YILDIZ!";
        if (winScoreAndComboText != null) winScoreAndComboText.text = $"Toplanan: {currentScore} Coin | Maks Kombo: x{maxCombo}";

        if (winNextLevelTargetText != null)
        {
            int nextIndex = LevelManager.Instance.currentLevelIndex + 1;
            if (nextIndex < LevelManager.Instance.allLevels.Count)
            {
                LevelData nextLvl = LevelManager.Instance.allLevels[nextIndex];
                string nextTask = nextLvl.taskType != ExtraTaskType.None ? $"\nEk Görev: {nextLvl.taskDescription}" : "";
                winNextLevelTargetText.text = $"Sıradaki Hedef:\n{nextLvl.targetScore} Puan{nextTask}";
            }
            else
            {
                winNextLevelTargetText.text = "Tebrikler, tüm bölümleri tamamladın!";
            }
        }
    }

    private void SetupLoseScreen(int targetScore, int currentScore, int maxCombo, LevelData activeLevel, bool isTaskCompleted)
    {
        if (winContentGroup != null) winContentGroup.SetActive(false);
        if (loseContentGroup != null) loseContentGroup.SetActive(true);

        if (loseMotivationText != null) 
        {
            if (currentScore < targetScore) loseMotivationText.text = "Hedef Puana Ulaşamadın!";
            else loseMotivationText.text = "Ek Görevi Tamamlayamadın!";
        }

        if (loseScoreAndComboText != null) loseScoreAndComboText.text = $"Toplanan: {currentScore} / {targetScore}";
        
        if (loseTargetInfoText != null) 
        {
            string taskInfo = activeLevel.taskType != ExtraTaskType.None ? $"\nGörev Durumu: {(isTaskCompleted ? "Başarılı" : "Başarısız")}" : "";
            loseTargetInfoText.text = $"Bölüm Hedefi: {targetScore} Puan{taskInfo}";
        }
    }

    public void OnClickClosePanel()
    {
        IsLevelEnded = false;
        if (levelEndPanel != null) levelEndPanel.SetActive(false); 

        bool won = winContentGroup != null && winContentGroup.activeSelf;
        if (won)
        {
            int nextIdx = LevelManager.Instance.currentLevelIndex + 1;
            LevelManager.Instance.StartLevel(nextIdx);
        }
        else
        {
            LevelManager.Instance.StartLevel(LevelManager.Instance.currentLevelIndex);
        }

        if (timerManager != null) timerManager.ResetTimerFull(); 
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetLevelData();
    }
}