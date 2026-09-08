using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Referansları")]
    public GameObject settingsPanel;
    public TextMeshProUGUI musicButtonText;
    public TextMeshProUGUI languageButtonText;

    private bool isMusicOn = true;
    private string currentLanguage = "TR";

    private void Start()
    {
        isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1; 
        currentLanguage = PlayerPrefs.GetString("Language", "TR");
        
        ApplySettings();
    }

    public void OpenSettingsPanel()
    {

        settingsPanel.SetActive(true);
    }

    public void CloseSettingsPanel()
    {

        settingsPanel.SetActive(false);
    }

    // --- Music Control ---
    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn; 
        PlayerPrefs.SetInt("MusicOn", isMusicOn ? 1 : 0);
        PlayerPrefs.Save();
        
        ApplySettings();
    }

    // --- Language Control ---
    public void ToggleLanguage()
    {
        string currentLang = LocalizationManager.Instance.CurrentLanguage;

        if (currentLang == "tr")
        {
            LocalizationManager.Instance.SetLanguage("en");
        }
        else
        {
            LocalizationManager.Instance.SetLanguage("tr");
        }
        
        languageButtonText.text = currentLang == "tr" ? "Language:EN" : "Dil:TR";
    }

    private void ApplySettings()
    {
        AudioListener.volume = isMusicOn ? 1f : 0f;
        
        if (musicButtonText != null)
        {
            string musicKey = isMusicOn ? "music_on" : "music_off";
            musicButtonText.text = LocalizationManager.Instance.GetText(musicKey);
        }
            
        if (languageButtonText != null)
        {
            languageButtonText.text = LocalizationManager.Instance.GetText("language_btn") + " " + LocalizationManager.Instance.CurrentLanguage;
        }
    }
    // --- Contact Us ---
    public void ContactUs()
    {
        string email = "destek@comborush.com";
        string subject = "Combo Rush - Oyuncu Geri Bildirimi";
        Application.OpenURL($"mailto:{email}?subject={subject}");
    }
}