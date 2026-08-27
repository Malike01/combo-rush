using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening; // Required for DOTween

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
        // 1-4 Jelly = Match, 5+ = Combo
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
            textScale = 1.15f; // Slightly increased for a juicier feel
        }
        else if (jellyCount >= 10 && jellyCount <= 14)
        {
            qualityTitle = "PERFECT!!";
            qualityColor = perfectColor;
            textScale = 1.3f;
        }
        else if (jellyCount >= 15 && jellyCount <= 19)
        {
            qualityTitle = "LEGENDARY!!!";
            qualityColor = legendaryColor;
            textScale = 1.45f;
        }
        else if (jellyCount >= 20)
        {
            qualityTitle = "IMMORTAL!!!";
            qualityColor = immortalColor;
            textScale = 1.6f;
            triggerGlobalShake = true; // 20+ Jelly ASMR Shockwave
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
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.TriggerShake(0.4f, 0.6f, 25);
            }
        }

        if (AudioManager.Instance != null)
        {
            // AudioManager.Instance.PlayComboQualitySound(jellyCount);
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
            // Start with white color to give a flash effect before settling into the final color
            textMesh.color = Color.white; 

            MeshRenderer mr = textObj.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sortingLayerName = "UI"; 
                mr.sortingOrder = 9999;
            }
        }

        // Trigger the DOTween animation sequence
        AnimateWithDOTween(textObj, textMesh, targetScale, finalColor);
    }

    private void AnimateWithDOTween(GameObject textObj, TextMeshPro tmp, float targetScale, Color finalColor)
    {
        // 1. Initial Setup: Set scale to zero so it can pop up
        textObj.transform.localScale = Vector3.zero;

        // Create a DOTween Sequence to chain animations step-by-step
        Sequence seq = DOTween.Sequence();

        // PHASE 1: Pop Up & Color Flash (0.4 Seconds)
        // The text springs up using Ease.OutBack, giving it a bouncy, juicy feel.
        seq.Append(textObj.transform.DOScale(targetScale, 0.4f).SetEase(Ease.OutBack));
        
        if (tmp != null)
        {
            // Smoothly transition from white to the combo quality color while popping up
            seq.Join(tmp.DOColor(finalColor, 0.4f));
        }

        // PHASE 2: Wait (1 Second)
        // Keep the text on screen for 1 second so the player can read the combo.
        seq.AppendInterval(1f);

        // PHASE 3: Float Up & Fade Out (0.5 Seconds)
        // Move the text up slightly and fade the alpha to 0 simultaneously.
        seq.Append(textObj.transform.DOMoveY(textObj.transform.position.y + 1.5f, 0.5f).SetEase(Ease.InQuad));
        
        if (tmp != null)
        {
            seq.Join(tmp.DOFade(0f, 0.5f));
        }

        // Slightly shrink it back down while fading out for a clean exit
        seq.Join(textObj.transform.DOScale(targetScale * 0.8f, 0.5f));

        // PHASE 4: Cleanup
        // Destroy the GameObject once the entire sequence is complete to free memory.
        seq.OnComplete(() =>
        {
            if (textObj != null) Destroy(textObj);
        });
    }
}