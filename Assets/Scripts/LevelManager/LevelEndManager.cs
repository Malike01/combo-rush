using UnityEngine;
using TMPro; 

public class LevelEndManager : MonoBehaviour
{
    public static LevelEndManager Instance;

    [Header("Panels")]
    public GameObject levelEndPanel; 
    public GameObject winContentGroup; 
    public GameObject loseContentGroup;

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

        IsLevelEnded = true;
        transform.SetAsLastSibling(); 
        
        if (levelEndPanel != null) levelEndPanel.SetActive(true);

        EvaluateLevelResults();
    }

    private void EvaluateLevelResults()
    {
        LevelData activeLevel = LevelManager.Instance.allLevels[LevelManager.Instance.currentLevelIndex];

        if (activeLevel == null || ScoreManager.Instance == null) return;

        int currentScore = ScoreManager.Instance.currentScore;
        int maxCombo = ScoreManager.Instance.maxComboReached;

        int targetScore = activeLevel.targetScore;
        bool isTaskCompleted = LevelManager.Instance.isExtraTaskCompleted;

        if (currentScore >= targetScore && isTaskCompleted)
        {
            LevelManager.Instance.LevelCompletedSuccess(); 
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

        bool won = winContentGroup.activeSelf;
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