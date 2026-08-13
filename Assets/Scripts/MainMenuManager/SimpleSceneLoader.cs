using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneLoader : MonoBehaviour
{
    [Header("Gidilecek Sahne Adı")]
    public string bookSceneName = "MainMenu";

    public void LoadBookScene()
    {
        SceneManager.LoadScene(bookSceneName);
    }
}