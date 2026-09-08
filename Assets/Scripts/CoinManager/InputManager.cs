using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; 
using DG.Tweening;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    [Header("References")]
    public BoardManager boardManager;
    public LineRenderer lineRenderer;
    public GameObject dragSparklePrefab;
    private bool hasGameStarted = false;
    public static event System.Action OnPlayerMoveExecuted;

    [Header("Color Palette")]
    public Color[] jellyColors = new Color[]
    {
        new Color(0.2f, 0.6f, 1.0f, 1.0f), 
        new Color(0.2f, 0.8f, 0.2f, 1.0f), 
        new Color(1.0f, 0.5f, 0.0f, 1.0f), 
        new Color(0.6f, 0.2f, 0.8f, 1.0f), 
        new Color(1.0f, 0.0f, 0.0f, 1.0f)
    };

    [Header("Line Visuals")]
    public float lineWidth = 0.2f;

    [Header("Idle / Hover Settings")]
    public float hoverTimeThreshold = 5.0f; 

    private List<Coin> selectedJellies = new List<Coin>();
    private bool isDragging = false;

    private Coin hoveredCoin = null;
    private float hoverTimer = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.sortingOrder = 100;
        }
    }

    void Update()
    {
        if (IsGameplayLocked())
        {
            if (isDragging || (lineRenderer != null && lineRenderer.positionCount > 0) || selectedJellies.Count > 0)
            {
                ForceClearSelection();
            }
            return;
        }

        HandleInput();
        HandleHoverDetection();
    }

    // --- SYNCHRONIZED IDLE WIGGLE DETECTION ---
    void HandleHoverDetection()
    {
        if (IsGameplayLocked()) return;

        Coin currentCoin = GetCoinUnderMouse();

        if (currentCoin != null)
        {
            if (currentCoin == hoveredCoin)
            {
                hoverTimer += Time.deltaTime;
                if (hoverTimer >= hoverTimeThreshold)
                {
                    if (selectedJellies.Count > 0)
                    {
                        foreach (var coin in selectedJellies)
                        {
                            if (coin != null) coin.StartIdleWiggle();
                        }
                    }
                    else
                    {
                        currentCoin.StartIdleWiggle();
                    }

                    hoverTimer = 0f; 
                }
            }
            else
            {
                ResetHoverTimer();
                hoveredCoin = currentCoin;
            }
        }
        else
        {
            ResetHoverTimer();
        }
    }

    void ResetHoverTimer()
    {
        if (selectedJellies.Count > 0)
        {
            foreach (var coin in selectedJellies)
            {
                if (coin != null) coin.StopIdleWiggle();
            }
        }
        else if (hoveredCoin != null)
        {
            hoveredCoin.StopIdleWiggle();
        }

        hoveredCoin = null;
        hoverTimer = 0f;
    }

    void HandleInput()
    {
        if (IsGameplayLocked()) return;

        if (Input.GetMouseButtonDown(0))
        {
            // Ignore UI clicks ONLY when first pressing down, unless dragging
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return; 

            Coin startCoin = GetCoinUnderMouse();
            if (startCoin != null)
            {
                isDragging = true;
                SelectCoin(startCoin);
                SetLineColor(startCoin.colorIndex);
            }
        }
   else if (Input.GetMouseButton(0) && isDragging)
        {
            Coin currentCoin = GetCoinUnderMouse();
            
            if (currentCoin != null && !selectedJellies.Contains(currentCoin))
            {
                Coin lastCoin = selectedJellies[selectedJellies.Count - 1];

                // 1. Normal Kural: Renkler aynı mı?
                bool isColorMatch = (currentCoin.colorIndex == lastCoin.colorIndex);
                
                // 2. GDD Kuralı: İkisi de Güç Jetonu mu? (Farklı renk olsalar bile çarpışabilirler!)
                bool isPowerUpCombo = (lastCoin.currentPowerUp != PowerUpType.None && currentCoin.currentPowerUp != PowerUpType.None);

                if ((isColorMatch || isPowerUpCombo) && IsNeighbor(lastCoin, currentCoin))
                {
                    // Sadece 2 jeton çarpıştırılabilir, oyuncunun 3.-4. jetonu seçmesini (zinciri uzatmasını) engelliyoruz.
                    if (isPowerUpCombo && selectedJellies.Count >= 2) return; 

                    SelectCoin(currentCoin);
                }
            }

            if (selectedJellies.Count > 0)
            {
                UpdateFullLinePositions();
            }
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            ResetHoverTimer();

            bool isPowerUpCombo = selectedJellies.Count == 2 && 
                                  selectedJellies[0].currentPowerUp != PowerUpType.None && 
                                  selectedJellies[1].currentPowerUp != PowerUpType.None;

            if (selectedJellies.Count >= 4 || 
               (selectedJellies.Count == 1 && selectedJellies[0].currentPowerUp != PowerUpType.None) ||
               isPowerUpCombo) 
            {
                StartCoroutine(ProcessMergeSequence(new List<Coin>(selectedJellies)));
            }
            else
            {
                ClearSelection();
            }
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            ResetHoverTimer();

            if (selectedJellies.Count >= 4) 
            {
                StartCoroutine(ProcessMergeSequence(new List<Coin>(selectedJellies)));
            }
            else if (selectedJellies.Count == 1 && selectedJellies[0].currentPowerUp != PowerUpType.None)
            {
                StartCoroutine(ProcessMergeSequence(new List<Coin>(selectedJellies)));
            }
            else
            {
                ClearSelection();
            }
        }
    }

    private void SelectCoin(Coin coin)
    {
        if (IsGameplayLocked()) return;

        if (!selectedJellies.Contains(coin))
        {
            selectedJellies.Add(coin);

            Vector3 originalScale = coin.transform.localScale;
            coin.transform.DOKill();
            coin.transform.DOScale(originalScale * 1.25f, 0.1f).OnComplete(() => 
            {
                coin.transform.DOScale(originalScale, 0.15f).SetEase(Ease.OutBounce);
            });

            UpdateSelectedCoinFaces();

            if (!hasGameStarted)
            {
                hasGameStarted = true;
                if (ComboTimerManager.Instance != null) ComboTimerManager.Instance.StartOrResetTimer();
            }
            
            StartCoroutine(coin.ShakeJelly(0.08f, 0.03f));
            coin.EnableSelectionGlow();

            if (AudioManager.Instance != null)
            {
                float pitchStep = 0.08f; 
                float currentPitch = Mathf.Clamp(0.85f + (selectedJellies.Count * pitchStep), 0.85f, 2.0f);
                AudioManager.Instance.PlaySFX(AudioManager.Instance.jellyPopSound, currentPitch, 0.7f);
            }

            if (dragSparklePrefab != null)
            {
                Instantiate(dragSparklePrefab, coin.transform.position, Quaternion.identity, coin.transform);
            }

            if (ComboUIManager.Instance != null)
            {
                ComboUIManager.Instance.UpdateComboUI(selectedJellies.Count);
            }

            UpdateFullLinePositions();
        }
    }

    private int GetFaceLevelForCombo(int count)
    {
        if (count >= 20) return 5;       
        if (count >= 15) return 4;       
        if (count >= 10) return 3;       
        if (count >= 7)  return 2;       
        if (count >= 5)  return 1;       
        return 0;                        
    }

    private void UpdateSelectedCoinFaces()
    {
        int count = selectedJellies.Count;
        int targetFaceLevel = GetFaceLevelForCombo(count);

        foreach (Coin coin in selectedJellies)
        {
           if (coin != null && !coin.isMysteryCoin && coin.currentPowerUp == PowerUpType.None)
            {
                coin.UpdateFaceByCombo(targetFaceLevel);
            }
        }
    }

    void UpdateFullLinePositions()
    {
        if (lineRenderer == null) return;

        int pointCount = selectedJellies.Count;
        lineRenderer.positionCount = pointCount + 1;

        for (int i = 0; i < pointCount; i++)
        {
            Vector3 pos = selectedJellies[i].transform.position;
            pos.z = -2f;
            lineRenderer.SetPosition(i, pos);
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = -2f;
        lineRenderer.SetPosition(pointCount, mouseWorldPos);
    }

    private IEnumerator ProcessMergeSequence(List<Coin> coinsToMerge)
    {
        if (lineRenderer != null) lineRenderer.positionCount = 0;

        OnPlayerMoveExecuted?.Invoke();

        Vector3 mergeCenter = coinsToMerge[coinsToMerge.Count - 1].transform.position;
        int coinCount = coinsToMerge.Count;

        for (int i = 0; i < coinsToMerge.Count; i++)
        {
            float delay = i * 0.03f; 
            StartCoroutine(coinsToMerge[i].AnimateMergeTo(mergeCenter, delay));
        }

        yield return new WaitForSeconds((coinsToMerge.Count * 0.03f) + 0.18f);

        int simulatedComboMultiplier = 27; 

        if (ComboQualityManager.Instance != null)
        {
            ComboQualityManager.Instance.ProcessComboQuality(coinCount, simulatedComboMultiplier, mergeCenter);
        }

        if (boardManager != null)
        {
            boardManager.ProcessMatches(coinsToMerge);
        }
        
        selectedJellies.Clear();
    }

    void SetLineColor(int colorIndex)
    {
        if (lineRenderer == null) return;
        Color targetColor = Color.white;
        if (colorIndex >= 0 && colorIndex < jellyColors.Length)
        {
            targetColor = jellyColors[colorIndex];
        }

        lineRenderer.startColor = targetColor;
        lineRenderer.endColor = targetColor;
    }

    public void ForceClearSelection()
    {
        isDragging = false;
        ResetHoverTimer();

        foreach (var coin in selectedJellies)
        {
            if (coin != null)
            {
                coin.ResetShape();
                if (!coin.isMysteryCoin && coin.currentPowerUp == PowerUpType.None) 
                {
                    coin.UpdateFaceByCombo(0); 
                }
            }
        }

        selectedJellies.Clear();
        if (lineRenderer != null) lineRenderer.positionCount = 0;
        if (ComboUIManager.Instance != null) ComboUIManager.Instance.UpdateComboUI(0);
    }

    void ClearSelection()
    {
        ForceClearSelection();

        if (AudioManager.Instance != null)
        {
            float randomPitch = Random.Range(0.95f, 1.05f);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.jellyPopSound, randomPitch);
        }
    }

    // Call this whenever a level starts or resets
    public void ResetInputState()
    {
        hasGameStarted = false;
        hoverTimer = 0f;
        hoveredCoin = null;
        ForceClearSelection();
    }

    private bool IsGameplayLocked()
    {
        if (BoardManager.Instance != null && BoardManager.Instance.IsProcessing)
        {
            return true;
        }

        if (LevelManager.Instance != null && LevelManager.Instance.IsIntroPlaying)
        {
            return true;
        }

        if (ComboTimerManager.Instance != null && ComboTimerManager.Instance.IsLevelFinished)
        {
            return true;
        }

        return false;
    }

    Coin GetCoinUnderMouse()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return null;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCam.transform.position.z);
        Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);

        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, 0.35f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Jelly")) return hit.GetComponent<Coin>();
        }
        return null;
    }

    bool IsNeighbor(Coin a, Coin b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy == 1);
    }
}