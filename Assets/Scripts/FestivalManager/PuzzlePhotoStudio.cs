using UnityEngine;

public class PuzzlePhotoStudio : MonoBehaviour
{
    [Header("Stüdyo Ayarları")]
    public Camera studioCamera;
    public RenderTexture renderTexture;
    public Transform spawnPoint; 
    
    [Header("Referanslar")]
    public FestivalPuzzleManager puzzleManager; 

    private GameObject currentItem;

    public void TakePhotoAndCreatePuzzle(GameObject itemPrefab)
    {
        if (currentItem != null) Destroy(currentItem);

        currentItem = Instantiate(itemPrefab, spawnPoint.position, spawnPoint.rotation);
        
        SetLayerRecursively(currentItem, LayerMask.NameToLayer("PuzzleStudio"));

        RenderTexture.active = renderTexture;
        studioCamera.Render();

        Texture2D snapshot = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        snapshot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        snapshot.Apply();
        RenderTexture.active = null;

        if (puzzleManager != null)
        {
            puzzleManager.puzzleImage = snapshot; 
            puzzleManager.GeneratePuzzle();       
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}