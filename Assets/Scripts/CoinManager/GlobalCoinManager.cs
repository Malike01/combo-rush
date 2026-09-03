using UnityEngine;
using TMPro;

public class GlobalCoinManager : MonoBehaviour
{
    public static GlobalCoinManager Instance; 
    
    [Header("UI Referansı")]
    public TextMeshProUGUI totalCoinText;
    
    private const string TOTAL_COIN_KEY = "GlobalTotalCoins"; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        UpdateCoinDisplay();
    }

    public void AddGlobalCoins(int amount)
    {
        int currentCoins = PlayerPrefs.GetInt(TOTAL_COIN_KEY, 0);
        currentCoins += amount;
        
        PlayerPrefs.SetInt(TOTAL_COIN_KEY, currentCoins);
        PlayerPrefs.Save();
        
        UpdateCoinDisplay();
    }

    public bool SpendGlobalCoins(int amount)
    {
        int currentCoins = PlayerPrefs.GetInt(TOTAL_COIN_KEY, 0);
        
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            PlayerPrefs.SetInt(TOTAL_COIN_KEY, currentCoins);
            PlayerPrefs.Save();
            UpdateCoinDisplay();
            return true; 
        }
        
        return false; 
    }

    public void UpdateCoinDisplay()
    {
        if (totalCoinText != null)
        {
            int currentCoins = PlayerPrefs.GetInt(TOTAL_COIN_KEY, 0);
            totalCoinText.text = currentCoins.ToString("N0"); 
        }
    }
}