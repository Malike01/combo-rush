using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class WikiPage
{
    public string pageTitle;
    [TextArea(5, 10)] 
    public string pageContent;
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

        titleText.text = pages[currentPageIndex].pageTitle;
        contentText.text = pages[currentPageIndex].pageContent;

        prevButton.interactable = (currentPageIndex > 0);
        nextButton.interactable = (currentPageIndex < pages.Length - 1);
    }
}