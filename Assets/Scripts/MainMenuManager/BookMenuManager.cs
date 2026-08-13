using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; 

public class BookMenuManager : MonoBehaviour
{
    [Header("Geçiş Ayarları")]
    public string sceneToLoad = "MiniatureWorld";
    public float zoomDuration = 1.2f;

    [Header("Kamera ve Hedef")]
    public Camera mainCamera;
    public Transform targetPicture;

    [Header("Loading UI (Yükleme Ekranı)")]
    public GameObject loadingPanel;
    public TextMeshProUGUI loadingText; 

    private bool isTransitioning = false;

    void Start()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    public void EnterMinitureWorld()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        
        StartCoroutine(ZoomAndLoad());
    }

    private IEnumerator ZoomAndLoad()
    {
        Vector3 startPos = mainCamera.transform.position;
        Vector3 targetPos = targetPicture.position; 
        
        float startFOV = mainCamera.fieldOfView;
        float targetFOV = 5f; 

        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;
            float smoothStep = t * t * (3f - 2f * t); 

            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, smoothStep);
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, smoothStep);

            yield return null;
        }

        if (loadingPanel != null) loadingPanel.SetActive(true);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false; 

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            if (loadingText != null)
            {
                loadingText.text = "Minyatür Dünya Hazırlanıyor... %" + (progress * 100).ToString("F0");
            }

            if (asyncLoad.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.5f);
                asyncLoad.allowSceneActivation = true; 
            }

            yield return null;
        }
    }
}