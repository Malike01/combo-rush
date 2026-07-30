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

    [Header("Level Data")]
    public LevelData currentLevel;
    public bool[,] activeCells;

    [Header("References")]
    public GameObject coinPrefab;

    [Header("Jelly Evolutions")]
    public JellyTheme[] jellyThemes;

    [Header("Grid Visuals")]
    public GameObject cellPrefab;
    public GameObject boardBackgroundPrefab;
    public Transform gridParent;

    [Header("Jelly Theme Palette")]
    public Color lightCellColor = new Color(1f, 1f, 1f, 0.25f);
    public Color darkCellColor  = new Color(1f, 1f, 1f, 0.12f);

    private Coin[,] board;
    public bool IsProcessing { get; private set; } = false;

    public static BoardManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (coinPrefab == null || jellyThemes == null || jellyThemes.Length == 0)
        {
            Debug.LogError("BoardManager: Missing Prefab or Coin Themes array!");
            return;
        }

        board = new Coin[width, height];

        ResetHintTimer();
    }

    public void LoadLevel(LevelData level)
    {
        currentLevel = level;
        width = level.gridWidth;
        height = level.gridHeight;

        ParseGridLayout();
        GenerateBoard();
    }


   public void GenerateBoard()
    {
        if (currentLevel != null)
        {
            width = currentLevel.gridWidth;
            height = currentLevel.gridHeight;
            ParseGridLayout(); 
        }

        if (gridParent != null)
        {
            for (int i = gridParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(gridParent.GetChild(i).gameObject); 
            }
        }

        board = new Coin[width, height]; 
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (activeCells != null && !activeCells[x, y]) continue;

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

    private void ParseGridLayout()
    {
        activeCells = new bool[width, height];

        if (currentLevel == null || string.IsNullOrWhiteSpace(currentLevel.gridLayout))
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    activeCells[x, y] = true;
            return;
        }

        string cleanLayout = currentLevel.gridLayout.Replace("\n", "").Replace("\r", "").Replace(" ", "");

        int charIndex = 0;
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (charIndex < cleanLayout.Length)
                {
                    activeCells[x, y] = (cleanLayout[charIndex] == '1');
                    charIndex++;
                }
                else
                {
                    activeCells[x, y] = false; 
                }
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
            sr.sortingOrder = 1;
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

        Transform parentTransform = gridParent != null ? gridParent : transform;
        GameObject coinObj = Instantiate(coinPrefab, spawnPos, Quaternion.identity, parentTransform);
        coinObj.name = $"Coin ({x},{y})";

        Coin coinScript = coinObj.GetComponent<Coin>();
        if (coinScript != null)
        {
            coinScript.x = x;
            coinScript.y = y;
            coinScript.SetupCoin(jellyThemes[randomIndex].progressionSprites[0], randomIndex, 10);
            board[x, y] = coinScript;
        }
    }

    public void OnBoardInteractionStart()
    {
        if (ComboTimerManager.Instance != null)
        {
            ComboTimerManager.Instance.StartOrResetTimer();
        }
    }

    public void ProcessMatches(List<Coin> matchedCoins)
    {
        if (matchedCoins == null || matchedCoins.Count == 0) return;

        ResetHintTimer();

        if (ComboTimerManager.Instance != null) 
        {
            ComboTimerManager.Instance.StartOrResetTimer();
        }

      if (ScoreManager.Instance != null) 
        {
            int totalMatchValue = 0;
            foreach (Coin coin in matchedCoins) 
            {
                totalMatchValue += coin.pointValue;
            }

            ScoreManager.Instance.AddScore(totalMatchValue, matchedCoins.Count);
        }

        Vector3 burstPoint = matchedCoins[matchedCoins.Count - 1].transform.position;
        SpriteRenderer sr = matchedCoins[0].GetComponent<SpriteRenderer>();
        Color burstColor = sr != null ? sr.color : Color.white;

        if (ParticleFXManager.Instance != null)
        {
            ParticleFXManager.Instance.PlayBurst(burstPoint, burstColor, matchedCoins.Count);
        }

        StartCoroutine(ProcessMatchesRoutine(matchedCoins));
    }

    private IEnumerator ProcessMatchesRoutine(List<Coin> matchedCoins)
    {
        IsProcessing = true;

        for (int i = 0; i < matchedCoins.Count; i++)
        {
            Coin coin = matchedCoins[i];
            if (coin != null)
            {
                board[coin.x, coin.y] = null;
                coin.transform.SetParent(null);
                StartCoroutine(FlyJellyToUI(coin.gameObject, i * 0.08f));
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
                if (activeCells != null && !activeCells[x, y]) continue;

                if (board[x, y] == null)
                {
                    for (int aboveY = y + 1; aboveY < height; aboveY++)
                    {
                        if (activeCells != null && !activeCells[x, y]) continue;
                        if (board[x, aboveY] != null)
                        {
                            Coin movingCoins = board[x, aboveY];
                            board[x, y] = movingCoins;
                            board[x, aboveY] = null;

                            movingCoins.x = x;
                            movingCoins.y = y;
                            movingCoins.transform.name = $"Coin ({x},{y})";

                            Vector3 targetPos = new Vector3(GetWorldPosition(x, y).x, GetWorldPosition(x, y).y, -1f);
                            StartCoroutine(SmoothMove(movingCoins.transform, targetPos, 0.15f));
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
        Transform parentTransform = gridParent != null ? gridParent : transform;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (activeCells != null && !activeCells[x, y]) continue;

                if (board[x, y] == null)
                {
                    Vector3 spawnAbovePos = new Vector3(GetWorldPosition(x, height).x, GetWorldPosition(x, height).y, -1f); 
                    Vector3 targetPos = new Vector3(GetWorldPosition(x, y).x, GetWorldPosition(x, y).y, -1f);

                    int randomIndex = Random.Range(0, jellyThemes.Length);
                    
                    GameObject coinObj = Instantiate(coinPrefab, spawnAbovePos, Quaternion.identity, parentTransform);
                    coinObj.name = $"Coin ({x},{y})";

                    Coin coinScript = coinObj.GetComponent<Coin>();
                    if (coinScript != null)
                    {
                        coinScript.x = x;
                        coinScript.y = y;
                        coinScript.SetupCoin(jellyThemes[randomIndex].progressionSprites[0], randomIndex, 10);
                        board[x, y] = coinScript;

                        StartCoroutine(SmoothMove(coinObj.transform, targetPos, 0.2f));
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
