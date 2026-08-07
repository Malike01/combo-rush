using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Ayarları")]
    public float zoomSpeed = 5f;
    public float minZoom = 3f; 
    public float maxZoom = 15f; 

    [Header("Döndürme Ayarları")]
    public float rotateSpeed = 5f;

    private Camera cam;
    private Vector3 previousPosition;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        HandleZoom();
        HandleRotation();
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            ApplyZoom(-scroll * zoomSpeed * 2f);
        }

        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            ApplyZoom(-difference * 0.01f * zoomSpeed);
        }
    }

    void ApplyZoom(float increment)
    {
        if (cam.orthographic)
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize + increment, minZoom, maxZoom);
        }
        else
        {
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView + increment, minZoom * 5f, maxZoom * 5f);
        }
    }

    void HandleRotation()
    {
        if (Input.GetMouseButtonDown(1))
        {
            previousPosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 direction = previousPosition - Input.mousePosition;
            
            transform.RotateAround(Vector3.zero, Vector3.up, -direction.x * rotateSpeed * Time.deltaTime);

            previousPosition = Input.mousePosition;
        }
    }
}