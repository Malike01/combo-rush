using UnityEngine;

public class Jelly : MonoBehaviour
{
    public int x;
    public int y;
    public int colorIndex;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        // We get the SpriteRenderer component from the object
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Setup(int newX, int newY, int newColorIndex, Color newColor)
    {
        x = newX;
        y = newY;
        colorIndex = newColorIndex;

        // We secure the component if Awake hasn't been triggered yet
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // THIS IS WHERE THE COLOR CHANGE TAKES PLACE
        spriteRenderer.color = newColor; 
    }
}