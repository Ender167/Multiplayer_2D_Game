using UnityEngine;

//[CreateAssetMenu(fileName = "CruiserControl", menuName = "Scriptable Objects/CruiserControl")]
public class CruiserControl : MovementControl
{
    float currentMoveZ = 0;
    Vector2 currentVelocity;

    public CruiserControl(Rigidbody2D rb, ShipManager shipManager) : base(rb, shipManager)
    {

    }
    public override void MovePlayer(float networkDeltaTime)
    {

        float moveX = Input.GetAxis("Horizontal") * networkDeltaTime;
        float moveZ = Input.GetAxis("Vertical") * networkDeltaTime;

        Transform objTransform = rb.gameObject.transform;

        Vector3 moveDir = (objTransform.up * moveZ).normalized;
        if (Input.GetAxisRaw("Vertical") != 0)
        {
            currentMoveZ = moveZ;
            currentVelocity = rb.linearVelocity;
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, shipManager.shipMovement.CurrentMovementSpeed.Value);
        }
        else
        {
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, currentVelocity.magnitude);
        }

        if (Input.GetKey(KeyCode.X))
            currentVelocity = Vector2.zero;

        Vector3 newDir = (objTransform.up * currentMoveZ).normalized;
        rb.AddForce(newDir * shipManager.shipMovement.CurrentMovementSpeed.Value, ForceMode2D.Force);

        rb.AddTorque(-moveX * shipManager.shipMovement.CurrentTurnSpeed.Value, ForceMode2D.Force);
    }

    public override void MoveNpc(Vector2 dir, float networkDeltaTime)
    {
        Vector2 directionToTarget = (new Vector3(dir.x, dir.y, 0) - rb.gameObject.transform.position).normalized;
        float angleToTarget = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f;
        float currentAngle = rb.gameObject.transform.eulerAngles.z;
        float angleDifference = Mathf.DeltaAngle(currentAngle, angleToTarget);
        float rotationSpeed = shipManager.shipMovement.CurrentTurnSpeed.Value * Time.deltaTime;

        float torqueAmount = Mathf.Clamp(angleDifference, -rotationSpeed, rotationSpeed);

        rb.AddTorque(torqueAmount);

        float dist = Vector2.Distance(new Vector3(dir.x, dir.y, 0), rb.transform.position);

        if (dist < shipManager.shipStats.minFollowRange && dist >= shipManager.shipStats.maxFollowRange)
        {
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, shipManager.shipMovement.CurrentMovementSpeed.Value);

            Vector3 moveDir = rb.gameObject.transform.up.normalized;
            float moveZ = shipManager.shipMovement.CurrentMovementSpeed.Value;

            rb.AddForce(moveDir * moveZ, ForceMode2D.Force);
        }
    }
    /*
    public override void MoveNpc(Vector2 dir, float networkDeltaTime)
    {
        //Transform target = shipManager.shipCombat.GetTarget();
        //if (target != null)
        //{

        //Vector3 directionToTarget = (target.position - rb.gameObject.transform.position).normalized;

        Vector2 directionToTarget = (new Vector3(dir.x, dir.y, 0) - rb.gameObject.transform.position).normalized;
        float angleToTarget = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f;
        float currentAngle = rb.gameObject.transform.eulerAngles.z;
        float angleDifference = Mathf.DeltaAngle(currentAngle, angleToTarget);
        float rotationSpeed = shipManager.shipStats.maxTurnSpeed * Time.deltaTime;

        float turnAmount = Mathf.Clamp(angleDifference, -rotationSpeed, rotationSpeed);
        rb.MoveRotation(rb.rotation + turnAmount);

        //float dist = Vector2.Distance(target.position, rb.transform.position);

        float dist = Vector2.Distance(new Vector3(dir.x, dir.y, 0), rb.transform.position);

        if (dist < shipManager.shipStats.minFollowRange && dist >= shipManager.shipStats.maxFollowRange)
        {
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, shipManager.shipStats.maxSpeed);

            Vector3 moveDir = rb.gameObject.transform.up.normalized;
            float moveZ = shipManager.shipStats.maxSpeed;
            rb.AddForce(moveDir * moveZ, ForceMode2D.Force);
        }

        //}
    }
    */


}
