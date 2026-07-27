using UnityEngine;

public class PlayBtnAnimator : MonoBehaviour
{
    public float speed = 2.5f; 
    public float scaleAmount = 0.05f; 

    private Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
    }

    void Update()
    {
        float wave = Mathf.Sin(Time.time * speed) * scaleAmount;
        transform.localScale = startScale + new Vector3(wave, wave, wave);
    }
}