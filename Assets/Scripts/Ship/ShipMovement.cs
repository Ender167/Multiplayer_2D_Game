using Unity.Netcode;
using UnityEngine;

public class ShipMovement : NetworkBehaviour
{
    private Rigidbody2D rb;
    [System.NonSerialized] public NetworkVariable<Vector2> Position = new NetworkVariable<Vector2>();

    public NetworkVariable<float> CurrentTurnSpeed = new NetworkVariable<float>();
    public NetworkVariable<float> CurrentMovementSpeed = new NetworkVariable<float>();
    [System.NonSerialized] public NetworkVariable<float> MaxTurnSpeed = new NetworkVariable<float>();
    [System.NonSerialized] public NetworkVariable<float> MaxMovementSpeed = new NetworkVariable<float>();

    MovementControl movementControl;
    ShipManager shipManager;

    bool canMoveNpc = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Position.OnValueChanged += OnStateChanged;

        if (!IsOwner) return;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        shipManager = GetComponent<ShipManager>();
        InitializeSpeed();
        movementControl = shipManager.shipStats.GetMovementController(rb, shipManager);
    }
    void InitializeSpeed()
    {
        if (!IsServer) return;

        MaxTurnSpeed.Value = shipManager.shipStats.maxTurnSpeed;
        MaxMovementSpeed.Value = shipManager.shipStats.maxSpeed;

        CurrentTurnSpeed.Value = shipManager.shipStats.maxTurnSpeed;
        CurrentMovementSpeed.Value = shipManager.shipStats.maxSpeed;
    }
    private void FixedUpdate()
    {
        if (!IsOwner) return;

        float deltaTime = (float)NetworkManager.LocalTime.FixedDeltaTime;

        if (shipManager.IsPlayer())
        {
            movementControl.Move(deltaTime);
        }
        else
        {
            if (canMoveNpc)
            {
                if (!shipManager.npcFsm.GetCanStrafe())
                {
                    movementControl.Move(deltaTime);
                }
                else
                {
                    movementControl.Move(shipManager.npcFsm.GetStrafePoint(), deltaTime);
                }
                
            }
        }
        if (IsServer)
        {
            Position.Value = transform.position;
        }
    }
    public void SetCanMoveNpc(bool newState)
    {
        canMoveNpc = newState;
    }
    public void ModifySpeed(float newMovementSpeed, float newTurnSpeed)
    {
        if (!IsServer) return;

        CurrentMovementSpeed.Value += newMovementSpeed;
        CurrentTurnSpeed.Value += newTurnSpeed;
    }
    public void OnStateChanged(Vector2 previous, Vector2 current)
    {
        if (!IsOwner && Position.Value != previous)
        {
            transform.position = Position.Value;
        }
    }
}
