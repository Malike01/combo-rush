using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Data (Otomatik Yüklenir)")]
    public List<LevelData> allLevels = new List<LevelData>();    
    public int currentLevelIndex = 0;    

    [Header("Extra Task Tracking")]
    public bool isExtraTaskCompleted = false;

    [Header("UI - Oynanış Ekranı")]
    public TextMeshProUGUI currentLevelText; 
    public TextMeshProUGUI extraTaskText;
    
    [Header("UI - Seviye Seçim Menüsü")]
    public GameObject levelSelectionPanel;   
    public Transform levelButtonContainer;   
    public GameObject levelButtonPrefab;     

    private bool isComboSubTaskDone = false;
    private int currentVariantCount = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadAllLevelsAutomatically();
    }

    void Start()
    {
        GenerateLevelUI();
        StartLevel(PlayerPrefs.GetInt("LastPlayedLevel", 0)); 
    }

    private void LoadAllLevelsAutomatically()
    {
        LevelData[] loadedLevels = Resources.LoadAll<LevelData>("Levels");
        allLevels = loadedLevels.OrderBy(l => l.levelNumber).ToList();
        
        if(allLevels.Count == 0)
        {
            Debug.LogError("Resources/Levels folder is empty or no LevelData assets found. Please add LevelData assets to this folder.");
        }
    }

    public void StartLevel(int index)
    {
        if (index >= allLevels.Count) return;

        currentLevelIndex = index;
        PlayerPrefs.SetInt("LastPlayedLevel", currentLevelIndex);
        
        LevelData levelToLoad = allLevels[currentLevelIndex];

        // New: Reset the extra task completion status when starting a new level
        isExtraTaskCompleted = (levelToLoad.taskType == ExtraTaskType.None);

        UpdateLevelDisplay(levelToLoad);

        if (BoardManager.Instance != null)
            BoardManager.Instance.LoadLevel(levelToLoad);
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
            if (currentLevelText != null) currentLevelText.text = "Level " + targetLevel.levelNumber;
            
            if (extraTaskText != null)
            {
                extraTaskText.text = targetLevel.taskType == ExtraTaskType.None ? "Ek Görev Yok" : targetLevel.taskDescription;
                extraTaskText.color = isExtraTaskCompleted ? Color.green : Color.white;
            }
        }
    }

    //This method checks if the extra task for the current level is completed based on the provided parameters.
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

            // NEW: Dual Task Evaluation (e.g., Level 14: 1x 10-Combo AND 2x V2)
            case ExtraTaskType.ComboAndVariant:
                // Check Combo requirement (taskTargetValue = 10)
                if (matchCount >= currentLevel.taskTargetValue)
                {
                    isComboSubTaskDone = true;
                }

                // Check Variant requirement (V2 variant = 1, required count = taskSecondaryValue = 2)
                if (maxVariantMatched >= 1)
                {
                    currentVariantCount++;
                }

                // Both conditions must be met
                if (isComboSubTaskDone && currentVariantCount >= currentLevel.taskSecondaryValue)
                {
                    isExtraTaskCompleted = true;
                }
                break;
        }

        if (isExtraTaskCompleted && extraTaskText != null) extraTaskText.color = Color.green;
    }
    public void LevelCompletedSuccess()
    {
        int nextLevelNum = currentLevelIndex + 2; 
        int maxUnlocked = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
        if (nextLevelNum > maxUnlocked)
        {
            PlayerPrefs.SetInt("MaxUnlockedLevel", nextLevelNum);
        }
    }

    private void GenerateLevelUI()
    {
        if (levelButtonPrefab == null || levelButtonContainer == null) return;

        foreach (Transform child in levelButtonContainer) Destroy(child.gameObject);

        int maxUnlocked = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);

        for (int i = 0; i < allLevels.Count; i++)
        {
            int lvlIndex = i; 
            GameObject btnObj = Instantiate(levelButtonPrefab, levelButtonContainer);
            
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = allLevels[i].levelNumber.ToString();

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = (i < maxUnlocked); // Locked levels are not interactable
                btn.onClick.AddListener(() => {
                    StartLevel(lvlIndex);
                    ToggleLevelPanel();
                });
            }
        }
    }

    public void ToggleLevelPanel()
    {
        if (levelSelectionPanel != null)
            levelSelectionPanel.SetActive(!levelSelectionPanel.activeSelf);
    }
}