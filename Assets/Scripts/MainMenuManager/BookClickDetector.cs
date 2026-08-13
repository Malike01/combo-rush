using UnityEngine;

public class BookClickDetector : MonoBehaviour
{
    [Header("Sistem Bağlantısı")]
    public BookMenuManager menuManager;

    [Header("Özel İmleç (Opsiyonel)")]
    public Texture2D customPointerCursor; 

    private void OnMouseEnter()
    {
        if (customPointerCursor != null)
        {
            Cursor.SetCursor(customPointerCursor, Vector2.zero, CursorMode.Auto);
        }
        else
        {

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); 
        }
    }

    private void OnMouseExit()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void OnMouseDown()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        if (menuManager != null)
        {
            menuManager.EnterMinitureWorld();
        }
    }
}