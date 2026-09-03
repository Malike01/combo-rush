using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class FestivalTimeManager : MonoBehaviour
{
    public static FestivalTimeManager Instance;

    [Header("Festival Ayarları")]
    public float cooldownDays = 3f; 
    private const string LAST_PLAY_KEY = "FestivalLastPlayTime";

    [Header("UI Referansları")]
    public Button playButton;
    public TextMeshProUGUI statusText; 
    
    [Header("Puzzle Geçişi (Yeni)")]
    public GameObject festivalPuzzlePanel; 
    public PuzzlePhotoStudio photoStudio;  
    public GameObject[] possibleRewardItems; 
    private DateTime targetTime;
    private bool isReady = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        LoadTime();
        InvokeRepeating(nameof(UpdateTimerUI), 0f, 1f);
    }

    private void LoadTime()
    {
        if (PlayerPrefs.HasKey(LAST_PLAY_KEY))
        {
            string timeStr = PlayerPrefs.GetString(LAST_PLAY_KEY);
            if (DateTime.TryParse(timeStr, out DateTime lastPlayTime))
            {
                targetTime = lastPlayTime.AddDays(cooldownDays);
            }
        }
        else
        {
            isReady = true;
            UpdateUIReady();
        }
    }

    private void UpdateTimerUI()
    {
        if (isReady) return;

        TimeSpan timeLeft = targetTime - DateTime.Now;

        if (timeLeft.TotalSeconds <= 0)
        {
            isReady = true;
            UpdateUIReady();
        }
        else
        {
            isReady = false;
            playButton.interactable = false;
            statusText.text = string.Format("Kalan Süre:\n{0}g {1}s {2}d {3}sn", 
                timeLeft.Days, timeLeft.Hours, timeLeft.Minutes, timeLeft.Seconds);
        }
    }

    private void UpdateUIReady()
    {
        playButton.interactable = true;
        statusText.text = "Biletin Hazır!";
    }
public void UseTicket()
    {

        if (!isReady) 
        {
            return; 
        }


        PlayerPrefs.SetString(LAST_PLAY_KEY, DateTime.Now.ToString("O"));
        PlayerPrefs.Save();
        isReady = false;
        LoadTime(); 

        if (possibleRewardItems.Length > 0 && photoStudio != null)
        {
            GameObject selectedItem = possibleRewardItems[Random.Range(0, possibleRewardItems.Length)];
            
            photoStudio.TakePhotoAndCreatePuzzle(selectedItem);
        }
       

        if (festivalPuzzlePanel != null)
        {
            festivalPuzzlePanel.SetActive(true);
        }
    }
}