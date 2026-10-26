using UnityEngine;

//[CreateAssetMenu(fileName = "FighterControl", menuName = "Scriptable Objects/FighterControl")]
public class FighterControl : MovementControl
{
    public FighterControl(Rigidbody2D rb, ShipManager shipManager) : base(rb, shipManager)
    {
        
    }
    public override void MovePlayer(float networkDeltaTime)
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Transform objTransform = rb.gameObject.transform;

        Vector3 move = (objTransform.up * moveZ + objTransform.right * moveX) * shipManager.shipMovement.CurrentMovementSpeed.Value;

        rb.linearVelocity = move;

        Vector2 mouseWorldPosition = shipManager.playerCamera.cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorldPosition - (Vector2)objTransform.position).normalized;

        Vector2 newDirection = Vector2.Lerp(objTransform.up, direction, networkDeltaTime * shipManager.shipMovement.CurrentTurnSpeed.Value).normalized;

        objTransform.up = newDirection;
    }
    public override void MoveNpc(Vector2 dir, float networkDeltaTime)
    {
        //to implement
    }
}
