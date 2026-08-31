using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardManager : MonoBehaviour
{
    [Header("Juice & UI Effects")]
    [Tooltip("Sahnede Combo Bar'ın veya sayının olduğu UI Objesi")]
    public Transform comboBarTarget;

    public void TriggerGlobalImmortalShake()
    {
        Coin[] allJellies = Object.FindObjectsByType<Coin>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        
        foreach (Coin coin in allJellies)
        {
            if (coin != null && coin.gameObject != null)
            {
                StartCoroutine(coin.ShakeJelly());
            }
        }
    }

    private IEnumerator FlyJellyToUI(GameObject jellyObj, float delay)
    {
        if (jellyObj == null) yield break;

        Coin jellyComponent = jellyObj.GetComponent<Coin>();
        if (jellyComponent != null) jellyComponent.enabled = false;

        yield return new WaitForSeconds(delay); 
        
        if (jellyObj == null) yield break;

        SpriteRenderer sr = jellyObj.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 50;

        Vector3 startPos = jellyObj.transform.position;
        Vector3 endPos = comboBarTarget != null ? comboBarTarget.position : startPos;

        float arcHeight = Random.Range(1.5f, 3f);
        int direction = Random.Range(0, 2) == 0 ? -1 : 1; 
        Vector3 controlPoint = startPos + (endPos - startPos) / 2f + new Vector3(arcHeight * direction, arcHeight, 0f);

        float duration = 0.45f;
        float elapsed = 0f;
        Vector3 startScale = jellyObj.transform.localScale;

        StartCoroutine(PulseComboTargetsRoutine(duration));

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            Vector3 m1 = Vector3.Lerp(startPos, controlPoint, t);
            Vector3 m2 = Vector3.Lerp(controlPoint, endPos, t);
            jellyObj.transform.position = Vector3.Lerp(m1, m2, t);
            jellyObj.transform.localScale = Vector3.Lerp(startScale, startScale * 0.25f, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Color jellyColor = sr != null ? sr.color : Color.white;

        if (ComboBarImpactFX.Instance != null)
        {
            ComboBarImpactFX.Instance.PlayImpact(jellyColor);
        }

        Destroy(jellyObj); 
        TriggerComboBarImpact();
    }

    private IEnumerator PulseComboTargetsRoutine(float duration)
    {
        List<Transform> targets = new List<Transform>();

        if (comboBarTarget != null)
        {
            targets.Add(comboBarTarget);
        }

        if (ScoreManager.Instance != null)
        {
            if (ScoreManager.Instance.scoreText != null)
            {
                targets.Add(ScoreManager.Instance.scoreText.transform);
            }

            if (ScoreManager.Instance.comboText != null)
            {
                targets.Add(ScoreManager.Instance.comboText.transform);
            }

            if (ScoreManager.Instance.comboTitleText != null && !string.IsNullOrEmpty(ScoreManager.Instance.comboTitleText.text))
            {
                targets.Add(ScoreManager.Instance.comboTitleText.transform);
            }
        }

        if (targets.Count == 0)
        {
            yield break;
        }

        Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();
        foreach (Transform target in targets)
        {
            if (target != null)
            {
                originalScales[target] = target.localScale;
            }
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            foreach (Transform target in targets)
            {
                if (target == null) continue;

                if (target == comboBarTarget)
                {
                    float wobble = Mathf.Sin(elapsed * 16f) * 1.5f;
                    target.localRotation = Quaternion.Euler(0f, 0f, wobble);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (Transform target in targets)
        {
            if (target == null) continue;
            if (originalScales.ContainsKey(target))
            {
                target.localScale = originalScales[target];
            }

            if (target == comboBarTarget)
            {
                target.localRotation = Quaternion.identity;
            }
        }
    }

    private void TriggerComboBarImpact()
    {
        if (comboBarTarget != null)
        {
            StartCoroutine(PunchScaleUI(comboBarTarget));
        }
    }

    private IEnumerator PunchScaleUI(Transform uiTransform)
    {
        Vector3 originalScale = Vector3.one; 
        Vector3 punchScale = originalScale;

        float duration = 0.1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            uiTransform.localScale = Vector3.Lerp(originalScale, punchScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        
        while (elapsed < duration)
        {
            uiTransform.localScale = Vector3.Lerp(punchScale, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        uiTransform.localScale = originalScale;
    }

    public bool HasAnyValidMatch()
    {
        List<Coin> bestGroup = FindLargestConnectedGroup();
        return bestGroup != null && bestGroup.Count >= 4;
    }

    public void CheckForPossibleMoves()
    {
        if (!HasAnyValidMatch())
        {
            Debug.Log("Oynanabilir hamle kalmadı! Grid karıştırılıyor...");
            StartCoroutine(ShuffleBoardRoutine());
        }
    }

    private IEnumerator ShuffleBoardRoutine()
    {
        IsProcessing = true;
        ClearHintEffects();

        // 1. WARNING ANIMATION: Global panic shake
        TriggerGlobalImmortalShake();
        yield return new WaitForSeconds(0.4f);

        List<Coin> allJellies = new List<Coin>();
        
        Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (activeCells != null && !activeCells[x, y]) continue;
                if (board[x, y] != null && board[x, y].gameObject != null)
                {
                    Coin coin = board[x, y];
                    allJellies.Add(coin);
                    
                    // Jölenin gerçek boyutunu hafızaya alıyoruz
                    originalScales[coin.transform] = coin.transform.localScale;
                }
                else
                {
                    board[x, y] = null;
                }
            }
        }

        Vector3 centerWorldPos = GetWorldPosition((width - 1) / 2f, (height - 1) / 2f);

        // 2. BLACK HOLE EFFECT
        foreach (Coin coin in allJellies)
        {
            if (coin != null && coin.gameObject != null)
            {
                float randomDelay = Random.Range(0f, 0.15f); 
                StartCoroutine(SuckIntoCenterRoutine(coin.transform, centerWorldPos, randomDelay));
            }
        }

        yield return new WaitForSeconds(0.45f);

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.TriggerShake(0.2f, 0.15f);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.ShuffleExplosion, 1.0f, 1.0f);
        }

        // 3. MATHEMATICAL SHUFFLE
        bool validBoardFound = false;
        int attempts = 0;

        while (!validBoardFound && attempts < 50)
        {
            attempts++;

            for (int i = 0; i < allJellies.Count; i++)
            {
                Coin temp = allJellies[i];
                int randomIndex = Random.Range(i, allJellies.Count);
                allJellies[i] = allJellies[randomIndex];
                allJellies[randomIndex] = temp;
            }

            int index = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (activeCells != null && !activeCells[x, y]) continue;
                    if (index < allJellies.Count)
                    {
                        Coin j = allJellies[index];
                        if (j != null && j.gameObject != null)
                        {
                            board[x, y] = j;
                            j.x = x;
                            j.y = y;
                            index++;
                        }
                    }
                }
            }

            List<Coin> testGroup = FindLargestConnectedGroup();
            if (testGroup != null && testGroup.Count >= 4)
            {
                validBoardFound = true;
            }

            yield return null; 
        }

        // 4. EXPLOSION & BOUNCE EFFECT
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Coin coin = board[x, y];
                if (activeCells != null && !activeCells[x, y]) continue;
                if (coin != null && coin.gameObject != null)
                {
                    coin.transform.name = $"Coin ({x},{y})";
                    Vector3 targetPos = new Vector3(GetWorldPosition(x, y).x, GetWorldPosition(x, y).y, -1f);
                    
                    float outDelay = Random.Range(0f, 0.1f);
                    
                    Vector3 correctScale = originalScales.ContainsKey(coin.transform) ? originalScales[coin.transform] : Vector3.one;
                    
                    StartCoroutine(PopOutFromCenterRoutine(coin.transform, centerWorldPos, targetPos, outDelay, correctScale));
                }
            }
        }

        yield return new WaitForSeconds(0.5f);

        IsProcessing = false;
        ResetHintTimer();
    }

    // --- NEW JUICY SHUFFLE ANIMATIONS ---
    private IEnumerator SuckIntoCenterRoutine(Transform coinTransform, Vector3 center, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (coinTransform == null) yield break;

        Vector3 startPos = coinTransform.position;
        Vector3 startScale = coinTransform.localScale;
        
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (coinTransform == null) yield break;

            float t = elapsed / duration;
            // Ease In Cubic (Starts slow, speeds up into the center)
            float easeT = t * t * t; 

            // Add a spinning effect as they get sucked in
            coinTransform.Rotate(0f, 0f, 15f);

            coinTransform.position = Vector3.Lerp(startPos, center, easeT);
            coinTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, easeT);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (coinTransform != null)
        {
            coinTransform.position = center;
            coinTransform.localScale = Vector3.zero;
            coinTransform.rotation = Quaternion.identity; // Reset rotation
        }
    }

   private IEnumerator PopOutFromCenterRoutine(Transform coinTransform, Vector3 center, Vector3 targetPos, float delay, Vector3 targetScale)
    {
        yield return new WaitForSeconds(delay);
        if (coinTransform == null) yield break;

        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (coinTransform == null) yield break;

            float t = elapsed / duration;
            
            float overshoot = 1.70158f;
            float p = t - 1f;
            float easeT = p * p * ((overshoot + 1f) * p + overshoot) + 1f;

            coinTransform.position = Vector3.LerpUnclamped(center, targetPos, easeT);
            
            coinTransform.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (coinTransform != null)
        {
            coinTransform.position = targetPos;
            coinTransform.localScale = targetScale; 
        }
    }
}