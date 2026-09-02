using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [Tooltip("LocalizationManager içindeki karşılık gelen Key adı")]
    public string textKey; 

    private TextMeshProUGUI tmpText;

    private void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        UpdateText();
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += UpdateText;
        }
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
        }
    }

    public void UpdateText()
    {
        if (LocalizationManager.Instance != null && !string.IsNullOrEmpty(textKey))
        {
            tmpText.text = LocalizationManager.Instance.GetText(textKey);
        }
    }
}