using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerComboSlot : MonoBehaviour
{
    [Header("UI Referansları")]
    public Image backgroundImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI comboText;

    public void SetupSlot(string playerName, int comboScore, Color bgColor)
    {
        if (nameText != null) nameText.text = playerName;
        
        if (comboText != null) comboText.text = "x" + comboScore.ToString();
        
        if (backgroundImage != null) backgroundImage.color = bgColor;
    }
}