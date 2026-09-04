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

    [Header("Delay & Visual Effects (Popups)")]
    public GameObject levelCompleteTexture; 
    public GameObject gameOverTexture;      
    public float popupTextureScale = 0.7f; 
    public GameObject[] otherUIElements; 

    [Header("Win UI")]
    public TextMeshProUGUI winTitleText;          
    
    [Header("Win Stars System (Single Dynamic Badge)")]
    [Tooltip("The single UI Image GameObject where the star texture will be shown")]
    public Image singleStarDisplayImage; 
    
    [Tooltip("Texture for 1 Star result")]
    public Sprite star1Texture;
    [Tooltip("Texture for 2 Stars result")]
    public Sprite star2Texture;
    [Tooltip("Texture for 3 Stars result")]
    public Sprite star3Texture;

    [Tooltip("Final scale size of the star banner on screen")]
    public float starFinalScale = 1.0f; 

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

        Debug.Log("Level Ended! Sequence started.");
        IsLevelEnded = true;

        this.gameObject.SetActive(true);
        if (levelEndPanel != null) levelEndPanel.SetActive(true);

        transform.SetAsLastSibling(); 
        
        StartCoroutine(ShowLevelEndSequence());
    }

   private IEnumerator ShowLevelEndSequence()
    {
        bool isWin = EvaluateLevelResults();

        if (winContentGroup != null) winContentGroup.SetActive(false);
        if (loseContentGroup != null) loseContentGroup.SetActive(false);

        Image panelBgImage = null;
        if (levelEndPanel != null)
        {
            panelBgImage = levelEndPanel.GetComponent<Image>();
            if (panelBgImage != null) panelBgImage.enabled = false;
        }
        
        foreach (GameObject uiElement in otherUIElements)
        {
            if (uiElement != null && uiElement != levelEndPanel) uiElement.SetActive(false);
        }

        GameObject activePopupTexture = isWin ? levelCompleteTexture : gameOverTexture;

        if (activePopupTexture != null) 
        {
            activePopupTexture.transform.DOKill();
            activePopupTexture.transform.localScale = Vector3.zero; 
            activePopupTexture.SetActive(true);
            
            Vector3 targetScale = Vector3.one * popupTextureScale;
            activePopupTexture.transform.DOScale(targetScale, 0.6f).SetEase(Ease.OutBounce);
        }

        yield return new WaitForSeconds(3f);

        if (activePopupTexture != null) 
        {
            activePopupTexture.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
            yield return new WaitForSeconds(0.3f); 
            activePopupTexture.SetActive(false);
        }

        if (panelBgImage != null) panelBgImage.enabled = true;

        foreach (GameObject uiElement in otherUIElements)
        {
            if (uiElement != null && uiElement != levelEndPanel) uiElement.SetActive(true);
        }

        if (isWin) 
        {
            if (winContentGroup != null) winContentGroup.SetActive(true);
            if (loseContentGroup != null) loseContentGroup.SetActive(false); 
            AnimateStarBanner();
        }
        else 
        {
            if (loseContentGroup != null) loseContentGroup.SetActive(true);
            if (winContentGroup != null) winContentGroup.SetActive(false); 
        }
    }
    private bool EvaluateLevelResults()
    {
        if (LevelManager.Instance == null || ScoreManager.Instance == null) return false;

        LevelData activeLevel = LevelManager.Instance.allLevels[LevelManager.Instance.currentLevelIndex];
        
        int currentScore = ScoreManager.Instance.currentScore;
        int maxCombo = ScoreManager.Instance.maxComboReached;
        int targetScore = activeLevel.targetScore;
        bool isTaskCompleted = LevelManager.Instance.isExtraTaskCompleted;

        bool isWin = (currentScore >= targetScore && isTaskCompleted);

        if (isWin)
        {
            LevelManager.Instance.LevelCompletedSuccess(); 
            SetupWinScreen(currentScore, maxCombo, activeLevel);
        }
        else
        {
            SetupLoseScreen(targetScore, currentScore, maxCombo, activeLevel, isTaskCompleted);
        }

        return isWin;
    }

    private void SetupWinScreen(int currentScore, int maxCombo, LevelData activeLevel)
    {
        int stars = 1;
        if (currentScore >= activeLevel.targetScore * 2) stars = 3; 
        else if (currentScore >= activeLevel.targetScore + (activeLevel.targetScore / 2f)) stars = 2; 

        if (winTitleText != null)
        {
            if (stars == 3) winTitleText.text = "MÜKEMMEL GEÇİŞ!";
            else if (stars == 2) winTitleText.text = "BAŞARILI GEÇİŞ!";
            else winTitleText.text = "GÖREV TAMAMLANDI!";
        }

        // Set the single texture based on calculated star score
        if (singleStarDisplayImage != null)
        {
            if (stars == 1 && star1Texture != null) singleStarDisplayImage.sprite = star1Texture;
            else if (stars == 2 && star2Texture != null) singleStarDisplayImage.sprite = star2Texture;
            else if (stars >= 3 && star3Texture != null) singleStarDisplayImage.sprite = star3Texture;

            // Maintain native texture aspect ratio so it doesn't stretch or shrink weirdly
            singleStarDisplayImage.SetNativeSize();
        }

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

    private void AnimateStarBanner()
    {
        if (singleStarDisplayImage == null) return;

        singleStarDisplayImage.transform.DOKill();
        singleStarDisplayImage.transform.localScale = Vector3.zero;
        singleStarDisplayImage.gameObject.SetActive(true);

        // Punch & Bounce Effect: Pops out large and settles down juicy
        Vector3 targetScale = Vector3.one * starFinalScale;
        singleStarDisplayImage.transform.DOScale(targetScale, 0.55f).SetEase(Ease.OutBack);
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
        bool won = (winContentGroup != null && winContentGroup.activeSelf);

        IsLevelEnded = false;
        if (levelEndPanel != null) levelEndPanel.SetActive(false);
        if (winContentGroup != null) winContentGroup.SetActive(false);
        if (loseContentGroup != null) loseContentGroup.SetActive(false);

        if (BoardManager.Instance != null) BoardManager.Instance.ResetProcessingState();
        if (LevelManager.Instance != null) LevelManager.Instance.ResetIntroState();
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetLevelData();
        if (ComboTimerManager.Instance != null) ComboTimerManager.Instance.ResetTimerFull();

        InputManager inputMgr = Object.FindFirstObjectByType<InputManager>();
        if (inputMgr != null)
        {
            inputMgr.ResetInputState();
        }

        if (won)
        {
            int nextIdx = LevelManager.Instance.currentLevelIndex + 1;
            LevelManager.Instance.StartLevel(nextIdx);
        }
        else
        {
            LevelManager.Instance.StartLevel(LevelManager.Instance.currentLevelIndex);
        }
    }
}