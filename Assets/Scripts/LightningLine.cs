using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningLine : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [Header("Şimşek Akış Ayarları")]
    [SerializeField] private int pointsCount = 12;       // Number of line detail points
    [SerializeField] private float jitterAmount = 0.15f;  // Vibration/Deviation amount
    [SerializeField] private float pulseSpeed = 25f;     // Pulse speed

    [Header("Görsel Kalınlık")]
    [SerializeField] private float startWidth = 0.22f;
    [SerializeField] private float endWidth = 0.15f;

    private Vector3 startPos;
    private Vector3 endPos;
    private Color currentColor;
    private bool isActive = false;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 0;
    }

    public void SetupLightning(Vector3 start, Vector3 end, Color color)
    {
        startPos = start;
        endPos = end;
        currentColor = color;
        isActive = true;

        // Color and width settings for the line renderer
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
    }

    void Update()
    {
        if (!isActive) return;

        GenerateSmoothLightning();
    }

    private void GenerateSmoothLightning()
    {
        lineRenderer.positionCount = pointsCount;
        Vector3 dir = endPos - startPos;
        Vector3 normal = new Vector3(-dir.y, dir.x, 0).normalized;

        lineRenderer.SetPosition(0, startPos);

        for (int i = 1; i < pointsCount - 1; i++)
        {
            float t = (float)i / (pointsCount - 1);
            Vector3 basePoint = Vector3.Lerp(startPos, endPos, t);

            float noise = (Mathf.PerlinNoise(Time.time * pulseSpeed, t * 10f) - 0.5f) * 2f;
            
            float envelope = Mathf.Sin(t * Mathf.PI); 

            Vector3 offset = normal * noise * jitterAmount * envelope;
            lineRenderer.SetPosition(i, basePoint + offset);
        }

        lineRenderer.SetPosition(pointsCount - 1, endPos);
    }

    public void ClearLightning()
    {
        isActive = false;
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
    }
}