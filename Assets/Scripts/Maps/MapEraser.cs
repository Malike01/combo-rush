using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MapEraser : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Silgi Ayarları")]
    public int eraserSize = 30; //[cite: 1]
    public float autoRevealThreshold = 0.8f; //[cite: 1]

    [Header("Juice (Efekt) Ayarları")]
    [Tooltip("Parmağı takip edecek partikül efekti (Yıldızlar/Toz vb.)")] //[cite: 1]
    public ParticleSystem eraseParticles; //[cite: 1]
    
    [Tooltip("Kazıma sırasında çalacak ses (AudioSource içinde Loop açık olmalı)")] //[cite: 1]
    public AudioSource eraseAudio; //[cite: 1]

    private RawImage bwRawImage; //[cite: 1]
    private Texture2D dynamicTexture; //[cite: 1]
    private int totalPixels; //[cite: 1]
    private int erasedPixels; //[cite: 1]
    private bool isRevealed = false; //[cite: 1]

    private void Start()
    {
        bwRawImage = GetComponent<RawImage>(); //[cite: 1]
        
        Texture2D originalTex = (Texture2D)bwRawImage.texture; //[cite: 1]
        dynamicTexture = new Texture2D(originalTex.width, originalTex.height, TextureFormat.RGBA32, false); //[cite: 1]
        dynamicTexture.SetPixels(originalTex.GetPixels()); //[cite: 1]
        dynamicTexture.Apply(); //[cite: 1]

        bwRawImage.texture = dynamicTexture; //[cite: 1]
        
        totalPixels = dynamicTexture.width * dynamicTexture.height; //[cite: 1]
        erasedPixels = 0; //[cite: 1]
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isRevealed) //[cite: 1]
        {
            StartEffects(); //[cite: 1]
            EraseAtPointer(eventData); //[cite: 1]
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isRevealed) 
        {
            StartEffects(); 
            EraseAtPointer(eventData); //[cite: 1]
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        StopEffects();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopEffects(); //[cite: 1]
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopEffects();
    }

    private void EraseAtPointer(PointerEventData eventData)
    {
        RectTransform rectTransform = GetComponent<RectTransform>(); //[cite: 1]
        
        if (eraseParticles != null) //[cite: 1]
        {
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector3 worldPoint)) //[cite: 1]
            {
                eraseParticles.transform.position = worldPoint; //[cite: 1]
            }
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint)) //[cite: 1]
        {
            float normalizedX = (localPoint.x - rectTransform.rect.x) / rectTransform.rect.width; //[cite: 1]
            float normalizedY = (localPoint.y - rectTransform.rect.y) / rectTransform.rect.height; //[cite: 1]

            int pixelX = Mathf.RoundToInt(normalizedX * dynamicTexture.width); //[cite: 1]
            int pixelY = Mathf.RoundToInt(normalizedY * dynamicTexture.height); //[cite: 1]

            EraseCircle(pixelX, pixelY); //[cite: 1]
        }
    }

    private void EraseCircle(int centerX, int centerY)
    {
        bool textureChanged = false; //[cite: 1]

        for (int y = -eraserSize; y <= eraserSize; y++) //[cite: 1]
        {
            for (int x = -eraserSize; x <= eraserSize; x++) //[cite: 1]
            {
                if (x * x + y * y <= eraserSize * eraserSize) //[cite: 1]
                {
                    int pX = centerX + x; //[cite: 1]
                    int pY = centerY + y; //[cite: 1]

                    if (pX >= 0 && pX < dynamicTexture.width && pY >= 0 && pY < dynamicTexture.height) //[cite: 1]
                    {
                        Color currentPixel = dynamicTexture.GetPixel(pX, pY); //[cite: 1]
                        if (currentPixel.a > 0.1f) //[cite: 1]
                        {
                            dynamicTexture.SetPixel(pX, pY, Color.clear); //[cite: 1]
                            erasedPixels++; //[cite: 1]
                            textureChanged = true; //[cite: 1]
                        }
                    }
                }
            }
        }

        if (textureChanged) //[cite: 1]
        {
            dynamicTexture.Apply(); //[cite: 1]
            CheckRevealProgress(); //[cite: 1]
        }
    }

    private void CheckRevealProgress()
    {
        float progress = (float)erasedPixels / totalPixels; //[cite: 1]
        
        if (progress >= autoRevealThreshold && !isRevealed) //[cite: 1]
        {
            isRevealed = true; //[cite: 1]
            RevealAll(); //[cite: 1]
        }
    }

    private void StartEffects()
    {
        if (eraseParticles != null && !eraseParticles.isPlaying) eraseParticles.Play(); //[cite: 1]
        if (eraseAudio != null && !eraseAudio.isPlaying) eraseAudio.Play(); //[cite: 1]
    }

    private void StopEffects()
    {
        if (eraseParticles != null && eraseParticles.isPlaying) eraseParticles.Stop(); //[cite: 1]
        if (eraseAudio != null && eraseAudio.isPlaying) eraseAudio.Pause(); //[cite: 1]
    }

    private void RevealAll()
    {
        StopEffects(); //[cite: 1]
        
        gameObject.SetActive(false); //[cite: 1]
    }
}