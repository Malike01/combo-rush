using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening; 

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Data (Auto Loaded)")]
    public List<LevelData> allLevels = new List<LevelData>();    
    public int currentLevelIndex = 0;    

    [Header("Extra Task Tracking")]
    public bool isExtraTaskCompleted = false;

    public bool IsIntroPlaying { get; private set; } = false;

    [Header("UI - In-Game HUD")]
    public TextMeshProUGUI currentLevelText; 
    public TextMeshProUGUI extraTaskText;
    
    [Header("UI - Level Intro Popup (Bölüm Başlangıcı)")]
    [Tooltip("The panel that shows goals when a level starts")]
    public GameObject levelIntroPanel;            
    public TextMeshProUGUI introLevelTitleText;   
    public TextMeshProUGUI introTargetScoreText;  
    public TextMeshProUGUI introTaskDescText;     

    [Header("UI - Level Selection Menu")]
    public GameObject levelSelectionPanel;   
    public Transform levelButtonContainer;   
    public GameObject levelButtonPrefab;     

    private bool isComboSubTaskDone = false;
    private int currentVariantCount = 0;

    private int mysteryCoinsPoppedCount = 0;
    public int clockRotationCount = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadAllLevelsAutomatically();
    }

    void Start()
    {
        GenerateLevelUI();
        
    }

    private void LoadAllLevelsAutomatically()
    {
        LevelData[] loadedLevels = Resources.LoadAll<LevelData>("Levels");
        allLevels = loadedLevels.OrderBy(l => l.levelNumber).ToList();
        
        if(allLevels.Count == 0)
        {
            Debug.LogError("Resources/Levels folder is empty or no LevelData assets found!");
        }
    }

    public void ResetIntroState()
    {
        IsIntroPlaying = false;
        if (levelIntroPanel != null)
        {
            levelIntroPanel.transform.DOKill();
            levelIntroPanel.SetActive(false);
        }
    }

    public void StartLevel(int index)
    {
        if (index >= allLevels.Count) return;

        if (LevelEndManager.Instance != null)
        {
            LevelEndManager.Instance.ResetForNewLevel();
        }

        if (ComboTimerManager.Instance != null)
        {
            ComboTimerManager.Instance.ResetTimerFull();
        }

        IsIntroPlaying = false;
        currentLevelIndex = index;
        PlayerPrefs.SetInt("LastPlayedLevel", currentLevelIndex);
        
        LevelData levelToLoad = allLevels[currentLevelIndex];

        // Reset the extra task completion status and sub-counters
        isExtraTaskCompleted = (levelToLoad.taskType == ExtraTaskType.None);
        isComboSubTaskDone = false;
        currentVariantCount = 0;

        mysteryCoinsPoppedCount = 0;
        clockRotationCount = 0;

        UpdateLevelDisplay(levelToLoad);

        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.LoadLevel(levelToLoad);
        }

        // TRIGGER THE INTRO POPUP ANIMATION
        ShowLevelIntroPopup(levelToLoad);    
    }

    public void StartGameFromMenu()
    {
        int savedLevel = PlayerPrefs.GetInt("LastPlayedLevel", 0);
        StartLevel(savedLevel);
    }

    private void ShowLevelIntroPopup(LevelData level)
    {
        if (levelIntroPanel == null) 
        {
            Debug.LogWarning("LevelManager: 'Level Intro Panel' is not assigned in the Inspector! Cannot show intro.");
            IsIntroPlaying = false;
            return;
        }

        // 1. Lock the gameplay while the intro is playing
        IsIntroPlaying = true; 

        string lvlPrefix = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetText("level_prefix") : "LEVEL";
        string targetPrefix = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetText("target_prefix") : "Hedef:";
        string pointsSuffix = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetText("points_suffix") : "Puan";
        string missionPrefix = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetText("mission_prefix") : "Görev:";
        string justPointsText = LocalizationManager.Instance != null ? LocalizationManager.Instance.GetText("just_points") : "Sadece Puan Topla!";

        // 2. Set the texts based on the current level data
        if (introLevelTitleText != null) introLevelTitleText.text = $"{lvlPrefix} {level.levelNumber}";
        if (introTargetScoreText != null) introTargetScoreText.text = $"{targetPrefix}: {level.targetScore} {pointsSuffix}";
        
        if (introTaskDescText != null)
        {
           string taskKey = level.taskType != ExtraTaskType.None ? level.taskDescription : "just_points";
            
            if (LocalizationManager.Instance != null)
            {
                taskKey = LocalizationManager.Instance.GetText(taskKey);
            }

            introTaskDescText.text = $"{missionPrefix}: {taskKey}";
        }

        // 3. Prepare the panel for animation
        levelIntroPanel.SetActive(true);
        levelIntroPanel.transform.DOKill(); 
        levelIntroPanel.transform.localScale = Vector3.zero;

        // 4. Play the pop-up sequence with snappy timing
        levelIntroPanel.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).OnComplete(() => 
        {
            levelIntroPanel.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).SetDelay(1.8f).OnComplete(() => 
            {
                levelIntroPanel.SetActive(false);
                
                // 5. Unlock the gameplay! The player can now make moves.
                IsIntroPlaying = false; 
            });
        });

        StartCoroutine(SafetyUnlockIntroRoutine(3.0f));
    }

    private System.Collections.IEnumerator SafetyUnlockIntroRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        IsIntroPlaying = false;
    }

    public void SetCurrentLevel(LevelData level)
    {
        if (level == null) return;
        int index = allLevels.FindIndex(item => item == level);
        if (index >= 0) currentLevelIndex = index;
        UpdateLevelDisplay(level);
    }

    public void UpdateLevelDisplay(LevelData level = null)
    {
        LevelData targetLevel = level != null ? level : (currentLevelIndex >= 0 && currentLevelIndex < allLevels.Count ? allLevels[currentLevelIndex] : null);

        if (targetLevel != null)
        {
            if (currentLevelText != null) currentLevelText.text = "Level " + '\n' + targetLevel.levelNumber;
            
            if (extraTaskText != null)
            {
                extraTaskText.text = targetLevel.taskType == ExtraTaskType.None ? "Ek Görev Yok" : targetLevel.taskDescription;
                extraTaskText.color = isExtraTaskCompleted ? Color.green : Color.white;
            }
        }
    }

    public void ProcessExtraTask(int matchCount, int colorIndex, bool hasMysteryCoin, int maxVariantMatched)
    {
        if (isExtraTaskCompleted || currentLevelIndex >= allLevels.Count) return;

        LevelData currentLevel = allLevels[currentLevelIndex];

        switch (currentLevel.taskType)
        {
            case ExtraTaskType.ReachComboCount:
                if (matchCount >= currentLevel.taskTargetValue) isExtraTaskCompleted = true;
                break;

            case ExtraTaskType.SpecificColorCombo:
                if (colorIndex == currentLevel.taskSecondaryValue && matchCount >= currentLevel.taskTargetValue) isExtraTaskCompleted = true;
                break;

            case ExtraTaskType.IncludeMysteryCoin:
                if (hasMysteryCoin) isExtraTaskCompleted = true;
                break;

            case ExtraTaskType.MatchSpecificVariant:
                if (maxVariantMatched >= currentLevel.taskTargetValue)
                {
                    currentVariantCount++;
                    if (currentVariantCount >= currentLevel.taskSecondaryValue) isExtraTaskCompleted = true;
                }
                break;

            case ExtraTaskType.ComboAndVariant:
                if (matchCount >= currentLevel.taskTargetValue) isComboSubTaskDone = true;
                if (maxVariantMatched >= 1) currentVariantCount++;
                if (isComboSubTaskDone && currentVariantCount >= currentLevel.taskSecondaryValue) isExtraTaskCompleted = true;
                break;

            case ExtraTaskType.MysteryAndClock:
                if (hasMysteryCoin)
                {
                    mysteryCoinsPoppedCount++;
                }

                if (mysteryCoinsPoppedCount >= currentLevel.taskTargetValue && 
                    clockRotationCount >= currentLevel.taskSecondaryValue)
                {
                    isExtraTaskCompleted = true;
                }
                break;
        }

        if (isExtraTaskCompleted && extraTaskText != null) extraTaskText.color = Color.green;
    }

    public void RegisterClockRotation()
    {
        clockRotationCount++;
        ProcessExtraTask(0, -1, false, 0);
    }

