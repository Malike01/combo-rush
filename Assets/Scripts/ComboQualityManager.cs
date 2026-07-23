using System.Collections;
using UnityEngine;
using TMPro;

public class ComboQualityManager : MonoBehaviour
{
    public static ComboQualityManager Instance;

    [Header("References")]
    public BoardManager boardManager;
    public GameObject floatingTextPrefab; 

    [Header("Colors for Quality")]
    public Color greatColor = new Color(0.2f, 0.8f, 0.2f); // Green
    public Color perfectColor = new Color(0.2f, 0.6f, 1.0f); // Blue
    public Color legendaryColor = new Color(0.6f, 0.2f, 0.8f); // Purple
    public Color immortalColor = new Color(1.0f, 0.5f, 0.0f); // Orange/Gold

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void ProcessComboQuality(int jellyCount, int currentCombo, Vector3 spawnPosition)
    {
        // 1-4 Jelly = Match, 5+ = Combo[cite: 1]
        if (jellyCount < 5) return; 

        string qualityTitle = "";
        Color qualityColor = Color.white;
        float textScale = 1.0f;
        bool triggerGlobalShake = false;

        // Combo Quality system logic based on the number of jellies matched
        if (jellyCount >= 5 && jellyCount <= 6)
        {
            qualityTitle = "NORMAL";
            qualityColor = Color.white;
            textScale = 1.0f;
        }
        else if (jellyCount >= 7 && jellyCount <= 9)
        {
            qualityTitle = "GREAT!";
            qualityColor = greatColor;
            textScale = 1.05f;
        }
        else if (jellyCount >= 10 && jellyCount <= 14)
        {
            qualityTitle = "PERFECT!!";
            qualityColor = perfectColor;
            textScale = 1.1f;
        }
        else if (jellyCount >= 15 && jellyCount <= 19)
        {
            qualityTitle = "LEGENDARY!!!";
            qualityColor = legendaryColor;
            textScale = 1.15f;
        }
        else if (jellyCount >= 20)
        {
            qualityTitle = "IMMORTAL!!!";
            qualityColor = immortalColor;
            textScale = 1.2f;
            triggerGlobalShake = true; // 20+ Jelly ASMR Shockwave[cite: 1]
        }

        string finalMessage = $"<size=130%><b>{qualityTitle}</b></size>\n<size=80%>{jellyCount} JELLY</size>\n<size=90%>COMBO x{currentCombo}</size>";

        float centerX = 0f; 
        if (boardManager != null)
        {
            centerX = boardManager.transform.position.x;
        }

        Vector3 centeredPosition = new Vector3(centerX, spawnPosition.y, -5f);

        SpawnJuicyText(finalMessage, centeredPosition, qualityColor, textScale);

        if (triggerGlobalShake && boardManager != null)
        {
            boardManager.TriggerGlobalImmortalShake();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayComboQualitySound(jellyCount);
        }
    }

    private void SpawnJuicyText(string message, Vector3 position, Color finalColor, float targetScale)
    {
        if (floatingTextPrefab == null) return;

        GameObject textObj = Instantiate(floatingTextPrefab, position, Quaternion.identity);
        TextMeshPro textMesh = textObj.GetComponent<TextMeshPro>();
        
        if (textMesh != null)
        {
            textMesh.text = message;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.sortingOrder = 100; 
            textMesh.color = Color.white;
        }

        StartCoroutine(JuicyTextAnimation(textObj, textMesh, targetScale, finalColor));
    }

    private IEnumerator JuicyTextAnimation(GameObject textObj, TextMeshPro tmp, float targetScale, Color finalColor)
    {
        float duration = 1.8f;
        float elapsed = 0f;
        Vector3 startPos = textObj.transform.position;
        
        textObj.transform.localScale = Vector3.zero;

        while (elapsed < duration && textObj != null)
        {
            float t = elapsed / duration;

            // FAZ 1: Explosion and Bursting Out of the Screen (0% - 15%)
            if (t < 0.15f)
            {
                float popT = t / 0.15f;
                float currentScale = Mathf.Lerp(0f, targetScale * 1.5f, popT);
                textObj.transform.localScale = new Vector3(currentScale, currentScale, 1f);
            }
            // FAZ 2: Spring-back and Discoloration (15% - 30%)
            else if (t < 0.3f)
            {
                float settleT = (t - 0.15f) / 0.15f;
                if (tmp != null) tmp.color = Color.Lerp(Color.white, finalColor, settleT);
                float currentScale = Mathf.Lerp(targetScale * 1.5f, targetScale, settleT);
                textObj.transform.localScale = new Vector3(currentScale, currentScale, 1f);
            }
            // FAZ 3: Hovering in the Air, Trembling, and Rising (30% - 80%)
            else if (t < 0.8f)
            {
                float floatT = (t - 0.3f) / 0.5f;
                float wobbleRot = Mathf.Sin(floatT * Mathf.PI * 6f) * 3f; // +/- 3 degree gentle swaying
                textObj.transform.rotation = Quaternion.Euler(0, 0, wobbleRot);
                textObj.transform.position = startPos + new Vector3(0, floatT * 0.8f, 0);
            }
            // FAZ 4: Accelerating and Fading Out (80% - 100%)
            else
            {
                float fadeT = (t - 0.8f) / 0.2f;
                textObj.transform.rotation = Quaternion.Euler(0, 0, 0);
                textObj.transform.position += new Vector3(0, Time.deltaTime * 5f, 0);
                textObj.transform.localScale = Vector3.Lerp(Vector3.one * targetScale, Vector3.zero, fadeT);
                
                if (tmp != null)
                {
                    Color c = tmp.color;
                    c.a = Mathf.Lerp(1f, 0f, fadeT);
                    tmp.color = c;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (textObj != null) Destroy(textObj);
    }
}