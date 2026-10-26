using Unity.Netcode;
using UnityEngine;

public class CameraControl : NetworkBehaviour
{
    public GameObject Player;
    public Vector3 offset;
    [System.NonSerialized] public Camera cam;

    public NetworkVariable<Vector2> Position = new NetworkVariable<Vector2>();
    Vector3 currentOffset;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Position.OnValueChanged += OnStateChanged;
        //if (!IsOwner) return;

        Init();

    }
    void Init()
    {
        if(cam == null)
        {
            cam = GetComponent<Camera>();
            offset = transform.position - Player.transform.position;
            currentOffset = offset;
        }
    }
    void Update()
    {
        //if (!IsOwner) return;

        transform.position = Player.transform.position + currentOffset;

        if (IsServer)
        {
            Position.Value = transform.position;
        }
    }

    public void EnableCamera(float cameraFoV)
    {
        cam.enabled = true;
        cam.backgroundColor = Color.black;
        cam.GetComponent<AudioListener>().enabled = true;
        cam.orthographicSize = cameraFoV;
    }
    public void OnStateChanged(Vector2 previous, Vector2 current)
    {
        if (!IsOwner && Position.Value != previous)
        {
            transform.position = Position.Value;
            //transform.position = Vector3.Lerp(transform.position, new Vector3(Position.Value.x, Position.Value.y, 0), NetworkManager.LocalTime.FixedDeltaTime * 5f);
        }
    }
}
