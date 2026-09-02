using UnityEngine;
using DG.Tweening;

public class MenuLogoAnimator : MonoBehaviour
{
    [Header("Süzülme (Floating)")]
    [SerializeField] private float floatDistance = 15f;    
    [SerializeField] private float floatDuration = 2.2f;   
    [SerializeField] private float tiltAngle = 2.5f;     
    [Header("Nefes Alma (Breath)")]
    [SerializeField] private float breathScale = 1.04f;    
    [SerializeField] private float breathDuration = 1.8f;

    [Header("Tatlı Vurgu (Periodic Pop)")]
    [SerializeField] private bool enablePeriodicPop = true;
    [SerializeField] private float popInterval = 4.5f;     

    private RectTransform rectTransform;
    private Vector2 initialAnchoredPos;
    private Vector3 initialScale;
    private Sequence idleSequence;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            initialAnchoredPos = rectTransform.anchoredPosition;
            initialScale = rectTransform.localScale;
        }
    }

    private void OnEnable()
    {
        StartLogoAnimation();
        if (enablePeriodicPop)
        {
            InvokeRepeating(nameof(TriggerJuicyPop), popInterval, popInterval);
        }
    }

    private void OnDisable()
    {
        KillAnimations();
        CancelInvoke(nameof(TriggerJuicyPop));
    }

    private void StartLogoAnimation()
    {
        KillAnimations();

        idleSequence = DOTween.Sequence();

        idleSequence.Append(rectTransform.DOAnchorPosY(initialAnchoredPos.y + floatDistance, floatDuration)
            .SetEase(Ease.InOutSine));
        idleSequence.Join(rectTransform.DORotate(new Vector3(0, 0, tiltAngle), floatDuration)
            .SetEase(Ease.InOutSine));

        idleSequence.Append(rectTransform.DOAnchorPosY(initialAnchoredPos.y - floatDistance, floatDuration)
            .SetEase(Ease.InOutSine));
        idleSequence.Join(rectTransform.DORotate(new Vector3(0, 0, -tiltAngle), floatDuration)
            .SetEase(Ease.InOutSine));

        idleSequence.SetLoops(-1, LoopType.Yoyo);

        rectTransform.DOScale(initialScale * breathScale, breathDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);
    }

    public void TriggerJuicyPop()
    {
        if (rectTransform == null) return;

        rectTransform.DOPunchScale(new Vector3(0.12f, -0.08f, 0f), 0.5f, 6, 0.5f)
            .OnComplete(() => rectTransform.localScale = initialScale);
    }

    private void KillAnimations()
    {
        if (rectTransform != null)
        {
            rectTransform.DOKill();
            rectTransform.anchoredPosition = initialAnchoredPos;
            rectTransform.localScale = initialScale;
            rectTransform.localRotation = Quaternion.identity;
        }

        if (idleSequence != null && idleSequence.IsActive())
        {
            idleSequence.Kill();
        }
    }
}