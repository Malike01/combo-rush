using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class WikiPage
{
    [Tooltip("Localization Manager'daki başlık anahtarı (Örn: wiki_1_title)")]
    public string titleKey; 
    
    [Tooltip("Localization Manager'daki içerik anahtarı (Örn: wiki_1_content)")]
    public string contentKey; 
}

public class WikiManager : MonoBehaviour
{
    [Header("UI Referansları")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI contentText;
    public Button nextButton;
    public Button prevButton;

    [Header("Wiki Sayfaları")]
    public WikiPage[] pages;

    private int currentPageIndex = 0;

    private void Start()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += UpdatePageDisplay;
        }
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= UpdatePageDisplay;
        }
    }

    private void OnEnable()
    {
        currentPageIndex = 0;
        UpdatePageDisplay();
    }

    public void NextPage()
    {
        if (currentPageIndex < pages.Length - 1)
        {
            currentPageIndex++;
            UpdatePageDisplay();
        }
    }

    public void PrevPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageDisplay();
        }
    }

    private void UpdatePageDisplay()
    {
        if (pages == null || pages.Length == 0) return;

        string tKey = pages[currentPageIndex].titleKey;
        string cKey = pages[currentPageIndex].contentKey;

        if (LocalizationManager.Instance != null)
        {
            titleText.text = LocalizationManager.Instance.GetText(tKey);
            contentText.text = LocalizationManager.Instance.GetText(cKey);
        }
        else
        {
            titleText.text = tKey;
            contentText.text = cKey;
        }


        if (prevButton != null)
        {
            prevButton.gameObject.SetActive(currentPageIndex > 0);
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(currentPageIndex < pages.Length - 1);
        }
    }
}