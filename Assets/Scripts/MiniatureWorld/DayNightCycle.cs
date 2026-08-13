using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Güneş ve Işık Ayarları")]
    public Light directionalLight; 
    public float dayDurationInSeconds = 120f; 
    
    [Header("Renk Paleti")]
    public Gradient lightColorGradient; 
    public Gradient ambientColorGradient; 

    [Header("Günün Mevcut Durumu (0 ile 1 arası)")]
    [Range(0f, 1f)]
    public float timeOfDay = 0.25f; 

    public static bool isNight = false;

    void Update()
    {
        if (directionalLight == null) return;

        timeOfDay += (Time.deltaTime / dayDurationInSeconds);
        if (timeOfDay >= 1f) timeOfDay = 0f; 

        float sunAngle = timeOfDay * 360f - 90f;
        directionalLight.transform.rotation = Quaternion.Euler(new Vector3(sunAngle, 170f, 0f));

        directionalLight.color = lightColorGradient.Evaluate(timeOfDay);
        RenderSettings.ambientLight = ambientColorGradient.Evaluate(timeOfDay);

        bool currentlyNight = timeOfDay >= 0.5f && timeOfDay <= 0.9f;
        if (currentlyNight != isNight)
        {
            isNight = currentlyNight;
        }
    }
}