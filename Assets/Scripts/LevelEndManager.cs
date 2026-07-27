using UnityEngine;

public class LevelEndManager : MonoBehaviour
{
    [Header("Paneller")]
    [Tooltip("Şu an ekranda gördüğün, butonların olduğu ana bölüm sonu paneli")]
    public GameObject levelEndPanel; 
    
    [Tooltip("Tam ekran açılacak olan harita paneli")]
    public GameObject mapContainer;      
    public GameObject buttonContainer;

    [Header("Oyun Sistem Referansları")]
    [Tooltip("Sahnendeki ComboTimerManager objesini sürükle")]
    public ComboTimerManager timerManager;

    [Tooltip("Sahnendeki BoardManager (veya grid'i oluşturan) objeyi sürükle")]
    public BoardManager boardManager;

    private void Start()
    {
        if (mapContainer != null)
        {
            mapContainer.SetActive(false);
        }
    }

    public void ShowLevelEndScreen()
    {
        Debug.Log("Süre bitti, Bölüm Sonu Ekranı açılıyor!");
        
        transform.SetAsLastSibling(); 
        
        if (levelEndPanel != null)
        {
            levelEndPanel.SetActive(true);
        }

        if (mapContainer != null)
        {
            mapContainer.SetActive(false);
        }
    }

    public void OnClickContinue()
    {
        Debug.Log("Devam Et'e tıklandı, oyun yeniden başlatılıyor!");
        
        if (levelEndPanel != null)
        {
            levelEndPanel.SetActive(false); 
        }

        if (boardManager != null)
        {
            boardManager.GenerateBoard(); 
        }
        else
        {
            var board = FindFirstObjectByType<BoardManager>();
            if (board != null) board.GenerateBoard();
        }

        if (timerManager != null)
        {
            timerManager.StartOrResetTimer(); 
        }
        else
        {
            var timer = FindFirstObjectByType<ComboTimerManager>();
            if (timer != null) timer.ResetTimerFull();
        }
    }

    public void OnClickCombora()
    {
        Debug.Log("Combora'ya tıklandı, Harita tam ekran açılıyor!");
        
        if (mapContainer != null)
        {
            mapContainer.SetActive(true);
        }

        if(buttonContainer != null) buttonContainer.SetActive(false);
    }
}