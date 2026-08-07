using UnityEngine;
using TMPro;

public class DioramaItem : MonoBehaviour
{
    public int cost = 100; 
    
    [Header("Görsel Ayarlar")]
    public Material lockedMaterial;  
    public Material unlockedMaterial; 
    public MeshRenderer itemRenderer;

    [Header("UI Ayarları")]
    public GameObject purchaseUI;     
    public TextMeshProUGUI costText; 

    private bool isUnlocked = false;

    void Start() 
    {
        itemRenderer.material = lockedMaterial;
        costText.text = cost.ToString() + " Coin";
        purchaseUI.SetActive(true);

     
        float topOfObject = itemRenderer.bounds.max.y;
        
        purchaseUI.transform.position = new Vector3(
            transform.position.x, 
            topOfObject + 0.3f, 
            transform.position.z
        );
    }

    public void BuyItem() 
    {
        if(isUnlocked) return;

        if(EconomyManager.Instance.SpendCoins(cost)) 
        {
            isUnlocked = true;
            itemRenderer.material = unlockedMaterial; 
            purchaseUI.SetActive(false); 

            Debug.Log("Obje satın alındı: " + gameObject.name);
        } 
        else 
        {
            Debug.Log("Yeterli paranız yok!");
        }
    }
}