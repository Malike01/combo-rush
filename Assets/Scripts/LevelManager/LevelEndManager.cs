using UnityEngine;
using TMPro; 

public class LevelEndManager : MonoBehaviour
{
    public static LevelEndManager Instance;

    [Header("Paneller")]
    [Tooltip("Şu an ekranda gördüğün ana bölüm sonu paneli")]
    public GameObject levelEndPanel; 

    [Header("Durum İçerikleri (LevelEndPanel İçinde)")]
    [Tooltip("Kazanma durumunda aktif olacak UI objesi (Grup)")]
    public GameObject winContentGroup; 
    [Tooltip("Kaybetme durumunda aktif olacak UI objesi (Grup)")]
    public GameObject loseContentGroup;

    [Header("Win (Kazanma) Ekranı Yazıları")]
    public TextMeshProUGUI winTitleText;          
    public TextMeshProUGUI winStarsText;          
    public TextMeshProUGUI winScoreAndComboText;   
    public TextMeshProUGUI winNextLevelTargetText; 

    [Header("Lose (Kaybetme) Ekranı Yazıları")]
    public TextMeshProUGUI loseMotivationText;     
    public TextMeshProUGUI loseScoreAndComboText;  
    public TextMeshProUGUI loseTargetInfoText;    

    [Header("Oyun Sistem Referansları")]
    public ComboTimerManager timerManager; 
    public BoardManager boardManager; 

    [Header("Level Yönetimi")]
    public LevelData currentLevel; 
    public LevelData nextLevel; 

    private bool isLevelWon = false; 
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
        LevelData activeLevel = currentLevel != null ? currentLevel : (boardManager != null ? boardManager.currentLevel : null);

        if (activeLevel == null)
        {
            Debug.LogError("PANEL AÇILAMIYOR: Aktif bir Bölüm (LevelData) verisi bulunamadı! Oyun hedefleri okuyamıyor.");
            return;
        }

        if (ScoreManager.Instance == null)
        {
            Debug.LogError("PANEL AÇILAMIYOR: Sahnede ScoreManager objesi bulunamadı!");
            return;
        }
        
        if (winContentGroup == null || loseContentGroup == null)
        {
            Debug.LogError("PANEL AÇILAMIYOR: Win veya Lose grup objeleri Inspector'da atanmamış!");
            return;
        }
        // ----------------------------------------------

        int currentScore = ScoreManager.Instance.currentScore;
        int maxCombo = ScoreManager.Instance.maxComboReached;

        int targetScore = activeLevel.targetScore;
        int targetCombo = activeLevel.targetCombo;

        // Win condition: Both score and combo must meet or exceed the targets
        if (currentScore >= targetScore && maxCombo >= targetCombo)
        {
            isLevelWon = true;
            SetupWinScreen(currentScore, maxCombo);
        }
        else
        {
            isLevelWon = false;
            SetupLoseScreen(targetScore, targetCombo, currentScore, maxCombo);
        }
    }

    private void SetupWinScreen(int currentScore, int maxCombo)
    {
        if (winContentGroup != null) winContentGroup.SetActive(true);
        if (loseContentGroup != null) loseContentGroup.SetActive(false);

        LevelData activeLevel = currentLevel != null ? currentLevel : boardManager.currentLevel;
        
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
        if (winScoreAndComboText != null) winScoreAndComboText.text = $"Toplanan: {currentScore} Coin | Kombo: x{maxCombo}";

        if (winNextLevelTargetText != null)
        {
            if (nextLevel != null)
            {
                winNextLevelTargetText.text = $"Sıradaki Hedef:\n{nextLevel.targetScore} Coin & x{nextLevel.targetCombo} Kombo";
            }
            else
            {
                winNextLevelTargetText.text = "Tebrikler, tüm bölümleri tamamladın!";
            }
        }
    }

    private void SetupLoseScreen(int targetScore, int targetCombo, int currentScore, int maxCombo)
    {
        if (winContentGroup != null) winContentGroup.SetActive(false);
        if (loseContentGroup != null) loseContentGroup.SetActive(true);

        if (loseMotivationText != null) loseMotivationText.text = "Pes Etmek Yok, Tekrar Dene!";
        if (loseScoreAndComboText != null) loseScoreAndComboText.text = $"Toplanan: {currentScore} Coin | Kombo: x{maxCombo}";
        if (loseTargetInfoText != null) loseTargetInfoText.text = $"Bölüm Hedefi:\n{targetScore} Coin & x{targetCombo} Kombo";
    }

    public void OnClickClosePanel()
    {
        IsLevelEnded = false;

        if (levelEndPanel != null) levelEndPanel.SetActive(false); 

        LevelData activeLevel;
        if (isLevelWon && nextLevel != null)
        {
            activeLevel = nextLevel;
            currentLevel = nextLevel;
        }
        else
        {
            activeLevel = currentLevel != null ? currentLevel : boardManager.currentLevel;
        }

        if (boardManager != null)
        {
            boardManager.LoadLevel(activeLevel);
        }

        if (timerManager != null) timerManager.ResetTimerFull(); 
        
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetLevelData();

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SetCurrentLevel(activeLevel);
        }
    }

}