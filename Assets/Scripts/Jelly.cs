using UnityEngine;

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

    // BoardManager sets Jelly's image by calling this method
    public void SetSprite(Sprite newSprite, int index)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        
        spriteRenderer.sprite = newSprite;
        colorIndex = index;
    }
}