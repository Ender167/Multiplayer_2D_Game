using UnityEngine;
using Unity.Netcode;

public abstract class MovementControl
{
    protected Rigidbody2D rb;
    protected ShipManager shipManager;

    public MovementControl(Rigidbody2D rb1, ShipManager shipManager1)
    {
        rb = rb1;
        shipManager = shipManager1;
    }
    public void Move(float networkDeltaTime)
    {
        if (shipManager.IsPlayer())
        {
            MovePlayer(networkDeltaTime);
        }
        else
        {
            Vector2 targetPos = shipManager.shipCombat.GetTarget().position;
            MoveNpc(targetPos, networkDeltaTime);
        }
    }
    public void Move(Vector2 dir, float networkDeltaTime)
    {
        if (shipManager.IsPlayer())
        {
            MovePlayer(networkDeltaTime);
        }
        else
        {
            MoveNpc(dir, networkDeltaTime);
        }
    }

    public abstract void MovePlayer(float networkDeltaTime);
    public abstract void MoveNpc(Vector2 dir, float networkDeltaTime);
}
