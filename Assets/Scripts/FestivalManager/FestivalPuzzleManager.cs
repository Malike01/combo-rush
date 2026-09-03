using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; 

public class FestivalPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Ayarları")]
    public Texture2D puzzleImage; 
    public int gridSize = 3;      
    public float pieceSpacing = 5f;

    [Header("UI Referansları")]
    public RectTransform puzzleBoard; 
    public GameObject piecePrefab;    

    private PuzzlePiece[,] boardPieces;
    private float pieceWidth;
    private float pieceHeight;
    private bool isSwapping = false; 

    public void GeneratePuzzle()
    {
        foreach (Transform child in puzzleBoard) { Destroy(child.gameObject); }
        boardPieces = new PuzzlePiece[gridSize, gridSize];

        int texPieceWidth = puzzleImage.width / gridSize;
        int texPieceHeight = puzzleImage.height / gridSize;

        float boardWidth = puzzleBoard.rect.width;
        float boardHeight = puzzleBoard.rect.height;
        pieceWidth = (boardWidth - (pieceSpacing * (gridSize - 1))) / gridSize;
        pieceHeight = (boardHeight - (pieceSpacing * (gridSize - 1))) / gridSize;

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                Rect sliceRect = new Rect(x * texPieceWidth, y * texPieceHeight, texPieceWidth, texPieceHeight);
                Sprite slicedSprite = Sprite.Create(puzzleImage, sliceRect, new Vector2(0.5f, 0.5f));

                GameObject pieceObj = Instantiate(piecePrefab, puzzleBoard);
                RectTransform rectTransform = pieceObj.GetComponent<RectTransform>();
                
                rectTransform.sizeDelta = new Vector2(pieceWidth, pieceHeight);
                rectTransform.anchoredPosition = GetAnchoredPosition(x, y);

                PuzzlePiece piece = pieceObj.GetComponent<PuzzlePiece>();
                
                piece.Setup(slicedSprite, x, y, this);
                boardPieces[x, y] = piece;
            }
        }
        ShufflePuzzle();
    }

    private Vector2 GetAnchoredPosition(int x, int y)
    {
        float startX = -(pieceWidth * gridSize + pieceSpacing * (gridSize - 1)) / 2f + (pieceWidth / 2f);
        float startY = -(pieceHeight * gridSize + pieceSpacing * (gridSize - 1)) / 2f + (pieceHeight / 2f);
        return new Vector2(startX + (x * (pieceWidth + pieceSpacing)), startY + (y * (pieceHeight + pieceSpacing)));
    }

    public void TrySwapPieces(PuzzlePiece pieceA, Vector2Int swipeDirection)
    {
        if (isSwapping) return; 

        int targetX = pieceA.currentX + swipeDirection.x;
        int targetY = pieceA.currentY + swipeDirection.y;

        if (targetX >= 0 && targetX < gridSize && targetY >= 0 && targetY < gridSize)
        {
            PuzzlePiece pieceB = boardPieces[targetX, targetY];
            Swap(pieceA, pieceB);
        }
    }

    private void Swap(PuzzlePiece pieceA, PuzzlePiece pieceB)
    {
        isSwapping = true;

        boardPieces[pieceA.currentX, pieceA.currentY] = pieceB;
        boardPieces[pieceB.currentX, pieceB.currentY] = pieceA;

        int tempX = pieceA.currentX;
        int tempY = pieceA.currentY;
        
        pieceA.currentX = pieceB.currentX;
        pieceA.currentY = pieceB.currentY;
        
        pieceB.currentX = tempX;
        pieceB.currentY = tempY;

        RectTransform rectA = pieceA.GetComponent<RectTransform>();
        RectTransform rectB = pieceB.GetComponent<RectTransform>();

        Vector2 posA = rectA.anchoredPosition;
        Vector2 posB = rectB.anchoredPosition;

        rectA.DOAnchorPos(posB, 0.2f);
        rectB.DOAnchorPos(posA, 0.2f).OnComplete(() => 
        {
            isSwapping = false;
            CheckWinCondition(); 
        });
    }

    private void CheckWinCondition()
    {
        bool isWin = true;
        foreach (PuzzlePiece piece in boardPieces)
        {
            if (!piece.IsInCorrectPosition())
            {
                isWin = false;
                break;
            }
        }

        if (isWin)
        {
            int festivalAward = Random.Range(50, 201); 
            Vector3 boardCenterPos = puzzleBoard.transform.position;
            Transform globalCoinUI = GlobalCoinManager.Instance.totalCoinText.transform.parent;

            if (CoinFlyEffect.Instance != null)
            {
                CoinFlyEffect.Instance.PlayCoinFlyAnimation(boardCenterPos, globalCoinUI, festivalAward, () =>
                {
                    GlobalCoinManager.Instance.AddGlobalCoins(festivalAward);
                });
            }
            else
            {
                GlobalCoinManager.Instance.AddGlobalCoins(festivalAward);
            }
        }
    }

    private void ShufflePuzzle()
    {
        int shuffleCount = gridSize * gridSize * 3; 

        for (int i = 0; i < shuffleCount; i++)
        {
            int x1 = Random.Range(0, gridSize);
            int y1 = Random.Range(0, gridSize);
            int x2 = Random.Range(0, gridSize);
            int y2 = Random.Range(0, gridSize);

            if (x1 == x2 && y1 == y2) continue; 

            PuzzlePiece pieceA = boardPieces[x1, y1];
            PuzzlePiece pieceB = boardPieces[x2, y2];

            boardPieces[x1, y1] = pieceB;
            boardPieces[x2, y2] = pieceA;

            int tempX = pieceA.currentX;
            int tempY = pieceA.currentY;
            pieceA.currentX = pieceB.currentX;
            pieceA.currentY = pieceB.currentY;
            pieceB.currentX = tempX;
            pieceB.currentY = tempY;

            RectTransform rectA = pieceA.GetComponent<RectTransform>();
            RectTransform rectB = pieceB.GetComponent<RectTransform>();
            Vector2 tempPos = rectA.anchoredPosition;
            
            rectA.anchoredPosition = rectB.anchoredPosition;
            rectB.anchoredPosition = tempPos;
        }
        
        Debug.Log("Puzzle başarıyla karıştırıldı! Oyun hazır.");
    }
}