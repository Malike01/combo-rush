using UnityEngine;
using TMPro; 

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;
    
    public int coins; 
    public TextMeshProUGUI coinText;

    private readonly string coinKey = "GlobalTotalCoins"; 

    void Awake() 
    {
        if(Instance == null) Instance = this;

        coins = PlayerPrefs.GetInt(coinKey, 500); 

        UpdateUI();
    }

    public bool SpendCoins(int amount) 
    {
        if(coins >= amount) 
        {
            coins -= amount;
            
            PlayerPrefs.SetInt(coinKey, coins);
            PlayerPrefs.Save();
            
            UpdateUI();
            return true;
        }
        
        Debug.LogWarning("Yetersiz Coin!");
        return false;
    }

    void UpdateUI() 
    {
        if(coinText != null) 
        {
            coinText.text = coins.ToString() + " Coin"; 
        }
    }
}