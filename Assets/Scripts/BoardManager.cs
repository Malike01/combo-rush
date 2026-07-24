using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct JellyTheme
{
    public string themeName; 
    [Tooltip("0: Default, 1: Combo 1, 2: Combo 2...")]
    public Sprite[] progressionSprites; 
}

public partial class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    public int width = 6;
    public int height = 8;
    public float spacing = 1.2f;

    [Header("References")]
    public GameObject jellyPrefab;

    [Header("Jelly Evolutions")]
    public JellyTheme[] jellyThemes;

    [Header("Grid Visuals")]
    public GameObject cellPrefab;
    public GameObject boardBackgroundPrefab;
    public Transform gridParent;

    [Header("Jelly Theme Palette")]
    public Color lightCellColor = new Color(1f, 1f, 1f, 0.25f);
    public Color darkCellColor  = new Color(1f, 1f, 1f, 0.12f);

    private Jelly[,] board;
    public bool IsProcessing { get; private set; } = false;

    public static BoardManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (jellyPrefab == null || jellyThemes == null || jellyThemes.Length == 0)
        {
            Debug.LogError("BoardManager: Missing Prefab or Jelly Themes array!");
            return;
        }

        board = new Jelly[width, height];
        GenerateBoard();
        ResetHintTimer();
    }

    void GenerateBoard()
    {
        CreateBoardBackground();    
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = GetWorldPosition(x, y);

                if (cellPrefab != null)
                {
                    Vector3 bgPos = new Vector3(pos.x, pos.y, 0.1f);
                    GameObject cell = Instantiate(cellPrefab, bgPos, Quaternion.identity, gridParent);
                    cell.name = $"Cell_{x}_{y}";
                    cell.transform.localScale = new Vector3(1.15f, 1.15f, 1f);

                    SpriteRenderer cellSR = cell.GetComponent<SpriteRenderer>();
                    if (cellSR != null)
                    {
                        bool isEven = (x + y) % 2 == 0;
                        cellSR.color = isEven ? lightCellColor : darkCellColor;
                        cellSR.sortingOrder = 0; 
                    }
                }

                SpawnJellyAt(x, y, new Vector3(pos.x, pos.y, -1f));
            }
        }
    }

    void CreateBoardBackground()
    {
        if (boardBackgroundPrefab == null) return;

        Vector3 centerPos = GetWorldPosition((width - 1) / 2f, (height - 1) / 2f);
        float paddingX = 0.8f; 
        float paddingY = 0.8f; 
        float totalWidth = (52 * spacing) + paddingX;
        float totalHeight = (62 * spacing) + paddingY;

        Vector3 bgPos = new Vector3(centerPos.x, centerPos.y, 0.5f);
        GameObject bgObj = Instantiate(boardBackgroundPrefab, bgPos, Quaternion.identity, gridParent);
        bgObj.name = "Board_Main_Background";
        bgObj.transform.localScale = new Vector3(totalWidth, totalHeight, 1f);

        SpriteRenderer sr = bgObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(0.08f, 0.09f, 0.22f, 0.65f); 
            sr.sortingOrder = -5;
        }
    }

    public Vector3 GetWorldPosition(float x, float y)
    {
        float localX = -((width - 1) * spacing) / 2f + (x * spacing);
        float localY = -((height - 1) * spacing) / 2f + (y * spacing);
        Vector3 parentPos = gridParent != null ? gridParent.position : Vector3.zero;

        return new Vector3(localX + parentPos.x, localY + parentPos.y, 0f);
    }

    void SpawnJellyAt(int x, int y, Vector3 spawnPos)
    {
        int randomIndex = Random.Range(0, jellyThemes.Length);

        GameObject jellyObj = Instantiate(jellyPrefab, spawnPos, Quaternion.identity, transform);
        jellyObj.name = $"Jelly ({x},{y})";

        Jelly jellyScript = jellyObj.GetComponent<Jelly>();
        if (jellyScript != null)
        {
            jellyScript.x = x;
            jellyScript.y = y;
            jellyScript.SetupEvolution(jellyThemes[randomIndex].progressionSprites, randomIndex);
            board[x, y] = jellyScript;
        }
    }

    public void ProcessMatches(List<Jelly> matchedJellies)
    {
        if (matchedJellies == null || matchedJellies.Count == 0) return;

        ResetHintTimer();

        if (ComboTimerManager.Instance != null) 
        {
            ComboTimerManager.Instance.StartOrResetTimer();
        }

        if (ScoreManager.Instance != null) 
    {
        ScoreManager.Instance.AddScore(matchedJellies.Count);
    }

        Vector3 burstPoint = matchedJellies[matchedJellies.Count - 1].transform.position;
        SpriteRenderer sr = matchedJellies[0].GetComponent<SpriteRenderer>();
        Color burstColor = sr != null ? sr.color : Color.white;

        if (ParticleFXManager.Instance != null)
        {
            ParticleFXManager.Instance.PlayBurst(burstPoint, burstColor, matchedJellies.Count);
        }

        StartCoroutine(ProcessMatchesRoutine(matchedJellies));
    }

    private IEnumerator ProcessMatchesRoutine(List<Jelly> matchedJellies)
    {
        IsProcessing = true;

        for (int i = 0; i < matchedJellies.Count; i++)
        {
            Jelly jelly = matchedJellies[i];
            if (jelly != null)
            {
                board[jelly.x, jelly.y] = null;
                jelly.transform.SetParent(null);
                StartCoroutine(FlyJellyToUI(jelly.gameObject, i * 0.08f));
            }
        }

        yield return new WaitForSeconds(0.15f);
        yield return StartCoroutine(CollapseGridRoutine());
        yield return StartCoroutine(RefillGridRoutine());

        IsProcessing = false;
        CheckForPossibleMoves();
    }

    private IEnumerator CollapseGridRoutine()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y] == null)
                {
                    for (int aboveY = y + 1; aboveY < height; aboveY++)
                    {
                        if (board[x, aboveY] != null)
                        {
                            Jelly movingJelly = board[x, aboveY];
                            board[x, y] = movingJelly;
                            board[x, aboveY] = null;

                            movingJelly.x = x;
                            movingJelly.y = y;
                            movingJelly.transform.name = $"Jelly ({x},{y})";

                            Vector3 targetPos = new Vector3(GetWorldPosition(x, y).x, GetWorldPosition(x, y).y, -1f);
                            StartCoroutine(SmoothMove(movingJelly.transform, targetPos, 0.15f));
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(0.18f);
    }

    private IEnumerator RefillGridRoutine()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y] == null)
                {
                    Vector3 spawnAbovePos = new Vector3(GetWorldPosition(x, height).x, GetWorldPosition(x, height).y, -1f); 
                    Vector3 targetPos = new Vector3(GetWorldPosition(x, y).x, GetWorldPosition(x, y).y, -1f);

                    int randomIndex = Random.Range(0, jellyThemes.Length);
                    GameObject jellyObj = Instantiate(jellyPrefab, spawnAbovePos, Quaternion.identity, transform);
                    jellyObj.name = $"Jelly ({x},{y})";

                    Jelly jellyScript = jellyObj.GetComponent<Jelly>();
                    if (jellyScript != null)
                    {
                        jellyScript.x = x;
                        jellyScript.y = y;
                        jellyScript.SetupEvolution(jellyThemes[randomIndex].progressionSprites, randomIndex);
                        board[x, y] = jellyScript;

                        StartCoroutine(SmoothMove(jellyObj.transform, targetPos, 0.2f));
                    }
                }
            }
        }
        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator SmoothMove(Transform target, Vector3 endPos, float duration)
    {
        Vector3 startPos = target.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (target == null) yield break;
            target.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (target != null) target.position = endPos;
    }
}