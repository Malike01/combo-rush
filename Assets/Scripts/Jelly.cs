using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Jelly : MonoBehaviour
{
    public int x;
    public int y;
    public int colorIndex;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // BoardManager bu metodu çağırarak Jelly'nin görselini set eder
    public void SetSprite(Sprite newSprite, int index)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        
        spriteRenderer.sprite = newSprite;
        colorIndex = index;
    }

    public IEnumerator ShakeJelly(float duration = 0.15f, float magnitude = 0.1f)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }

}