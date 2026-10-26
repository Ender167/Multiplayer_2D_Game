using UnityEngine;
using Unity.Netcode;

public class MapManager : NetworkBehaviour
{
    public Camera mainCamera;
    public Camera mapCamera;

    public float zoomSpeed = 20;
    public float minZoom = 40f;
    public float maxZoom = 100f;

    public float panSpeed = 20f;
    private Vector3 lastMousePosition;

    float fixedZ;
    bool camState = false;

    void Start()
    {
        mapCamera = GameObject.FindWithTag("MapCamera").GetComponent<Camera>();

        if (!IsOwner) return;

        gameObject.GetComponent<ShipManager>().shipHealth.mainSprite.gameObject.layer = LayerMask.NameToLayer("LocalPlayer");

        mapCamera.cullingMask = LayerMask.GetMask("LocalPlayer", "Map");

        mainCamera.enabled = !camState;
        mapCamera.enabled = camState;

        fixedZ = mapCamera.transform.position.z;
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.M))
        {
            camState = !camState;
            mainCamera.enabled = !camState;
            mapCamera.enabled = camState;
        }
        if (camState)
        {
            HandlePan();
            HandleZoom();
        }
    }
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            Vector3 direction = (mousePosition - mapCamera.transform.position).normalized;

            float zoomAmount = scroll * zoomSpeed;
            float newSize = Mathf.Clamp(mapCamera.orthographicSize - zoomAmount, minZoom, maxZoom);

            mapCamera.orthographicSize = newSize;

            Vector3 newPosition = mapCamera.transform.position + direction * zoomAmount * 0.5f;
            newPosition.z = fixedZ;
            mapCamera.transform.position = newPosition;
        }
    }

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-delta.x, -delta.y, 0) * panSpeed * Time.deltaTime;

            mapCamera.transform.position += move;
            mapCamera.transform.position = new Vector3(mapCamera.transform.position.x, mapCamera.transform.position.y, fixedZ); // Lock Z
            lastMousePosition = Input.mousePosition;
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = -mapCamera.transform.position.z;
        return mapCamera.ScreenToWorldPoint(mouseScreenPos);
    }


    public bool IsMapOpen()
    {
        return camState;
    }
}
