using UnityEngine;

public class ExitMenuManager : MonoBehaviour
{
    [Header("UI Referansı")]
    public GameObject exitPanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleExitPanel();
        }
    }

    public void ToggleExitPanel()
    {
        if (exitPanel != null)
        {
            bool isActive = exitPanel.activeSelf;
            exitPanel.SetActive(!isActive);

            Time.timeScale = isActive ? 1f : 0f; 
        }
    }

    public void QuitGameOrReturn()
    {
        Time.timeScale = 1f; 
        
        Debug.Log("Oyundan çıkılıyor / Ana menüye dönülüyor...");
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
        
        //Application.Quit();
    }
}