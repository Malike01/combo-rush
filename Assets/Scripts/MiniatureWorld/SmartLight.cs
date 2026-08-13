using UnityEngine;

public class SmartLight : MonoBehaviour
{
    private Light myLight;
    public float nightIntensity = 3f;
    public float fadeSpeed = 3f;      

    private DioramaItem parentItem;

    void Awake()
    {
        myLight = GetComponent<Light>();
        
        if (myLight != null) 
            myLight.intensity = 0f;

        parentItem = GetComponentInParent<DioramaItem>();
    }

    void Update()
    {
        if (myLight == null) return;

        if (parentItem != null && !parentItem.IsUnlocked)
        {
            myLight.intensity = 0f;
            return;
        }

        float targetIntensity = DayNightCycle.isNight ? nightIntensity : 0f;

        myLight.intensity = Mathf.Lerp(myLight.intensity, targetIntensity, Time.deltaTime * fadeSpeed);
    }
}