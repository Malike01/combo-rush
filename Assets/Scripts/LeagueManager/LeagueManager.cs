using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

[System.Serializable]
public class LeagueTier
{
    public string tierName;     
    public int requiredPoints;  
    public int requiredLevel;     
    public Sprite tierIcon;       
}

public class LeagueManager : MonoBehaviour
{
    [Header("UI Referansları")]
    public GameObject leaguePanel;       
    public Transform panelContent;       
    public TextMeshProUGUI leagueNameText;
    public TextMeshProUGUI pointsText;
    public Image leagueIconImage;

    [Header("22 Kademelik Lig Sistemi")]
    public List<LeagueTier> leagueTiers = new List<LeagueTier>();

    private int playerLeaguePoints = 0; 
    private int playerCompletedLevel = 0; 

    private void Start()
    {
        playerLeaguePoints = PlayerPrefs.GetInt("LeaguePoints", 0);
        
        playerCompletedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1); 
    }

    public void OpenLeaguePanel()
    {
        playerLeaguePoints = PlayerPrefs.GetInt("LeaguePoints", 0);
        playerCompletedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        
        leaguePanel.SetActive(true);
        UpdateLeagueUI();

        panelContent.DOKill(); 
        panelContent.localScale = Vector3.zero; 
        panelContent.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
    }

    public void CloseLeaguePanel()
    {
        panelContent.DOKill();
        
        panelContent.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
        {
            leaguePanel.SetActive(false);
        });
    }

    private void UpdateLeagueUI()
    {
        if (leagueTiers == null || leagueTiers.Count == 0) return;

        LeagueTier currentTier = leagueTiers[0]; 

        for (int i = leagueTiers.Count - 1; i >= 0; i--)
        {
            if (playerLeaguePoints >= leagueTiers[i].requiredPoints && 
                playerCompletedLevel >= leagueTiers[i].requiredLevel)
            {
                currentTier = leagueTiers[i];
                break; 
            }
        }

        if (leagueNameText != null) leagueNameText.text = currentTier.tierName;
        if (pointsText != null) pointsText.text = $"Toplam Puan: {playerLeaguePoints}";
        if (leagueIconImage != null && currentTier.tierIcon != null) leagueIconImage.sprite = currentTier.tierIcon;
    }

    public void AddLeaguePoints(int amount)
    {
        playerLeaguePoints += amount;
        PlayerPrefs.SetInt("LeaguePoints", playerLeaguePoints);
        PlayerPrefs.Save();
    }
}