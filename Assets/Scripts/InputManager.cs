using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Header("References")]
    public BoardManager boardManager;
    public LineRenderer lineRenderer;

    [Header("Color Palette")]
    public Color[] jellyColors = new Color[]
    {
        new Color(0.2f, 0.6f, 1.0f, 1.0f), 
        new Color(0.2f, 0.8f, 0.2f, 1.0f), 
        new Color(1.0f, 0.5f, 0.0f, 1.0f), 
        new Color(0.6f, 0.2f, 0.8f, 1.0f), 
        new Color(0.0f, 0.8f, 0.8f, 1.0f)  
    };

    [Header("Line Visuals")]
    public float lineWidth = 0.2f;

    [Header("Idle / Hover Settings")]
    public float hoverTimeThreshold = 5.0f; // Time required to trigger synchronized wiggle

    private List<Jelly> selectedJellies = new List<Jelly>();
    private bool isDragging = false;

    // Hover Tracking Variables
    private Jelly hoveredJelly = null;
    private float hoverTimer = 0f;

    void Start()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.sortingOrder = 50;
        }
    }

    void Update()
    {
        if (boardManager != null && boardManager.IsProcessing) return;

        HandleInput();
        HandleHoverDetection();
    }

    // --- SYNCHRONIZED IDLE WIGGLE DETECTION ---
    void HandleHoverDetection()
    {
        Jelly currentJelly = GetJellyUnderMouse();

        if (currentJelly != null)
        {
            // Track if cursor stays on the same target jelly
            if (currentJelly == hoveredJelly)
            {
                hoverTimer += Time.deltaTime;

                if (hoverTimer >= hoverTimeThreshold)
                {
                    // IF a combo selection exists, wiggle ALL selected jellies together
                    if (selectedJellies.Count > 0)
                    {
                        foreach (var jelly in selectedJellies)
                        {
                            if (jelly != null)
                            {
                                jelly.StartIdleWiggle();
                            }
                        }
                    }
                    // ELSE wiggle only the single hovered jelly
                    else
                    {
                        currentJelly.StartIdleWiggle();
                    }

                    hoverTimer = 0f; // Reset timer for continuous breathing loop
                }
            }
            else
            {
                ResetHoverTimer();
                hoveredJelly = currentJelly;
            }
        }
        else
        {
            ResetHoverTimer();
        }
    }

    void ResetHoverTimer()
    {
        // Stop wiggle on active selection or single hovered target
        if (selectedJellies.Count > 0)
        {
            foreach (var jelly in selectedJellies)
            {
                if (jelly != null) jelly.StopIdleWiggle();
            }
        }
        else if (hoveredJelly != null)
        {
            hoveredJelly.StopIdleWiggle();
        }

        hoveredJelly = null;
        hoverTimer = 0f;
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
                UpdateFullLinePositions();
            }
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            ResetHoverTimer();

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
        StartCoroutine(jelly.ShakeJelly(0.08f, 0.03f));
    }

    void UpdateFullLinePositions()
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

    private IEnumerator ProcessMergeSequence(List<Jelly> jelliesToMerge)
    {
        if (lineRenderer != null) lineRenderer.positionCount = 0;

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

        lineRenderer.startColor = targetColor;
        lineRenderer.endColor = targetColor;
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