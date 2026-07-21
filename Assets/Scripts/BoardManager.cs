using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public int width = 6;
    public int height = 8;
    public GameObject jellyPrefab;
    public Color[] jellyColors;

    public Jelly[,] grid;

    public bool IsProcessing { get; private set; } = false;

    // The amount of shift calculated to center the grid exactly
    private Vector2 boardOffset;

    void Start()
    {
        // Offset calculation to align the center of a 6x8 grid with the (0,0) world coordinate
        boardOffset = new Vector2((width - 1) / 2f, (height - 1) / 2f);
        
        grid = new Jelly[width, height];
        GenerateBoard();
    }

    // A helpful method that converts grid coordinates (x,y) to Earth position
    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(x - boardOffset.x, y - boardOffset.y);
    }

    void GenerateBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SpawnJellyAt(x, y);
            }
        }
    }

    void SpawnJellyAt(int x, int y)
    {
        Vector2 position = GetWorldPosition(x, y);
        GameObject newJellyObj = Instantiate(jellyPrefab, position, Quaternion.identity, transform);
        newJellyObj.name = $"Jelly ({x},{y})";

        Jelly jellyScript = newJellyObj.GetComponent<Jelly>();
        int randomColorIndex = Random.Range(0, jellyColors.Length);
        
        jellyScript.Setup(x, y, randomColorIndex, jellyColors[randomColorIndex]);
        grid[x, y] = jellyScript;
    }

    public void ProcessMatches(List<Jelly> matchedJellies)
    {
        if (IsProcessing) return;
        StartCoroutine(ProcessMatchesRoutine(matchedJellies));
    }

    private IEnumerator ProcessMatchesRoutine(List<Jelly> matchedJellies)
    {
        IsProcessing = true;

        foreach (Jelly jelly in matchedJellies)
        {
            if (jelly != null)
            {
                grid[jelly.x, jelly.y] = null;
                Destroy(jelly.gameObject);
            }
        }

        yield return new WaitForSeconds(0.15f);

        yield return StartCoroutine(CollapseColumns());
        yield return StartCoroutine(RefillBoard());

        IsProcessing = false;
    }

    private IEnumerator CollapseColumns()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    for (int aboveY = y + 1; aboveY < height; aboveY++)
                    {
                        if (grid[x, aboveY] != null)
                        {
                            Jelly fallingJelly = grid[x, aboveY];
                            grid[x, y] = fallingJelly;
                            grid[x, aboveY] = null;

                            fallingJelly.x = x;
                            fallingJelly.y = y;
                            fallingJelly.gameObject.name = $"Jelly ({x},{y})";

                            StartCoroutine(MoveJellyToPosition(fallingJelly, GetWorldPosition(x, y)));
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(0.25f);
    }

    private IEnumerator RefillBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    Vector2 targetWorldPos = GetWorldPosition(x, y);
                    Vector2 spawnPos = new Vector2(targetWorldPos.x, targetWorldPos.y + 5f); // Drop off screen

                    GameObject newJellyObj = Instantiate(jellyPrefab, spawnPos, Quaternion.identity, transform);
                    
                    Jelly jellyScript = newJellyObj.GetComponent<Jelly>();
                    int randomColorIndex = Random.Range(0, jellyColors.Length);
                    jellyScript.Setup(x, y, randomColorIndex, jellyColors[randomColorIndex]);
                    newJellyObj.name = $"Jelly ({x},{y})";

                    grid[x, y] = jellyScript;
                    
                    StartCoroutine(MoveJellyToPosition(jellyScript, targetWorldPos));
                }
            }
        }
        yield return new WaitForSeconds(0.25f);
    }

    private IEnumerator MoveJellyToPosition(Jelly jelly, Vector2 targetPos)
    {
        if (jelly == null) yield break;

        float elapsedTime = 0f;
        float duration = 0.2f;
        Vector2 startPos = jelly.transform.position;

        while (elapsedTime < duration)
        {
            if (jelly == null) yield break;

            jelly.transform.position = Vector2.Lerp(startPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (jelly != null)
        {
            jelly.transform.position = targetPos;
        }
    }
}