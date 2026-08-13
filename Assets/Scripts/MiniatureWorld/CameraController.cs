using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Hareket (Pan) - Sol Tık")]
    public float panSpeed = 10f;

    [Header("Döndürme (Rotate) - Sağ Tık")]
    public float rotateSpeed = 250f;

    [Header("Zoom - Fare Tekerleği")]
    public float zoomSpeed = 500f; 

    private Vector3 lastPanPos;
    private Vector3 lastRotPos;

    void Update()
    {
        HandlePan();
        HandleRotation();
        HandleZoom();
    }

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(0)) lastPanPos = Input.mousePosition;
        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastPanPos;
            float moveX = -delta.x * panSpeed * Time.deltaTime / (Screen.width / 100f);
            float moveY = -delta.y * panSpeed * Time.deltaTime / (Screen.height / 100f);
            
            transform.Translate(new Vector3(moveX, moveY, 0), Space.Self);
            lastPanPos = Input.mousePosition;
        }
    }

    void HandleRotation()
    {
        if (Input.GetMouseButtonDown(1)) lastRotPos = Input.mousePosition;
        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastRotPos;
            
            transform.Rotate(Vector3.up, delta.x * rotateSpeed * Time.deltaTime / (Screen.width / 100f), Space.World);
            
            lastRotPos = Input.mousePosition;
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            transform.Translate(Vector3.forward * scroll * zoomSpeed * Time.deltaTime, Space.Self);
        }

        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);
            Vector2 t0Prev = t0.position - t0.deltaPosition;
            Vector2 t1Prev = t1.position - t1.deltaPosition;
            
            float prevMag = (t0Prev - t1Prev).magnitude;
            float currentMag = (t0.position - t1.position).magnitude;
            float diff = currentMag - prevMag;
            
            transform.Translate(Vector3.forward * diff * 0.1f * zoomSpeed * Time.deltaTime, Space.Self);
        }

        if (transform.position.y < 2f) 
        {
            transform.position = new Vector3(transform.position.x, 2f, transform.position.z);
        }
    }
}