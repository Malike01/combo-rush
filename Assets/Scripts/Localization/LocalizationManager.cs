using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public struct Translation
{
    public string key; 
    [TextArea(1, 2)] public string tr;
    [TextArea(1, 2)] public string en;
}

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;

    [Header("Çeviri Listesi")]
    public List<Translation> translations;

    public string CurrentLanguage { get; private set; }
    
    public event Action OnLanguageChanged;

    private Dictionary<string, Translation> dictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDictionary();
            LoadLanguage();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDictionary()
    {
        dictionary = new Dictionary<string, Translation>();
        foreach (var t in translations)
        {
            if (!dictionary.ContainsKey(t.key))
            {
                dictionary.Add(t.key, t);
            }
        }
    }

    private void LoadLanguage()
    {
        CurrentLanguage = PlayerPrefs.GetString("Language", "TR");
    }

    public void SetLanguage(string langCode)
    {
        CurrentLanguage = langCode;
        PlayerPrefs.SetString("Language", CurrentLanguage);
        PlayerPrefs.Save();

        OnLanguageChanged?.Invoke();
    }
    public string GetText(string key)
    {
        if (dictionary != null && dictionary.TryGetValue(key, out Translation t))
        {
            return CurrentLanguage == "TR" ? t.tr : t.en;
        }
        return key; 
    }
}