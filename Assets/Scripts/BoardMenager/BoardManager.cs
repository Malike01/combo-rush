using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

// Defines the visual themes for the coins (colors)
[System.Serializable]
public struct JellyTheme
{
    public string themeName; 
    [Tooltip("Index Maps -> 0: 5, 1: 10, 2: 15, 3: 20, 4: 25(Ultra), 5: Variant6, 6: Mystery(?)")]
    public Sprite[] progressionSprites; 
}

// Defines the logic, point values, and drop rates for each coin type within a theme
[System.Serializable]
public struct CoinDropData
{
    public string dropName;      
    public int pointValue;       
    public int spriteIndex;      
    public float dropWeight;     
}

public partial class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    public int width = 6;
    public int height = 8;
    public float spacing = 1.2f;
    [Tooltip("The player must reach level 5 or higher for Mystery coins to spawn.")]
    public int currentPlayerLevel = 1;

    [Header("Score Settings (Algorithmic Increase)")]
    public float comboMultiplierStep = 0.25f; // Each extra coin after the 4th multiplies the score by this step (+25%)

    [Header("Level Data")]
    public LevelData currentLevel;
    public bool[,] activeCells;

    [Header("References")]
    public GameObject coinPrefab;

    [Header("Jelly Themes & Drop Rates")]
    public JellyTheme[] jellyThemes;
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

    [Header("Jokers")]
    public GameObject tickTockJokerPrefab;
    private GameObject activeJokerInstance;

    public static BoardManager Instance;

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null) Instance = this;
    }

    void Start()
    {
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

    public void ResetProcessingState()
    {
        StopAllCoroutines();
        IsProcessing = false;
    }

    public void LoadLevel(LevelData level)
    {
        ResetProcessingState();
        currentLevel = level;
        width = level.gridWidth;
        height = level.gridHeight;

        if (LevelManager.Instance != null)
        {
            currentPlayerLevel = LevelManager.Instance.currentLevelIndex + 1;
        }

        ParseGridLayout();
        GenerateBoard();
    }

   public void GenerateBoard()
    {
        IsProcessing = false;
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
                Transform child = gridParent.GetChild(i);
                if (child.name == "BoardFrame") 
                {
                    continue; 
                }
                DestroyImmediate(child.gameObject); 
            }
        }

        board = new Coin[width, height]; 

        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (activeCells != null && !activeCells[x, y]) continue;

                Vector2 pos = GetWorldPosition(x, y);

                SpawnJellyAt(x, y, new Vector3(pos.x, pos.y, -1f));
            }
        }

        if (!HasAnyValidMatch())
        {
            StartCoroutine(ShuffleBoardRoutine());
        }

        if (InputManager.Instance != null)
        {
            InputManager.Instance.ResetInputState();
        }

        SpawnCenterJokerIfNeeded();
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

        // Safely split the layout string by line breaks
        string[] rows = currentLevel.gridLayout.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int y = 0; y < height; y++)
        {
            int rowIndex = (height - 1) - y;

            for (int x = 0; x < width; x++)
            {
                // Check if the character exists safely
                if (rowIndex >= 0 && rowIndex < rows.Length && x < rows[rowIndex].Length)
                {
                    activeCells[x, y] = (rows[rowIndex][x] == '1');
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
        float paddingX = 0.6f; 
        float paddingY = 0.6f; 
        
        float totalWidth = (width * spacing * 9) + paddingX;
        float totalHeight = (height * spacing * 7) + paddingY;

        Vector3 bgPos = new Vector3(centerPos.x, centerPos.y, 0.5f);
        GameObject bgObj = Instantiate(boardBackgroundPrefab, bgPos, Quaternion.identity, gridParent);
        bgObj.name = "Board_Main_Background";
        bgObj.transform.localScale = new Vector3(totalWidth, totalHeight, 1f);

        SpriteRenderer sr = bgObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(1f, 1f, 1f, 0.4f);
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

    // --- Level-Sensitive Random Drop Manager ---
    private CoinDropData GetRandomCoinDropData()
    {
        if (coinDropSettings == null || coinDropSettings.Length == 0)
        {
            return new CoinDropData { pointValue = 10, spriteIndex = 0, dropWeight = 1f };
        }

        float totalWeight = 0f;
        List<CoinDropData> availableDrops = new List<CoinDropData>();

        foreach (var drop in coinDropSettings)
        {
            // If it is a Mystery Coin (Element 6) and the player has not reached level 5, skip adding it
            bool isMystery = drop.spriteIndex == 6; 
            if (isMystery && currentPlayerLevel < 5) continue;

            availableDrops.Add(drop);
            totalWeight += drop.dropWeight;
        }

        float randomVal = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var drop in availableDrops)
        {
            currentWeight += drop.dropWeight;
            if (randomVal <= currentWeight)
            {
                return drop;
            }
        }

        return availableDrops.Count > 0 ? availableDrops[0] : coinDropSettings[0];
    }

    // --- Risk/Reward Negative Point Mystery Coin System ---
    private int CalculateFinalPointValue(int initialValue, bool isMystery)
    {
        if (isMystery)
        {
            // 20% chance to spawn a coin that subtracts points (-)
            bool isNegative = Random.value < 0.2f; 
            
            if (isNegative)
            {
                int[] negativeValues = { -5, -10, -25, -50 };
                return negativeValues[Random.Range(0, negativeValues.Length)];
            }
            else
            {
                int[] positiveValues = { 10, 25, 50, 100 };
                return positiveValues[Random.Range(0, positiveValues.Length)];
            }
        }
        
        return initialValue;
    }

void SpawnJellyAt(int x, int y, Vector3 spawnPos)
    {
        // 1. Pick a random color theme
        int randomThemeIndex = Random.Range(0, jellyThemes.Length);
        JellyTheme selectedTheme = jellyThemes[randomThemeIndex];

        // 2. Mystery coin check: Spawns only if level >= 5 with a 5% drop chance
        bool isMystery = false;
        if (currentPlayerLevel >= 5)
        {
            isMystery = Random.value < 0.05f; 
        }

        // 3. Instantiate coin object
        Transform parentTransform = gridParent != null ? gridParent : transform;
        GameObject coinObj = Instantiate(coinPrefab, spawnPos, Quaternion.identity, parentTransform);
        coinObj.name = $"Coin ({x},{y})";

        Coin coinScript = coinObj.GetComponent<Coin>();
        if (coinScript != null)
        {
            coinScript.x = x;
            coinScript.y = y;

            // Pass the entire progression sprites array so InputManager can update faces during combo
            coinScript.progressionSprites = selectedTheme.progressionSprites;

            // Standard coins ALWAYS spawn with Element 0 (Base Face)
            // Mystery coins spawn with Element 6 (Mystery Face)
            int initialSpriteIndex = isMystery ? 6 : 0;
            
            Sprite selectedSprite = null;
            if (initialSpriteIndex < selectedTheme.progressionSprites.Length)
            {
                selectedSprite = selectedTheme.progressionSprites[initialSpriteIndex];
            }

            int basePoints = isMystery ? 0 : 5; // Base coins start with 5 points
            int finalValue = CalculateFinalPointValue(basePoints, isMystery);

            coinScript.SetupCoin(selectedSprite, randomThemeIndex, finalValue, isMystery);
            board[x, y] = coinScript;
        }
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

                    // 1. Pick a random color theme
                    int randomThemeIndex = Random.Range(0, jellyThemes.Length);
                    JellyTheme selectedTheme = jellyThemes[randomThemeIndex];

                    // 2. Mystery coin check
                    bool isMystery = false;
                    if (currentPlayerLevel >= 5)
                    {
                        isMystery = Random.value < 0.05f;
                    }
                    
                    GameObject coinObj = Instantiate(coinPrefab, spawnAbovePos, Quaternion.identity, parentTransform);
                    coinObj.name = $"Coin ({x},{y})";

                    Coin coinScript = coinObj.GetComponent<Coin>();
                    if (coinScript != null)
                    {
                        coinScript.x = x;
                        coinScript.y = y;

                        // Assign progression sprites array
                        coinScript.progressionSprites = selectedTheme.progressionSprites;

                        // Always start with Element 0 (or Element 6 for Mystery)
                        int initialSpriteIndex = isMystery ? 6 : 0;

                        Sprite selectedSprite = null;
                        if (initialSpriteIndex < selectedTheme.progressionSprites.Length)
                        {
                            selectedSprite = selectedTheme.progressionSprites[initialSpriteIndex];
                        }

                        int basePoints = isMystery ? 0 : 5;
                        int finalValue = CalculateFinalPointValue(basePoints, isMystery);

                        coinScript.SetupCoin(selectedSprite, randomThemeIndex, finalValue, isMystery);
                        board[x, y] = coinScript;

                        coinScript.DropToTargetPosition(targetPos, 0.4f);
                    }
                }
            }
        }
        yield return new WaitForSeconds(0.4f);
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

        // --- Algorithmic Point Increase Calculation ---
        int totalMatchValue = 0;
        int matchCount = matchedCoins.Count;

        float dynamicMultiplier = 1.0f;
        if (matchCount >= 5)
        {
            dynamicMultiplier += (matchCount - 4) * comboMultiplierStep; 
        }

        bool hasMystery = false;
        int colorIndex = matchedCoins[0].colorIndex; 
        int maxVariantMatched = matchCount >= 20 ? 5 : 
                                matchCount >= 15 ? 4 : 
                                matchCount >= 10 ? 3 : 
                                matchCount >= 7 ? 2 : 
                                matchCount >= 5 ? 1 : 0;

        foreach (Coin coin in matchedCoins) 
        {
            int baseValue = coin.pointValue;
            
            if (!coin.isMysteryCoin && baseValue > 0)
            {
                baseValue = Mathf.RoundToInt(baseValue * dynamicMultiplier);
            }
            
            totalMatchValue += baseValue;
        }

        if (ScoreManager.Instance != null) 
        {
            ScoreManager.Instance.AddScore(totalMatchValue, matchCount);
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ProcessExtraTask(matchCount, colorIndex, hasMystery, maxVariantMatched);
        }

        Vector3 burstPoint = matchedCoins[matchedCoins.Count - 1].transform.position;
        SpriteRenderer sr = matchedCoins[0].GetComponent<SpriteRenderer>();
        Color burstColor = sr != null ? sr.color : Color.white;

        if (matchedCoins.Count >= 5)
        {
            float shakeIntensity = 0.1f + (matchedCoins.Count * 0.02f);
            
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.TriggerShake(0.3f, shakeIntensity, 25);
            }
        }

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

                if (coin.isMysteryCoin)
                {
                    Vector3 revealTarget = Vector3.Lerp(coin.transform.position, new Vector3(0f, 1.5f, -2f), 0.35f);
                    StartCoroutine(coin.RevealMysteryCoinRoutine(revealTarget, i * 0.08f));
                }
                else
                {
                    StartCoroutine(FlyJellyToUI(coin.gameObject, i * 0.08f));
                }
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
                            movingCoins.DropToTargetPosition(targetPos, 0.3f);

                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(0.3f);
    }

   private void SpawnCenterJokerIfNeeded()
    {
        if (LevelManager.Instance == null || currentLevel == null) return;

        if (activeJokerInstance != null) Destroy(activeJokerInstance);

        if (currentLevel.taskType == ExtraTaskType.MysteryAndClock && tickTockJokerPrefab != null)
        {
            int centerX = width / 2;
            int centerY = height / 2;

            if (!activeCells[centerX, centerY])
            {
                Vector2 jokerPos = GetWorldPosition(centerX, centerY);
                Vector3 finalSpawnPos = new Vector3(jokerPos.x, jokerPos.y, -1f);
                
                activeJokerInstance = Instantiate(tickTockJokerPrefab, finalSpawnPos, Quaternion.identity, gridParent);
                
                activeJokerInstance.transform.localScale = new Vector3(0.15f, 0.15f, 1f);
            }
        }
    }
}