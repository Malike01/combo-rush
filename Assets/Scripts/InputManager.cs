using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Header("References")]
    public BoardManager boardManager;
    public LineRenderer lineRenderer;
    public LightningLine lightningLine;

    [Header("Color Palette")]
    public Color[] jellyColors = new Color[]
    {
        new Color(0.2f, 0.6f, 1.0f, 1.0f), 
        new Color(0.2f, 0.8f, 0.2f, 1.0f), 
        new Color(1.0f, 0.5f, 0.0f, 1.0f), 
        new Color(0.6f, 0.2f, 0.8f, 1.0f), 
        new Color(0.0f, 0.8f, 0.8f, 1.0f)  
    };

    [Header("Animation Settings")]
    public float scrollSpeed = 2.0f;

    private List<Jelly> selectedJellies = new List<Jelly>();
    private bool isDragging = false;
    private Material lineMaterial;

    void Start()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineMaterial = lineRenderer.material;
            lineRenderer.sortingOrder = 20; 
        }

        if (lightningLine == null)
        {
            lightningLine = GetComponent<LightningLine>();
        }
    }

    void Update()
    {
        if (boardManager != null && boardManager.IsProcessing) return;

        HandleInput();
        AnimateLine();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Jelly startJelly = GetJellyUnderMouse();
            if (startJelly != null)
            {
                isDragging = true;
                SelectJelly(startJelly);
                SetLineColor(startJelly.colorIndex);
            }
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Jelly currentJelly = GetJellyUnderMouse();
            if (currentJelly != null && !selectedJellies.Contains(currentJelly))
            {
                Jelly lastJelly = selectedJellies[selectedJellies.Count - 1];

                if (currentJelly.colorIndex == lastJelly.colorIndex && IsNeighbor(lastJelly, currentJelly))
                {
                    SelectJelly(currentJelly);
                }
            }
            
            if (selectedJellies.Count > 0)
            {
                UpdateLinePositions();
            }
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;

            if (selectedJellies.Count >= 3)
            {
                StartCoroutine(ProcessMergeSequence(new List<Jelly>(selectedJellies)));
            }
            else
            {
                ClearSelection();
            }
        }
    }

    void SelectJelly(Jelly jelly)
    {
        selectedJellies.Add(jelly);
        
        jelly.SelectSquish();

        if (selectedJellies.Count >= 2)
        {
            Jelly previousJelly = selectedJellies[selectedJellies.Count - 2];
            OnJellyConnected(previousJelly, jelly);
        }
    }

    void OnJellyConnected(Jelly currentJelly, Jelly targetJelly)
    {
        Color jellyColor = Color.white;
        if (currentJelly.colorIndex >= 0 && currentJelly.colorIndex < jellyColors.Length)
        {
            jellyColor = jellyColors[currentJelly.colorIndex];
        }

        if (lightningLine != null)
        {
            lightningLine.SetupLightning(currentJelly.transform.position, targetJelly.transform.position, jellyColor);
        }

        StartCoroutine(targetJelly.ShakeJelly());
    }

    private IEnumerator ProcessMergeSequence(List<Jelly> jelliesToMerge)
    {
        if (lineRenderer != null) lineRenderer.positionCount = 0;
        if (lightningLine != null) lightningLine.ClearLightning();

        Vector3 mergeCenter = jelliesToMerge[jelliesToMerge.Count - 1].transform.position;

        for (int i = 0; i < jelliesToMerge.Count; i++)
        {
            float delay = i * 0.03f; 
            StartCoroutine(jelliesToMerge[i].AnimateMergeTo(mergeCenter, delay));
        }

        yield return new WaitForSeconds((jelliesToMerge.Count * 0.03f) + 0.18f);

        boardManager.ProcessMatches(jelliesToMerge);
        selectedJellies.Clear();
    }

    void SetLineColor(int colorIndex)
    {
        if (lineRenderer == null) return;

        Color targetColor = Color.white;
        if (colorIndex >= 0 && colorIndex < jellyColors.Length)
        {
            targetColor = jellyColors[colorIndex];
        }

        targetColor.a = 1.0f; 

        lineRenderer.startColor = targetColor;
        lineRenderer.endColor = targetColor;
    }

    void AnimateLine()
    {
        if (isDragging && lineMaterial != null && lineRenderer.positionCount > 0)
        {
            float offset = Time.time * scrollSpeed;
            lineMaterial.mainTextureOffset = new Vector2(-offset, 0);
        }
    }

    void UpdateLinePositions()
    {
        if (lineRenderer == null) return;

        int pointCount = selectedJellies.Count;
        lineRenderer.positionCount = pointCount + 1;

        for (int i = 0; i < pointCount; i++)
        {
            Vector3 pos = selectedJellies[i].transform.position;
            pos.z = -2f; 
            lineRenderer.SetPosition(i, pos);
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = -2f;
        lineRenderer.SetPosition(pointCount, mouseWorldPos);
    }

    void ClearSelection()
    {
        foreach (var jelly in selectedJellies)
        {
            if (jelly != null)
            {
                jelly.ResetShape();
            }
        }

        selectedJellies.Clear();

        if (lineRenderer != null) lineRenderer.positionCount = 0;
        if (lightningLine != null) lightningLine.ClearLightning();
    }

    Jelly GetJellyUnderMouse()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return null;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCam.transform.position.z);
        Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);

        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, 0.35f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Jelly"))
            {
                return hit.GetComponent<Jelly>();
            }
        }
        return null;
    }

    bool IsNeighbor(Jelly a, Jelly b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx <= 1 && dy <= 1) && !(dx == 0 && dy == 0);
    }
}