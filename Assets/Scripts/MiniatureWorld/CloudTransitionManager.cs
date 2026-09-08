using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CloudTransitionManager : MonoBehaviour
{
    [Header("Referanslar")]
    public CanvasGroup canvasGroup;
    public RectTransform[] clouds;
    
    [Header("Ayarlar")]
    public float animDuration = 1.8f; 

    void Start()
    {
        foreach (RectTransform cloud in clouds)
        {
            float startOffsetX = Random.Range(-150f, 150f);
            float startOffsetY = Random.Range(-150f, 150f);
            cloud.anchoredPosition = new Vector2(startOffsetX, startOffsetY);
        }
        DisperseClouds();
    }

    public void DisperseClouds()
    {
        canvasGroup.blocksRaycasts = false;

        Vector2[] directions = new Vector2[]
        {
            new Vector2(-1500, 1500),  
            new Vector2(1500, 1500),   
            new Vector2(-1500, -1500), 
            new Vector2(1500, -1500), 
            new Vector2(0, 1500),      
            new Vector2(0, -1500)     
        };

        for (int i = 0; i < clouds.Length; i++)
        {
            RectTransform cloud = clouds[i];
            
            Vector2 targetDir = directions[i % directions.Length];
            
            Vector2 targetPos = cloud.anchoredPosition + targetDir;

            cloud.DOAnchorPos(targetPos, animDuration).SetEase(Ease.OutCubic);
            cloud.DOScale(cloud.localScale * 1.8f, animDuration).SetEase(Ease.OutQuad);

            float randomRotation = Random.Range(-45f, 45f);
            cloud.DORotate(new Vector3(0, 0, randomRotation), animDuration).SetEase(Ease.OutSine);
        }

        canvasGroup.DOFade(0f, animDuration * 0.7f).SetDelay(animDuration * 0.2f).OnComplete(() => 
        {
            gameObject.SetActive(false);
        });
    }
}