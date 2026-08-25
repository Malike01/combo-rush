using UnityEngine;
using UnityEngine.UI; // Eğer Canvas (UI) Image kullanıyorsan bunu aç

public class CoinFaceController : MonoBehaviour
{
    [Header("Yüz İfadeleri (Textures / Sprites)")]
    [Tooltip("0 = Normal Gülümseme, 1 = 3'lü Kombo Yüzü, 2 = Daha Büyük Kombo vs.")]
    public Sprite[] faceLevels; 

    private SpriteRenderer spriteRenderer; 

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        SetFace(0);
    }

    public void SetFace(int levelIndex)
    {
        if (faceLevels == null || faceLevels.Length == 0) return;

        int safeIndex = Mathf.Clamp(levelIndex, 0, faceLevels.Length - 1);

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = faceLevels[safeIndex];
        }
    }
}