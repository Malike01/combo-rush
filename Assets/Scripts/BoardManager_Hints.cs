using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardManager : MonoBehaviour
{
    private Coroutine hintTimerCoroutine;
    private List<Jelly> currentHintJellies = new List<Jelly>();
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

        List<Jelly> bestGroup = FindLargestConnectedGroup();

        if (bestGroup != null && bestGroup.Count > 0)
        {
            currentHintJellies = bestGroup;
            isHintActive = true;

            foreach (Jelly jelly in currentHintJellies)
            {
                if (jelly != null)
                {
                    jelly.StartIdleWiggle(); 
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

    private List<Jelly> FindLargestConnectedGroup()
    {
        bool[,] visited = new bool[width, height];
        List<Jelly> largestGroup = new List<Jelly>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y] != null && !visited[x, y])
                {
                    List<Jelly> currentGroup = GetConnectedGroup(x, y, visited);
                    
                    if (currentGroup.Count >= 4 && currentGroup.Count > largestGroup.Count)
                    {
                        largestGroup = currentGroup;
                    }
                }
            }
        }

        return largestGroup; 
    }

    private List<Jelly> GetConnectedGroup(int startX, int startY, bool[,] visited)
    {
        List<Jelly> group = new List<Jelly>();
        int targetTheme = board[startX, startY].colorIndex; 

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            Jelly currentJelly = board[current.x, current.y];
            group.Add(currentJelly);

            foreach (var dir in directions)
            {
                int nextX = current.x + dir.x;
                int nextY = current.y + dir.y;

                if (nextX >= 0 && nextX < width && nextY >= 0 && nextY < height)
                {
                    if (!visited[nextX, nextY] && board[nextX, nextY] != null)
                    {
                        if (board[nextX, nextY].colorIndex == targetTheme)
                        {
                            visited[nextX, nextY] = true;
                            queue.Enqueue(new Vector2Int(nextX, nextY));
                        }
                    }
                }
            }
        }

        return group;
    }

    private IEnumerator AnimateHintJelly(Transform jellyTransform)
    {
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = originalScale * 1.0f; 

        while (isHintActive && jellyTransform != null)
        {
            float elapsed = 0f;
            float duration = 0.8f;

            while (elapsed < duration && isHintActive && jellyTransform != null)
            {
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                jellyTransform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < duration && isHintActive && jellyTransform != null)
            {
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                jellyTransform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(0.3f);
        }

        if (jellyTransform != null)
        {
            jellyTransform.localScale = originalScale;
        }
    }
}