public void LevelCompletedSuccess()
    {
        int nextLevelNum = currentLevelIndex + 2; 
        int maxUnlocked = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
        if (nextLevelNum > maxUnlocked) PlayerPrefs.SetInt("MaxUnlockedLevel", nextLevelNum);

        int nextLevelIndex = currentLevelIndex + 1;
        if (nextLevelIndex < allLevels.Count)
        {
            PlayerPrefs.SetInt("LastPlayedLevel", nextLevelIndex);
        }
        
        PlayerPrefs.Save();

        int totalScore = ScoreManager.Instance.currentScore;
        int winCoin = Mathf.Max(1, Mathf.RoundToInt(totalScore * 0.05f));

        Vector3 screenPos = Camera.main.WorldToScreenPoint(ScoreManager.Instance.gameObject.transform.position); 
    
        if (CoinFlyEffect.Instance != null && GlobalCoinManager.Instance != null)
        {
            Transform globalSafeUI = GlobalCoinManager.Instance.totalCoinText.transform.parent;

            CoinFlyEffect.Instance.PlayCoinFlyAnimation(screenPos, globalSafeUI, winCoin, () =>
            {
                GlobalCoinManager.Instance.AddGlobalCoins(winCoin);
                ScoreManager.Instance.ResetLevelData();
            });
        }
        else
        {
            if (GlobalCoinManager.Instance != null)
            {
                GlobalCoinManager.Instance.AddGlobalCoins(winCoin);
                ScoreManager.Instance.ResetLevelData();
            }
        }
    }

    private void GenerateLevelUI()
    {
        // Internal standard UI logic (unchanged)
    }

    public void ToggleLevelPanel()
    {
        if (levelSelectionPanel != null) levelSelectionPanel.SetActive(!levelSelectionPanel.activeSelf);
    }
}