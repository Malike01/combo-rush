using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; 

public class PuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image pieceImage;
    
    [Header("Koordinatlar")]
    public int originalX;
    public int originalY;
    public int currentX;
    public int currentY;

    private FestivalPuzzleManager manager;
    private Vector2 startTouchPosition;

    public void Setup(Sprite sprite, int x, int y, FestivalPuzzleManager mgr)
    {
        if (pieceImage == null) pieceImage = GetComponent<Image>();
        
        pieceImage.sprite = sprite;
        originalX = x;
        originalY = y;
        currentX = x;
        currentY = y;
        manager = mgr;
        
        gameObject.name = $"Piece_{x}_{y}";
    }

    public bool IsInCorrectPosition()
    {
        return originalX == currentX && originalY == currentY;
    }

    // --- KAYDIRMA (SWIPE) MEKANİĞİ ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        startTouchPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 endTouchPosition = eventData.position;
        Vector2 swipeDelta = endTouchPosition - startTouchPosition;

        if (swipeDelta.magnitude < 30f) return;

        Vector2Int direction = Vector2Int.zero;

        if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
        {
            direction = swipeDelta.x > 0 ? Vector2Int.right : Vector2Int.left; 
        }
        else
        {
            direction = swipeDelta.y > 0 ? Vector2Int.up : Vector2Int.down; 
        }

        manager.TrySwapPieces(this, direction);
    }
}