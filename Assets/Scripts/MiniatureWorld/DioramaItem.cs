using UnityEngine;
using TMPro;
using System.Collections;

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
    
    private Vector3 originalScale;
    private Vector3 originalPosition;

    void Start()
    {
        renderers = GetComponentsInChildren<MeshRenderer>();
        
        originalScale = transform.localScale; 
        originalPosition = transform.localPosition; 
        
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

            StartCoroutine(PopAnimation());
            AudioManager.Instance.PlaySound("PurchaseSuccess");

            Debug.Log("Obje satın alındı: " + gameObject.name);
        }
        else
        {
            StartCoroutine(ErrorShakeAnimation());
            
            if (costText != null) 
                StartCoroutine(FlashTextRed());
        }
    }

    private IEnumerator PopAnimation()
    {
        float duration = 0.3f; 
        float time = 0f;
        Vector3 targetScale = originalScale * 1.2f;

        while (time < duration / 2)
        {
            time += Time.deltaTime;
            float t = time / (duration / 2);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null; 
        }

        time = 0f;
        while (time < duration / 2)
        {
            time += Time.deltaTime;
            float t = time / (duration / 2);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale; 
    }

    private IEnumerator ErrorShakeAnimation()
    {
        float duration = 0.4f; 
        float magnitude = 0.15f; 
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = originalPosition.x + Random.Range(-1f, 1f) * magnitude;
            float z = originalPosition.z + Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, originalPosition.y, z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    private IEnumerator FlashTextRed()
    {
        Color originalColor = costText.color; 
        costText.color = Color.red;           
        
        yield return new WaitForSeconds(0.4f); 
        
        costText.color = originalColor;       
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