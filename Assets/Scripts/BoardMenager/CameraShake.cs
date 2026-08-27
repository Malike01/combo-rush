using UnityEngine;
using DG.Tweening;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Vector3 initialPosition;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        initialPosition = transform.position;
    }

    public void TriggerShake(float duration = 0.3f, float strength = 0.4f, int vibrato = 25)
    {
        transform.DOKill();
        transform.position = initialPosition;

        Vector3 shakeStrength = new Vector3(strength, strength, 0f);

        transform.DOShakePosition(duration, shakeStrength, vibrato, 90f, false, true)
            .OnComplete(() =>
            {
                transform.position = initialPosition;
            });
    }
}