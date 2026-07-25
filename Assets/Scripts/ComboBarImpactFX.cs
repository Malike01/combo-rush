using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ComboBarImpactFX : MonoBehaviour
{
    public static ComboBarImpactFX Instance;
    
    // Kod ile üreteceğimiz kusursuz yuvarlak görseli
    private Sprite generatedCircleSprite;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        // Oyun başlarken kendi yuvarlak görselimizi yaratıyoruz.
        // Bu sayede Inspector'dan yanlış görsel atanması riskini sıfırlıyoruz.
        GeneratePerfectCircle();
    }

    private void GeneratePerfectCircle()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color[] colorArray = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(center, new Vector2(x, y));
                if (dist <= radius)
                {
                    // Kenarları yumuşak (soft edge) bir daire çiziyoruz
                    float alpha = Mathf.Clamp01((radius - dist) / 2f);
                    colorArray[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    colorArray[y * size + x] = Color.clear;
                }
            }
        }
        tex.SetPixels(colorArray);
        tex.Apply();
        
        // Texture'ı Sprite'a dönüştürüyoruz
        generatedCircleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    public void PlayImpact(Color jellyColor)
    {
        // 1. Merkezden dışarı doğru yayılan şok dalgası
        StartCoroutine(ShockwaveRoutine(jellyColor));
        
        // 2. Etrafa temizce saçılan kıvılcımlar
        StartCoroutine(SparksRoutine(jellyColor));
    }

    private IEnumerator ShockwaveRoutine(Color color)
    {
        GameObject waveObj = new GameObject("Shockwave");
        waveObj.transform.SetParent(this.transform, false);

        Image waveImg = waveObj.AddComponent<Image>();
        waveImg.sprite = generatedCircleSprite;
        waveImg.color = color;
        waveImg.raycastTarget = false;

        RectTransform rect = waveImg.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(60f, 60f); // Başlangıç boyutu

        float duration = 0.35f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            // Yuvarlağı büyüt (Şok dalgası gibi)
            rect.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 5f, t);

            // Rengi yavaşça şeffaflaştırarak yok et
            Color fadeColor = color;
            fadeColor.a = Mathf.Lerp(0.6f, 0f, t);
            waveImg.color = fadeColor;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(waveObj);
    }

    private IEnumerator SparksRoutine(Color color)
    {
        int sparkCount = Random.Range(5, 8); // 5-8 arası kıvılcım
        float duration = 0.45f;

        for (int i = 0; i < sparkCount; i++)
        {
            GameObject sparkObj = new GameObject("Spark");
            sparkObj.transform.SetParent(this.transform, false);

            Image sparkImg = sparkObj.AddComponent<Image>();
            sparkImg.sprite = generatedCircleSprite;
            
            // Kıvılcımlar parlak olsun diye orijinal renkle beyazı hafif karıştırıyoruz
            sparkImg.color = Color.Lerp(color, Color.white, 0.3f);
            sparkImg.raycastTarget = false;

            RectTransform rect = sparkImg.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            
            float randomSize = Random.Range(12f, 22f);
            rect.sizeDelta = new Vector2(randomSize, randomSize);
            rect.anchoredPosition = Vector2.zero;

            // Rastgele sağa/sola ve hafif yukarı/aşağı fırlatma yönleri
            Vector2 targetPos = new Vector2(Random.Range(-120f, 120f), Random.Range(-40f, 60f));

            StartCoroutine(AnimateSpark(sparkObj, rect, targetPos, sparkImg.color, duration));
        }
        yield return null;
    }

    private IEnumerator AnimateSpark(GameObject spark, RectTransform rect, Vector2 target, Color color, float duration)
    {
        Vector2 startPos = Vector2.zero;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            // Hızlıca çıkıp yavaşlama efekti (Ease-Out)
            float easeT = 1f - Mathf.Pow(1f - t, 3f);

            rect.anchoredPosition = Vector2.Lerp(startPos, target, easeT);
            
            // Uçarken giderek küçülerek yok olma
            rect.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);

            // Şeffaflık
            Color fade = color;
            fade.a = Mathf.Lerp(1f, 0f, t);
            spark.GetComponent<Image>().color = fade;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(spark);
    }
}