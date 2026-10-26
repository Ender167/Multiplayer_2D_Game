using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FiniteStateMachineController : NetworkBehaviour
{
    public State currentState;
    public State remainState;

    public float turnSpeed = 10f;

    public Transform target;
    public ShipManager shipManager;

    public NetworkVariable<float> strafeTimer = new NetworkVariable<float>(0);
    public NetworkVariable<float> strafeDuration = new NetworkVariable<float>(0);

    public NetworkVariable<bool> isStrafeTimerDone = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isStrafingDone = new NetworkVariable<bool>(false);
    Vector2 strafePoint;
    public bool canStrafe = false;
    
    void Start()
    {
        shipManager = GetComponent<ShipManager>();
    }

    void Update()
    {

        if (IsServer)
        {
            if (!isStrafeTimerDone.Value && currentState.stateName == "Move")
            {
                if (strafeTimer.Value < shipManager.shipStats.strafeTimer)
                {
                    strafeTimer.Value += Time.deltaTime;
                }

                if (strafeTimer.Value >= shipManager.shipStats.strafeTimer)
                {
                    strafeTimer.Value = 0;
                    isStrafeTimerDone.Value = true;
                }
            }
            if(isStrafeTimerDone.Value == true && currentState.stateName == "Strafe")
            {
                if (strafeDuration.Value < shipManager.shipStats.strafeDuration)
                {
                    strafeDuration.Value += Time.deltaTime;
                }

                if (strafeDuration.Value >= shipManager.shipStats.strafeDuration)
                {
                    strafeDuration.Value = 0;
                    isStrafingDone.Value = true;
                }
            }
        }
        if (!IsOwner) return;

        currentState.UpdateState(this);

        if(target != null)
        {
            shipManager.shipCombat.AssignTarget(target);
        }
        else
        {
            shipManager.shipCombat.AssignTarget(null);
        }
    }
    private void OnDrawGizmos()
    {
        if(currentState != null)
        {
            Gizmos.color = currentState.sceneGizmoColor;
        }
    }

    public void TransitionToState(State nextState)
    {
        if(nextState != remainState)
        {
            currentState = nextState;
        }
    }

    public Collider2D GetClosestTargetDirection()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, shipManager.shipStats.minFollowRange);
        Collider2D closestTarget = null;
        if (colliders.Length > 0)
        {
            float minDist = float.MaxValue;
            foreach (Collider2D coll in colliders)
            {
                if (coll.transform.root != transform)
                {
                    ShipManager shipColl = coll.GetComponent<ShipManager>();
                    float distToColl = Vector2.Distance(transform.position, coll.transform.position);
                    if (shipColl != null && shipColl.IsPlayer() && distToColl < minDist)
                    {
                        minDist = distToColl;
                        closestTarget = coll;
                    }
                }
            }
        }
        return closestTarget;
    }
    Vector2 GenerateStrafePoint()
    {
        Vector2 backward = -transform.up;
        Vector2 right = transform.right;
        Vector2 left = -transform.right;

        Vector2 chosenDir = Vector2.zero;

        int randNr = Random.Range(0, 2);
        switch (randNr)
        {
            case 0:
                chosenDir = backward;
                break;
            case 1:
                chosenDir = right;
                break;
            case 2:
                chosenDir = left;
                break;
        }
        return (Vector2) transform.position + chosenDir * 10f;
    }

    public void AssignStrafePoint()
    {
        strafePoint = GenerateStrafePoint();
    }
    public Vector2 GetStrafePoint()
    {
        return strafePoint;
    }
    public bool GetCanStrafe()
    {
        return canStrafe;
    }
    public void SetCanStrafe(bool newState)
    {
        canStrafe = newState;
    }
    public bool IsStrafeTimerDone()
    {
        return isStrafeTimerDone.Value;
    }
    public void SetIsStrafeTimerDone(bool newState)
    {
        if (IsServer)
        {
            isStrafeTimerDone.Value = newState;
        }
    }
    public void SetIsStrafingDone(bool newState)
    {
        if (IsServer)
        {
            isStrafingDone.Value = newState;
        }
    }
    public void LookAtTarget()
    {
        if (target != null)
        {
            Vector3 targetAdjusted = target.transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetAdjusted - transform.position);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

}
