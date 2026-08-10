using UnityEngine;
using TMPro;

public class DioramaItem : MonoBehaviour
{
    public int cost = 100;

    [Header("Görsel Ayarlar")]
    public Material lockedMaterial;   
    public Material unlockedMaterial;

    [Header("UI Ayarları")]
    public GameObject purchaseUI;
    public TextMeshProUGUI costText;

    [Header("Zincirleme Açılış (Opsiyonel)")]
    public GameObject[] objectsToEnable;
    private MeshRenderer[] renderers;
    private bool isUnlocked = false;

    void Start()
    {
        renderers = GetComponentsInChildren<MeshRenderer>();

        SetMaterial(lockedMaterial);

        if (costText != null)
            costText.text = cost.ToString() + " Coin";

        if (purchaseUI != null)
            purchaseUI.SetActive(true);
    }

    public void BuyItem()
    {
        if (isUnlocked) return;

        if (EconomyManager.Instance != null && EconomyManager.Instance.SpendCoins(cost))
        {
            isUnlocked = true;
            SetMaterial(unlockedMaterial);
            
            if (purchaseUI != null) 
                purchaseUI.SetActive(false);

            foreach (GameObject obj in objectsToEnable)
            {
                if (obj != null) obj.SetActive(true);
            }

            Debug.Log("Obje satın alındı: " + gameObject.name);
        }
        else
        {
            Debug.Log("Yeterli paranız yok!");
        }
    }

    private void SetMaterial(Material mat)
    {
        if (mat == null) return;

        foreach (MeshRenderer rend in renderers)
        {
            rend.material = mat;
        }
    }
}