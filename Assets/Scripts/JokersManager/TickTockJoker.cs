using UnityEngine;
using DG.Tweening;
using System.Collections;

public class TickTockJoker : JokerBase
{
    [Header("Saat Görselleri")]
    public Transform clockVisual; 
    public Transform blueGlowRing; 
    public float rotationDuration = 0.4f;

    protected override void InitializeJoker()
    {
        jokerName = "Tick-Tock Clock";
        InputManager.OnPlayerMoveExecuted += ExecuteJokerEffect;

        if (blueGlowRing != null)
        {
            blueGlowRing.DORotate(new Vector3(0, 0, -360), 4f, RotateMode.FastBeyond360)
                        .SetLoops(-1, LoopType.Restart)
                        .SetEase(Ease.Linear);
        }
    }

    protected override void CleanupJoker()
    {
        InputManager.OnPlayerMoveExecuted -= ExecuteJokerEffect;
        
        if (blueGlowRing != null) blueGlowRing.DOKill();
    }
    public override void ExecuteJokerEffect()
    {
        if (clockVisual != null)
        {
            clockVisual.DOKill(true);

            clockVisual.DORotate(new Vector3(0, 0, -90f), rotationDuration, RotateMode.LocalAxisAdd)
                       .SetEase(Ease.OutBack);

            clockVisual.DOPunchScale(new Vector3(0.15f, 0.15f, 0f), 0.3f, 6, 0.5f);

            clockVisual.DOShakePosition(0.3f, 0.05f, 15, 90f);
            
        }

        StartCoroutine(ShiftBoardColorsRoutine());
    }

    private IEnumerator ShiftBoardColorsRoutine()
    {
        yield return new WaitForSeconds(rotationDuration * 0.5f);

        Coin[] allCoinsOnBoard = Object.FindObjectsByType<Coin>(FindObjectsSortMode.None);
        foreach (Coin coin in allCoinsOnBoard)
        {
            if (coin != null) coin.ShiftColorToNext();
        }
    }
}