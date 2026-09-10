using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PowerUpManager : MonoBehaviour
{

    [Header("Power-Up Textures (Görseller)")]
    public Sprite lineSprite;      
    public Sprite bombSprite;       
    public Sprite timeBenderSprite; 
    public Sprite rainbowSprite;
    public static PowerUpManager Instance;

    [Header("VFX Prefabs")]
    public GameObject bombVFXPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void CheckAndSpawnPowerUp(int matchCount, int x, int y, Vector3 spawnPosition, int colorIndex)
    {
        PowerUpType typeToSpawn = PowerUpType.None;

        if (matchCount >= 15) typeToSpawn = PowerUpType.Rainbow;
        else if (matchCount >= 10) typeToSpawn = PowerUpType.TimeBender;
        else if (matchCount >= 7) typeToSpawn = PowerUpType.Bomb;
        else if (matchCount >= 5) typeToSpawn = PowerUpType.Line;

        if (typeToSpawn == PowerUpType.None) return;

        SpawnPowerUpCoin(typeToSpawn, x, y, spawnPosition, colorIndex);
    }

  private void SpawnPowerUpCoin(PowerUpType type, int x, int y, Vector3 position, int themeIndex)
    {
        BoardManager boardMgr = BoardManager.Instance;
        if (boardMgr == null || x < 0 || x >= boardMgr.width || y < 0 || y >= boardMgr.height) return;

        if (boardMgr.board[x, y] != null) 
        {
            Destroy(boardMgr.board[x, y].gameObject);
            boardMgr.board[x, y] = null;
        }

        Transform parentTransform = boardMgr.gridParent != null ? boardMgr.gridParent : boardMgr.transform;
        GameObject coinObj = Instantiate(boardMgr.coinPrefab, position, Quaternion.identity, parentTransform);
        coinObj.name = $"PowerUp_{type} ({x},{y})";

        Coin coinScript = coinObj.GetComponent<Coin>();
        if (coinScript != null)
        {
            coinScript.x = x;
            coinScript.y = y;
            
            Sprite powerUpTexture = null;
            switch (type)
            {
                case PowerUpType.Line: powerUpTexture = lineSprite; break;
                case PowerUpType.Bomb: powerUpTexture = bombSprite; break;
                case PowerUpType.TimeBender: powerUpTexture = timeBenderSprite; break;
                case PowerUpType.Rainbow: powerUpTexture = rainbowSprite; break;
            }

            JellyTheme selectedTheme = boardMgr.jellyThemes[themeIndex];
            if (powerUpTexture == null) powerUpTexture = selectedTheme.progressionSprites[0];
            
            coinScript.progressionSprites = selectedTheme.progressionSprites;
            
            int basePoints = 25; 
            
            coinScript.SetupCoin(powerUpTexture, themeIndex, basePoints, false);
            
            coinScript.currentPowerUp = type; 
            
            boardMgr.board[x, y] = coinScript;

            Vector3 originalScale = coinScript.transform.localScale; 
            
            coinScript.transform.localScale = Vector3.zero;
            coinScript.transform.DOScale(originalScale, 0.4f).SetEase(Ease.OutBack);
        }
    }

    public List<Coin> CalculatePowerUpEffects(List<Coin> initialMatches)
    {
        BoardManager boardMgr = BoardManager.Instance;
        if (boardMgr == null) return initialMatches;

        HashSet<Coin> coinsToDestroy = new HashSet<Coin>(initialMatches);
        Queue<Coin> powerUpsToProcess = new Queue<Coin>();

        foreach (Coin c in initialMatches)
        {
            if (c != null && c.currentPowerUp != PowerUpType.None) powerUpsToProcess.Enqueue(c);
        }

        while (powerUpsToProcess.Count > 0)
        {
            Coin pCoin = powerUpsToProcess.Dequeue();
            List<Coin> affectedCoins = new List<Coin>();

            if (pCoin.currentPowerUp == PowerUpType.Line)
            {
                for (int i = 0; i < boardMgr.width; i++)
                    if (boardMgr.board[i, pCoin.y] != null) affectedCoins.Add(boardMgr.board[i, pCoin.y]);
                    
                for (int j = 0; j < boardMgr.height; j++)
                    if (boardMgr.board[pCoin.x, j] != null) affectedCoins.Add(boardMgr.board[pCoin.x, j]);
            }
            else if (pCoin.currentPowerUp == PowerUpType.Bomb)
            {
                for (int i = pCoin.x - 1; i <= pCoin.x + 1; i++)
                {
                    for (int j = pCoin.y - 1; j <= pCoin.y + 1; j++)
                    {
                        if (i >= 0 && i < boardMgr.width && j >= 0 && j < boardMgr.height)
                        {
                            if (boardMgr.board[i, j] != null) affectedCoins.Add(boardMgr.board[i, j]);
                        }
                    }
                }
            }
            else if (pCoin.currentPowerUp == PowerUpType.TimeBender)
            {
                Debug.Log("<color=magenta>ZAMAN BÜKÜLDÜ!</color> Zaman 5 saniyeliğine donduruluyor.");
                if (ComboTimerManager.Instance != null)
                {
                    ComboTimerManager.Instance.FreezeTime(5f); 
                }
            }
            else if (pCoin.currentPowerUp == PowerUpType.Rainbow)
            {
               int mostFrequentColor = FindMostFrequentColor(boardMgr.board, boardMgr.width, boardMgr.height);
                
                for (int i = 0; i < boardMgr.width; i++)
                {
                    for (int j = 0; j < boardMgr.height; j++)
                    {
                        Coin targetCoin = boardMgr.board[i, j];

                        if (targetCoin != null && !targetCoin.isMysteryCoin && !targetCoin.isPowerUp && targetCoin.colorIndex == mostFrequentColor)
                        {
                            affectedCoins.Add(targetCoin);

                            targetCoin.transform.DOPunchScale(new Vector3(0.4f, 0.4f, 0f), 0.35f, 10, 1f);

                            SpriteRenderer sr = targetCoin.GetComponent<SpriteRenderer>();
                            if (sr != null)
                            {
                                sr.DOColor(Color.white, 0.15f).SetLoops(2, LoopType.Yoyo);
                            }

                            if (ParticleFXManager.Instance != null)
                            {
                                ParticleFXManager.Instance.PlayBurst(targetCoin.transform.position, Color.white, 1);
                            }
                        }
                    }
                }
                
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.jellyPopSound, 0.6f, 1.2f);
                }

                Debug.Log($"<color=cyan>GÖKKUŞAĞI AKTİF!</color> {affectedCoins.Count} adet coin süpürülüyor!");
            }

            foreach (Coin affected in affectedCoins)
            {
                if (!coinsToDestroy.Contains(affected))
                {
                    coinsToDestroy.Add(affected);
                    if (affected.isPowerUp) powerUpsToProcess.Enqueue(affected);
                }
            }
        }

        return new List<Coin>(coinsToDestroy);
    }

    private int FindMostFrequentColor(Coin[,] boardArray, int width, int height)
    {
        Dictionary<int, int> colorCounts = new Dictionary<int, int>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (boardArray[i, j] != null && !boardArray[i, j].isMysteryCoin && !boardArray[i, j].isPowerUp)
                {
                    int color = boardArray[i, j].colorIndex;
                    if (colorCounts.ContainsKey(color)) colorCounts[color]++;
                    else colorCounts[color] = 1;
                }
            }
        }

        int bestColor = 0;
        int maxCount = 0;
        foreach (var kvp in colorCounts)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                bestColor = kvp.Key;
            }
        }
        return bestColor;
    }

    public void PlayExplosionFX(GameObject expObj, PowerUpType type, Vector3 spawnPos)
    {
        Sequence popSeq = DOTween.Sequence();
        float dist = Vector2.Distance(spawnPos, expObj.transform.position);
        float delay = dist * 0.025f;

        switch (type)
        {
            case PowerUpType.Line:
                bool isHorizontal = Mathf.Abs(expObj.transform.position.y - spawnPos.y) < 0.1f;
                
                popSeq.AppendInterval(delay);
                popSeq.Append(expObj.transform.DOScale(new Vector3(0.6f, 0.6f, 1f), 0.08f).SetEase(Ease.InQuad));

                SpriteRenderer sr = expObj.GetComponent<SpriteRenderer>();
                if (sr != null) popSeq.Insert(delay + 0.05f, sr.DOColor(Color.white, 0.05f));

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.linePowerUpSound, 1.0f, 1.0f);
                }

                if (isHorizontal)
                {
                    float pushDirX = expObj.transform.position.x > spawnPos.x ? 1.5f : -1.5f;
                    popSeq.Append(expObj.transform.DOScale(new Vector3(4.0f, 0.1f, 1f), 0.15f).SetEase(Ease.OutExpo));
                    popSeq.Join(expObj.transform.DOMoveX(expObj.transform.position.x + pushDirX, 0.15f).SetEase(Ease.OutExpo));
                }
                else
                {
                    float pushDirY = expObj.transform.position.y > spawnPos.y ? 1.5f : -1.5f;
                    popSeq.Append(expObj.transform.DOScale(new Vector3(0.1f, 4.0f, 1f), 0.15f).SetEase(Ease.OutExpo));
                    popSeq.Join(expObj.transform.DOMoveY(expObj.transform.position.y + pushDirY, 0.15f).SetEase(Ease.OutExpo));
                }
                popSeq.Append(expObj.transform.DOScale(0f, 0.1f).SetEase(Ease.InExpo));
                break;

            case PowerUpType.Bomb:
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.bombPowerUpSound, 1.0f, 1.0f);
                }
                if (bombVFXPrefab != null)
                {
                    GameObject vfx = Instantiate(bombVFXPrefab, spawnPos, Quaternion.identity);
                    Destroy(vfx, 1f); 
                }
                Vector3 dir = (expObj.transform.position - spawnPos).normalized;
                if (dir == Vector3.zero) dir = Vector3.up;

                popSeq.AppendInterval(delay);

                SpriteRenderer srBomb = expObj.GetComponent<SpriteRenderer>();
                if (srBomb != null) 
                {
                    popSeq.Insert(delay, srBomb.DOColor(new Color(1f, 0.85f, 0.1f), 0.12f)); 
                }
                popSeq.Append(expObj.transform.DOScale(new Vector3(1.3f, 1.3f, 1f), 0.12f).SetEase(Ease.OutQuad));

                popSeq.Append(expObj.transform.DOMove(expObj.transform.position + (dir * 1.2f), 0.25f).SetEase(Ease.OutQuint));
                popSeq.Join(expObj.transform.DORotate(new Vector3(0, 0, Random.Range(-200f, 200f)), 0.25f, RotateMode.Fast)); // Havada takla
                popSeq.Join(expObj.transform.DOScale(0f, 0.2f).SetEase(Ease.InSine));
                break;

            case PowerUpType.Rainbow:
                popSeq.Append(expObj.transform.DOMove(spawnPos, 0.25f).SetEase(Ease.InCubic));
                popSeq.Join(expObj.transform.DORotate(new Vector3(0, 0, 360f), 0.25f, RotateMode.FastBeyond360));
                popSeq.Join(expObj.transform.DOScale(0f, 0.25f).SetEase(Ease.InBack));
                break;

            default:
                popSeq.Append(expObj.transform.DOScale(1.3f, 0.08f).SetEase(Ease.OutQuad));
                popSeq.Append(expObj.transform.DOScale(0f, 0.12f).SetEase(Ease.InBack));
                break;
        }

        popSeq.OnComplete(() => Destroy(expObj));

        if (ParticleFXManager.Instance != null)
        {
            float partDelay = (type == PowerUpType.Line) ? dist * 0.05f : 0.1f;
            DOVirtual.DelayedCall(partDelay, () => {
                ParticleFXManager.Instance.PlayBurst(expObj.transform.position, expObj.GetComponent<SpriteRenderer>().color, 1);
            });
        }
    }
}