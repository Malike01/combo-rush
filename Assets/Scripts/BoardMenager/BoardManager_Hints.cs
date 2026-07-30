using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardManager : MonoBehaviour
{
    private Coroutine hintTimerCoroutine;
    private List<Coin> currentHintJellies = new List<Coin>();
    private bool isHintActive = false;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            ResetHintTimer();
        }
    }

    public void ResetHintTimer()
    {
        ClearHintEffects();

        if (hintTimerCoroutine != null)
        {
            StopCoroutine(hintTimerCoroutine);
        }

        if (!IsProcessing)
        {
            hintTimerCoroutine = StartCoroutine(HintTimerRoutine());
        }
    }

    private IEnumerator HintTimerRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        ShowBestComboHint();
    }

    public void ShowBestComboHint()
    {
        if (IsProcessing) return;

        List<Coin> bestGroup = FindLargestConnectedGroup();

        if (bestGroup != null && bestGroup.Count >= 4)
        {
            currentHintJellies = bestGroup;
            isHintActive = true;

            foreach (Coin coin in currentHintJellies)
            {
                if (coin != null && coin.gameObject != null)
                {
                    coin.StartIdleWiggle(); 
                }
            }
        }
    }

    public void ClearHintEffects()
    {
        isHintActive = false;
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y] != null && board[x, y].gameObject != null)
                {
                    board[x, y].ResetScale(); 
                }
            }
        }
        
        currentHintJellies.Clear();
    }

    private List<Coin> FindLargestConnectedGroup()
    {
        List<Coin> bestPath = new List<Coin>();
      
        int maxPathSearchCap = 12; 

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y] != null)
                {
                    List<Coin> currentPath = new List<Coin>();
                    bool[,] visited = new bool[width, height];
                    
                    DFS_LongestPath(x, y, board[x, y].colorIndex, currentPath, visited, ref bestPath, maxPathSearchCap);
                    
                    if (bestPath.Count >= maxPathSearchCap) 
                        return bestPath; 
                }
            }
        }

        return bestPath; 
    }

    private void DFS_LongestPath(int x, int y, int targetTheme, List<Coin> currentPath, bool[,] visited, ref List<Coin> bestPath, int maxCap)
    {
        if (bestPath.Count >= maxCap || currentPath.Count >= maxCap) return;

        visited[x, y] = true;
        currentPath.Add(board[x, y]);

        if (currentPath.Count > bestPath.Count)
        {
            bestPath = new List<Coin>(currentPath);
        }

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
        {
            int nextX = x + dir.x;
            int nextY = y + dir.y;

            if (nextX >= 0 && nextX < width && nextY >= 0 && nextY < height)
            {
                if (!visited[nextX, nextY] && board[nextX, nextY] != null)
                {
                    if (board[nextX, nextY].colorIndex == targetTheme)
                    {
                        DFS_LongestPath(nextX, nextY, targetTheme, currentPath, visited, ref bestPath, maxCap);
                    }
                }
            }
        }

        visited[x, y] = false;
        currentPath.RemoveAt(currentPath.Count - 1);
    }
}