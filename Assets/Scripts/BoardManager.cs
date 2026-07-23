using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct JellyTheme
{
    public string themeName; // Inspector'da düzenli durması için (Örn: "Turuncu", "Mavi")
    [Tooltip("0: Default, 1: Combo 1, 2: Combo 2...")]
    public Sprite[] progressionSprites; 
}

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    public int width = 6;
    public int height = 8;
    public float spacing = 1.2f;

    [Header("References")]
    public GameObject jellyPrefab;

    [Header("Jelly Evolutions")]
    public JellyTheme[] jellyThemes; // Eski jellySprites yerine artık bu evrim dizisini kullanıyoruz

    [Header("Grid Visuals")]
    public GameObject cellPrefab;  // Background cell prefab assigned from Inspector
    public GameObject boardBackgroundPrefab;  // Background prefab assigned from Inspector
    public Transform gridParent;   // Parent object to keep the Hierarchy clean

    [Header("Jelly Theme Palette")]
    // Two soft pastel colors for the checkerboard background effect
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
    }

    void GenerateBoard()
    {
        CreateBoardBackground();    
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = GetWorldPosition(x, y);

                // 1. Create Background Grid Cell
                if (cellPrefab != null)
                {
                    Vector3 bgPos = new Vector3(pos.x, pos.y, 0.1f);
                    GameObject cell = Instantiate(cellPrefab, bgPos, Quaternion.identity, gridParent);
                    cell.name = $"Cell_{x}_{y}";

                    cell.transform.localScale = new Vector3(1.15f, 1.15f, 1f);

                    // Checkerboard Styling & Layer Sorting
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

        // We are creating the panel (at the very back, with Z = 0.5f)
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

    // Converts grid coordinates to world space position
    public Vector3 GetWorldPosition(float x, float y)
    {
        float localX = -((width - 1) * spacing) / 2f + (x * spacing);
        float localY = -((height - 1) * spacing) / 2f + (y * spacing);

        // We add GridParent's world position to it
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
            // YENİ EVRİM SİSTEMİ ÇAĞRISI
            jellyScript.SetupEvolution(jellyThemes[randomIndex].progressionSprites, randomIndex);
            board[x, y] = jellyScript;
        }
    }

    // Match/explosion trigger called from InputManager
    public void ProcessMatches(List<Jelly> matchedJellies)
    {
       if (matchedJellies == null || matchedJellies.Count == 0) return;

        Vector3 burstPoint = matchedJellies[matchedJellies.Count - 1].transform.position;
        Color burstColor = matchedJellies[0].GetComponent<SpriteRenderer>().color;

        // Play burst particle effect and camera shake if ParticleFXManager is available
        if (ParticleFXManager.Instance != null)
        {
            ParticleFXManager.Instance.PlayBurst(burstPoint, burstColor, matchedJellies.Count);
        }
        StartCoroutine(ProcessMatchesRoutine(matchedJellies));
    }

    private IEnumerator ProcessMatchesRoutine(List<Jelly> matchedJellies)
    {
        IsProcessing = true;

        // Add score if ScoreManager instance is active
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(matchedJellies.Count);
        }

        // 1. Clear matched jellies
        foreach (var jelly in matchedJellies)
        {
            if (jelly != null)
            {
                board[jelly.x, jelly.y] = null;
                Destroy(jelly.gameObject);
            }
        }

        yield return new WaitForSeconds(0.15f);

        // 2. Drop existing jellies down
        yield return StartCoroutine(CollapseGridRoutine());

        // 3. Refill the empty spaces
        yield return StartCoroutine(RefillGridRoutine());

        IsProcessing = false;
    }

    private IEnumerator CollapseGridRoutine()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // If the current cell is empty, search for the first occupied cell above it
                if (board[x, y] == null)
                {
                    for (int aboveY = y + 1; aboveY < height; aboveY++)
                    {
                        if (board[x, aboveY] != null)
                        {
                            // Shift the jelly down
                            Jelly movingJelly = board[x, aboveY];
                            board[x, y] = movingJelly;
                            board[x, aboveY] = null;

                            movingJelly.x = x;
                            movingJelly.y = y;
                            movingJelly.transform.name = $"Jelly ({x},{y})";

                            // Smooth downward movement maintaining Z-depth
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
                    // Spawn above the board boundary
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

                        // Animate falling into target position
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

    // --- IMMORTAL ASMR SHOCKWAVE ---
    public void TriggerGlobalImmortalShake()
    {
        Jelly[] allJellies = Object.FindObjectsByType<Jelly>(FindObjectsSortMode.None);

        foreach (Jelly jelly in allJellies)
        {
            if (jelly != null)
            {
                StartCoroutine(jelly.ShakeJelly(0.25f, 0.08f)); 
            }
        }
    }
}