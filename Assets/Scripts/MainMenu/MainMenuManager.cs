using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menü Panelleri")]
    public GameObject mainMenuPanel;
    public GameObject wikiPanel;

    [Header("Oyun İçi Arayüzler (Açılacak Olanlar)")]
    public GameObject timerRoot;
    public GameObject comboEvolution;

    private void Start()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (wikiPanel != null) wikiPanel.SetActive(false);
        
        if (timerRoot != null) timerRoot.SetActive(false);
        if (comboEvolution != null) comboEvolution.SetActive(false);
    }

    public void OnClickPlay()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);

        if (timerRoot != null) timerRoot.SetActive(true);
        if (comboEvolution != null) comboEvolution.SetActive(true);

        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.GenerateBoard();
        }

        if (ComboTimerManager.Instance != null)
        {
            ComboTimerManager.Instance.ResetTimerFull();
        }
    }

    public void OnClickOpenWiki()
    {
        if (wikiPanel != null) wikiPanel.SetActive(true);
    }

    public void OnClickCloseWiki()
    {
        if (wikiPanel != null) wikiPanel.SetActive(false);
    }
}