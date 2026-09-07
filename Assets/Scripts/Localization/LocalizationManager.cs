using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TranslationItem 
{
    public string key; 
    public string tr;
    public string en;
}

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;

    public string CurrentLanguage { get; private set; } = "tr"; // Varsayılan dil
    public event System.Action OnLanguageChanged;

    [Header("Genel UI (Arayüz) Çevirileri")]
    public List<TranslationItem> uiTranslations = new List<TranslationItem>();

    [Header("Görev (Task) Çevirileri")]
    public List<TranslationItem> taskTranslations = new List<TranslationItem>();

    [Header("Wiki & Bilgi Ekranı Çevirileri")]
    public List<TranslationItem> wikiTranslations = new List<TranslationItem>();

    private Dictionary<string, TranslationItem> allTranslationsDict = new Dictionary<string, TranslationItem>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        CurrentLanguage = PlayerPrefs.GetString("SelectedLanguage", "tr");

        MergeListIntoDictionary(uiTranslations);
        MergeListIntoDictionary(taskTranslations);
        MergeListIntoDictionary(wikiTranslations);
    }

    private void MergeListIntoDictionary(List<TranslationItem> listToMerge)
    {
        foreach (var item in listToMerge)
        {
            if (!string.IsNullOrEmpty(item.key) && !allTranslationsDict.ContainsKey(item.key))
            {
                allTranslationsDict.Add(item.key, item);
            }
        }
    }


    public void SetLanguage(string languageCode)
    {
        if (CurrentLanguage == languageCode) return;

        CurrentLanguage = languageCode;
        PlayerPrefs.SetString("SelectedLanguage", CurrentLanguage);
        PlayerPrefs.Save();

        OnLanguageChanged?.Invoke();
    }

    public string GetText(string key)
    {
        if (string.IsNullOrEmpty(key)) return "";

        if (allTranslationsDict.ContainsKey(key))
        {
            TranslationItem item = allTranslationsDict[key];
            
            if (CurrentLanguage == "tr" && !string.IsNullOrEmpty(item.tr)) return item.tr;
            if (CurrentLanguage == "en" && !string.IsNullOrEmpty(item.en)) return item.en;
        }
        
        return key; 
    }
}