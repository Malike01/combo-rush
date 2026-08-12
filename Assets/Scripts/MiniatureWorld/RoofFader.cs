using UnityEngine;

public class RoofFader : MonoBehaviour
{
    [Header("Erime Ayarları")]
    public float fadeDistance = 15f; 
    public float fadeSpeed = 5f;    

    private Camera mainCamera;
    private Material roofMaterial;
    private float currentAlpha = 1f;

    void Start()
    {
        mainCamera = Camera.main;
        
        Renderer renderer = GetComponent<Renderer>();
        if(renderer != null)
        {
            roofMaterial = renderer.material; 
        }
    }

    void Update()
    {
        if (mainCamera == null || roofMaterial == null) return;

        float distance = Vector3.Distance(transform.position, mainCamera.transform.position);

        float targetAlpha = distance < fadeDistance ? 0f : 1f;

        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

        Color newColor = roofMaterial.color;
        newColor.a = currentAlpha;
        roofMaterial.color = newColor;
    }
}