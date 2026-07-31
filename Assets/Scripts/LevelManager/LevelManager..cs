using UnityEngine;
using UnityEngine.UI;
using TMPro; // Yazılar için TextMeshPro kullanıyoruz
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance; // Her yerden kolayca ulaşmak için

    [Header("Level Data")]
    public List<LevelData> allLevels;    // Tüm bölümlerin listesi
    public int currentLevelIndex = 0;    // Şu an oynanan bölümün sırası

    [Header("References")]
    public BoardManager boardManager;    // Tahtayı çizen sistem

    [Header("UI - Oynanış Ekranı")]
    public TextMeshProUGUI currentLevelText; // Tepede "Level 1" yazacak metin

    [Header("UI - Seviye Seçim Menüsü")]
    public GameObject levelSelectionPanel;   // Bölümleri gösteren ana panel
    public Transform levelButtonContainer;   // Butonların içine dizileceği obje (Grid Layout)
    public GameObject levelButtonPrefab;     // Her bir bölüm için üretilecek buton

    void Awake()
    {
        // Singleton mantığı: Sistemin tek bir LevelManager ile çalışmasını sağlar
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        GenerateLevelUI();
        StartLevel(currentLevelIndex);
    }

    // İstenilen bölümü başlatan ana fonksiyon
    public void StartLevel(int index)
    {
        if (index >= allLevels.Count)
        {
            Debug.Log("Tebrikler, tüm bölümler bitti!");
            return;
        }

        currentLevelIndex = index;
        LevelData levelToLoad = allLevels[currentLevelIndex];

        // UI Güncellemesi
        if(currentLevelText != null)
            currentLevelText.text = "Level " + levelToLoad.levelNumber;

        // Tahtayı yeni bölüme göre sıfırla ve çiz
        boardManager.LoadLevel(levelToLoad);
    }

    // Puan kazanıldığında bu fonksiyon çağrılacak
    public void CheckLevelProgress(int currentScore)
    {
        LevelData currentLevelData = allLevels[currentLevelIndex];

        if (currentScore >= currentLevelData.targetScore)
        {
            Debug.Log("Bölüm Tamamlandı! Diğer levele geçiliyor...");
            NextLevel();
        }
    }

    private void NextLevel()
    {
        currentLevelIndex++;
        

        StartLevel(currentLevelIndex);
    }

    private void GenerateLevelUI()
    {
        if (levelButtonPrefab == null || levelButtonContainer == null) return;

        foreach (Transform child in levelButtonContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < allLevels.Count; i++)
        {
            int lvlIndex = i; // Listener için indexi kopyalıyoruz
            GameObject btnObj = Instantiate(levelButtonPrefab, levelButtonContainer);
            
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = (i + 1).ToString();

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
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