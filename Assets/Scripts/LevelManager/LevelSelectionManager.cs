using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelSelectionManager : MonoBehaviour
{
    [Header("UI Referansları")]
    public GameObject popupPanel;
    public TMP_Dropdown levelDropdown;
    public Button leftButton;
    public Button rightButton;
    public Button playButton;
    
    [Header("Renk Ayarları")]
    public Color unlockedColor = Color.white;
    public Color lockedColor = new Color(0.2f, 0.2f, 0.2f, 1f); 

    private int maxUnlockedLevel = 1; 
    private int currentlyDisplayedLevel = 1;

    private int MaxLevelInGame => LevelManager.Instance.allLevels.Count;

    private void Start()
    {
        leftButton.onClick.AddListener(DecreaseLevel);
        rightButton.onClick.AddListener(IncreaseLevel);
        playButton.onClick.AddListener(PlaySelectedLevel);
        
        levelDropdown.onValueChanged.AddListener(OnDropdownLevelSelected);
    }

    public void OpenPopup()
    {
        popupPanel.SetActive(true);
        maxUnlockedLevel = PlayerPrefs.GetInt("MaxUnlockedLevel", 1); 
        currentlyDisplayedLevel = maxUnlockedLevel;
        
        SetupDropdown();
        UpdateUI();
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }

    private void PlaySelectedLevel()
    {
        LevelManager.Instance.StartLevel(currentlyDisplayedLevel - 1); 
        ClosePopup();
    }

    private void DecreaseLevel()
    {
        if (currentlyDisplayedLevel > 1)
        {
            currentlyDisplayedLevel--;
            UpdateUI();
        }
    }

    private void IncreaseLevel()
    {
        if (currentlyDisplayedLevel < MaxLevelInGame)
        {
            currentlyDisplayedLevel++;
            UpdateUI();
        }
    }

    private void OnDropdownLevelSelected(int index)
    {
        currentlyDisplayedLevel = index + 1;
        UpdateUI();
    }

    private void UpdateUI()
    {
        levelDropdown.SetValueWithoutNotify(currentlyDisplayedLevel - 1);
        
        bool isUnlocked = currentlyDisplayedLevel <= maxUnlockedLevel;

        levelDropdown.captionText.color = isUnlocked ? unlockedColor : lockedColor;
        playButton.interactable = isUnlocked;

        leftButton.interactable = currentlyDisplayedLevel > 1;
        rightButton.interactable = currentlyDisplayedLevel < MaxLevelInGame;
    }

    private void SetupDropdown()
    {
        levelDropdown.ClearOptions();
        List<string> levelOptions = new List<string>();

        for (int i = 1; i <= MaxLevelInGame; i++)
        {
            string optionText = (i <= maxUnlockedLevel) ? "Level " + i : "Level " + i + " (Kilitli)";
            levelOptions.Add(optionText);
        }

        levelDropdown.AddOptions(levelOptions);
    }
}