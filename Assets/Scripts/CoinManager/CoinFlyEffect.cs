using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CoinFlyEffect : MonoBehaviour
{
    public static CoinFlyEffect Instance;

    [Header("Efekt Ayarları")]
    public GameObject coinIconPrefab; 
    public Transform canvasTransform;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void PlayCoinFlyAnimation(Vector3 startWorldPos, Transform targetUI, int coinAmount, System.Action onComplete = null)
    {
        int coinCount = 5; 
        
        for (int i = 0; i < coinCount; i++)
        {
            int index = i; 
            
            GameObject coin = Instantiate(coinIconPrefab, canvasTransform);
            RectTransform coinRect = coin.GetComponent<RectTransform>();

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, startWorldPos);
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasTransform as RectTransform, screenPoint, null, out localPoint);
            coinRect.anchoredPosition = localPoint;

            Vector2 randomSpread = Random.insideUnitCircle * 100f;
            Vector2 targetPos = coinRect.anchoredPosition + randomSpread;

            Sequence seq = DOTween.Sequence();
            
            seq.Append(coinRect.DOAnchorPos(targetPos, 0.3f).SetEase(Ease.OutBack));
            
            seq.AppendInterval(0.1f * index);
            
            Vector2 targetUIPos = (targetUI as RectTransform).anchoredPosition;
            
            seq.Append(coinRect.DOAnchorPos(targetUIPos, 0.6f).SetEase(Ease.InBack));
            seq.Join(coinRect.DOScale(0.5f, 0.6f)); 

            seq.OnComplete(() =>
            {
                Destroy(coin);
                
                if (index == coinCount - 1)
                {
                    targetUI.DOScale(1.2f, 0.1f).SetLoops(2, LoopType.Yoyo); 
                    onComplete?.Invoke();
                }
            });
        }
    }
}