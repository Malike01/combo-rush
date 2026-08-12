using UnityEngine;
using TMPro; 

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;
    
    public int coins = 500; 
    public TextMeshProUGUI coinText;

    void Awake() 
    {
        if(Instance == null) Instance = this;
        UpdateUI();
    }

    public bool SpendCoins(int amount) 
    {
        if(coins >= amount) 
        {
            coins -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    void UpdateUI() 
    {
        if(coinText != null) 
        {
            coinText.text = coins.ToString() + " Coin"; // Display the coin count with " Coin" suffix
        }
    }
}