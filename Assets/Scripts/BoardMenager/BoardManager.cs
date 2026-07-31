using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Defines the visual themes for the coins (colors)
[System.Serializable]
public struct JellyTheme
{
    public string themeName; 
    [Tooltip("Index Maps -> 0: 5, 1: 10, 2: 15, 3: 20, 4: 25(Ultra), 5: Mystery(?)")]
    public Sprite[] progressionSprites; 
}

// Defines the logic, point values, and drop rates for each coin type within a theme
[System.Serializable]
public struct CoinDropData
{
    public string dropName;      // Identifier for the Inspector (e.g., "Normal 5", "Ultra 25")
    public int pointValue;       // The actual score value of the coin (5, 10, 15, 20, 25, 0 for Mystery)
    public int spriteIndex;      // Which sprite index to use from the JellyTheme's progressionSprites array
    public float dropWeight;     // The probability weight (higher = more frequent)
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

    [Header("Jelly Themes & Drop Rates")]
    public JellyTheme[] jellyThemes;
    [Tooltip("Define the probabilities and values for 5, 10, 15, 20, 25(Ultra) and ?(Mystery)")]
    public CoinDropData[] coinDropSettings; 

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
        // Singleton pattern implementation
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // Null checks for crucial references before starting
        if (coinPrefab == null || jellyThemes == null || jellyThemes.Length == 0)
        {
            Debug.LogError("BoardManager: Missing Prefab or Coin Themes array!");
            return;
        }
        
        if (coinDropSettings == null || coinDropSettings.Length == 0)
        {
            Debug.LogWarning("BoardManager: Coin Drop Settings are not assigned in the Inspector!");
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

        // Clear existing cells and coins before generating a new board
        if (gridParent != null)
        {
            for (int i = gridParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(gridParent.GetChild(i).gameObject); 
            }
        }

        board = new Coin[width, height]; 

        CreateBoardBackground();   
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Skip inactive cells based on level design
                if (activeCells != null && !activeCells[x, y]) continue;

                Vector2 pos = GetWorldPosition(x, y);

                // Generate background visual cell
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

                // Generate the actual coin (Jelly)
                SpawnJellyAt(x, y, new Vector3(pos.x, pos.y, -1f));
            }
        }
    }

    private void ParseGridLayout()
    {
        activeCells = new bool[width, height];

        // If no layout is provided, make all cells active (rectangular grid)
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
            sr.color = new Color(1f, 1f, 1f, 0.15f);
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

    // Determines the drop type based on the assigned weights in the Inspector
    private CoinDropData GetRandomCoinDropData()
    {
        if (coinDropSettings == null || coinDropSettings.Length == 0)
        {
            // Fallback in case settings are missing
            return new CoinDropData { pointValue = 10, spriteIndex = 0, dropWeight = 1f };
        }

        float totalWeight = 0f;
        foreach (var drop in coinDropSettings)
        {
            totalWeight += drop.dropWeight;
        }

        float randomVal = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var drop in coinDropSettings)
        {
            currentWeight += drop.dropWeight;
            if (randomVal <= currentWeight)
            {
                return drop;
            }
        }

        // Failsafe
        return coinDropSettings[0];
    }

    // YENI EKLENEN FONKSIYON: Mystery Jetonlarin Puanini Rastgele Belirler
    private int CalculateFinalPointValue(int initialValue)
    {
        // Eger Inspector'dan puan 0 (Mystery) ayarlandiysa, icerisine rastgele gercek bir puan ata
        if (initialValue == 0)
        {
            int[] mysteryValues = new int[] { 5, 10, 15, 20 };
            return mysteryValues[Random.Range(0, mysteryValues.Length)];
        }
        
        return initialValue; // Degilse kendi orjinal puanini dondur
    }

    void SpawnJellyAt(int x, int y, Vector3 spawnPos)
    {
        // 1. Pick a random color theme
        int randomThemeIndex = Random.Range(0, jellyThemes.Length);
        
        // 2. Pick a random type/value based on weights
        CoinDropData dropData = GetRandomCoinDropData();

        Transform parentTransform = gridParent != null ? gridParent : transform;
        GameObject coinObj = Instantiate(coinPrefab, spawnPos, Quaternion.identity, parentTransform);
        coinObj.name = $"Coin ({x},{y})";

        Coin coinScript = coinObj.GetComponent<Coin>();
        if (coinScript != null)
        {
            coinScript.x = x;
            coinScript.y = y;

            Sprite selectedSprite = null;
            // Ensure the sprite index exists in the current theme's array to prevent OutOfBounds errors
            if (dropData.spriteIndex < jellyThemes[randomThemeIndex].progressionSprites.Length)
            {
                selectedSprite = jellyThemes[randomThemeIndex].progressionSprites[dropData.spriteIndex];
            }

            // Puan hesaplamasindan gecir (Mystery ise rastgele 5, 10, 15, 20 atar)
            int finalValue = CalculateFinalPointValue(dropData.pointValue);

            // Assign the correct sprite, color index, and calculated point value
            coinScript.SetupCoin(selectedSprite, randomThemeIndex, finalValue);
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

                    // 1. Pick random color
                    int randomThemeIndex = Random.Range(0, jellyThemes.Length);
                    
                    // 2. Pick random type/value based on weights
                    CoinDropData dropData = GetRandomCoinDropData();
                    
                    GameObject coinObj = Instantiate(coinPrefab, spawnAbovePos, Quaternion.identity, parentTransform);
                    coinObj.name = $"Coin ({x},{y})";

                    Coin coinScript = coinObj.GetComponent<Coin>();
                    if (coinScript != null)
                    {
                        coinScript.x = x;
                        coinScript.y = y;

                        Sprite selectedSprite = null;
                        if (dropData.spriteIndex < jellyThemes[randomThemeIndex].progressionSprites.Length)
                        {
                            selectedSprite = jellyThemes[randomThemeIndex].progressionSprites[dropData.spriteIndex];
                        }

                        // Puan hesaplamasindan gecir (Mystery ise rastgele atar)
                        int finalValue = CalculateFinalPointValue(dropData.pointValue);

                        // Set the specific data for the spawned coin
                        coinScript.SetupCoin(selectedSprite, randomThemeIndex, finalValue);
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