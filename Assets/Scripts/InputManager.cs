using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private List<Jelly> selectedJellies = new List<Jelly>();
    private bool isSelecting = false;
    private int currentColorIndex = -1;
    private BoardManager boardManager;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        boardManager = FindObjectOfType<BoardManager>();
    }

    void Update()
    {
        // Board meşgulse girdi alma
        if (boardManager != null && boardManager.IsProcessing) return;

        if (Input.GetMouseButtonDown(0))
        {
            StartSelection();
        }

        if (Input.GetMouseButton(0) && isSelecting)
        {
            UpdateSelection();
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndSelection();
        }
    }

    void StartSelection()
    {
        Jelly hitJelly = GetJellyUnderMouse();

        if (hitJelly != null)
        {
            isSelecting = true;
            selectedJellies.Clear();
            currentColorIndex = hitJelly.colorIndex;
            AddJellyToChain(hitJelly);
        }
    }

    void UpdateSelection()
    {
        Jelly hitJelly = GetJellyUnderMouse();

        if (hitJelly != null && hitJelly.colorIndex == currentColorIndex)
        {
            // Undo (Delete the last one if reverting to the previous Jelly)
            if (selectedJellies.Count > 1 && hitJelly == selectedJellies[selectedJellies.Count - 2])
            {
                selectedJellies.RemoveAt(selectedJellies.Count - 1);
                UpdateLinePosition();
            }
            // Add Jelly, which is a neighbor and has not yet been selected
            else if (!selectedJellies.Contains(hitJelly) && IsNeighbor(selectedJellies[selectedJellies.Count - 1], hitJelly))
            {
                AddJellyToChain(hitJelly);
            }
        }
    }

    Jelly GetJellyUnderMouse()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Jelly"))
        {
            return hit.collider.GetComponent<Jelly>();
        }
        return null;
    }

    void AddJellyToChain(Jelly jelly)
    {
        selectedJellies.Add(jelly);
        UpdateLinePosition();
    }

    void UpdateLinePosition()
    {
        if (selectedJellies.Count == 0)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        // The line passes only through the exact centers of the selected Jellies (no free ends)
        lineRenderer.positionCount = selectedJellies.Count;

        for (int i = 0; i < selectedJellies.Count; i++)
        {
            if (selectedJellies[i] != null)
            {
                Vector3 pos = selectedJellies[i].transform.position;
                lineRenderer.SetPosition(i, new Vector3(pos.x, pos.y, -1f));
            }
        }
    }

    bool IsNeighbor(Jelly lastJelly, Jelly newJelly)
    {
        int xDiff = Mathf.Abs(lastJelly.x - newJelly.x);
        int yDiff = Mathf.Abs(lastJelly.y - newJelly.y);

        return (xDiff == 1 && yDiff == 0) || (xDiff == 0 && yDiff == 1);
    }

   void EndSelection()
    {
        lineRenderer.positionCount = 0;
        isSelecting = false;

        if (selectedJellies.Count >= 3)
        {
            // 1.Update Score and Combo
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(selectedJellies.Count);
            }

            // 2.Start the Blasting Operation
            if (boardManager != null)
            {
                boardManager.ProcessMatches(new List<Jelly>(selectedJellies));
            }
        }

        selectedJellies.Clear();
    }
}