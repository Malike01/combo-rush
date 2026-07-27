using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menü Panelleri")]
    public GameObject mainMenuPanel;
    public GameObject wikiPanel;
    public GameObject loginModalPanel;

    [Header("Oyun İçi Arayüzler")]
    public GameObject timerRoot;
    public GameObject bottomPanel;

    [Header("Hesap Durumu")]

    public bool isLoggedIn = false; 

    private void Start()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (wikiPanel != null) wikiPanel.SetActive(false);
        if (loginModalPanel != null) loginModalPanel.SetActive(false);
        
        if (timerRoot != null) timerRoot.SetActive(false);
        if (bottomPanel != null) bottomPanel.SetActive(false);
    }

    public void OnClickPlay()
    {
        if (isLoggedIn)
        {
            StartGameFlow();
        }
        else
        {
            if (loginModalPanel != null) loginModalPanel.SetActive(true);
        }
    }

    public void OnClickPlayAsGuest()
    {
        Debug.Log("Misafir girişi seçildi. Oyuna geçiliyor...");
        StartGameFlow();
    }

    public void OnClickGoogleLogin()
    {
        Debug.Log("Google SDK tetiklenecek... (Şimdilik yapım aşamasında)");
        
        // isLoggedIn = true;
        // StartGameFlow();
    }

    public void OnClickCloseLoginModal()
    {
        if (loginModalPanel != null) loginModalPanel.SetActive(false);
    }

    private void StartGameFlow()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (loginModalPanel != null) loginModalPanel.SetActive(false);

        if (timerRoot != null) timerRoot.SetActive(true);
        if (bottomPanel != null) bottomPanel.SetActive(true);

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