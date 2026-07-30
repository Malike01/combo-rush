using UnityEngine;
using System.Collections.Generic;

public class TopComboBannerManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject playerSlotPrefab; 
    public Transform slotsParent;      

    [Header("Sıralama Renkleri")]
    public Color goldColor = new Color(1f, 0.84f, 0f, 0.8f);
    public Color silverColor = new Color(0.75f, 0.75f, 0.75f, 0.8f);
    public Color bronzeColor = new Color(0.8f, 0.5f, 0.2f, 0.8f);

    private List<GameObject> activeSlots = new List<GameObject>();

    private void Start()
    {
        CreateTestLeaderboard();
    }

    public void CreateTestLeaderboard()
    {
        foreach (var slot in activeSlots)
        {
            Destroy(slot);
        }
        activeSlots.Clear();

        CreateSlot("KralCombo", 84, goldColor);
        CreateSlot("CoinMaster", 56, silverColor);
        CreateSlot("Oyuncu77", 32, bronzeColor);
    }

    private void CreateSlot(string playerName, int combo, Color rankColor)
    {
        GameObject newSlot = Instantiate(playerSlotPrefab, slotsParent);
        
        activeSlots.Add(newSlot);

        PlayerComboSlot slotScript = newSlot.GetComponent<PlayerComboSlot>();
        if (slotScript != null)
        {
            slotScript.SetupSlot(playerName, combo, rankColor);
        }
    }
}