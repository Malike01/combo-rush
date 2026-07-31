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

    [Header("Level Ayarları")]
    public int maxLevelInGame = 100; 
    
    private int maxUnlockedLevel = 1; 
    private int currentlyDisplayedLevel = 1;

    private void Start()
    {
        leftButton.onClick.AddListener(DecreaseLevel);
        rightButton.onClick.AddListener(IncreaseLevel);
        
        levelDropdown.onValueChanged.AddListener(OnDropdownLevelSelected);
    }

    public void OpenPopup()
    {
        popupPanel.SetActive(true);
        
        maxUnlockedLevel = 5; 
        
        currentlyDisplayedLevel = maxUnlockedLevel;
        
        SetupDropdown();
        UpdateUI();
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
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
        if (currentlyDisplayedLevel < maxLevelInGame)
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
        rightButton.interactable = currentlyDisplayedLevel < maxLevelInGame;
    }

    private void SetupDropdown()
    {
        levelDropdown.ClearOptions();
        List<string> levelOptions = new List<string>();

        for (int i = 1; i <= maxLevelInGame; i++)
        {
            string optionText = (i <= maxUnlockedLevel) ? "Level " + i : "Level " + i + " (Kilitli)";
            levelOptions.Add(optionText);
        }

        levelDropdown.AddOptions(levelOptions);
    }
}