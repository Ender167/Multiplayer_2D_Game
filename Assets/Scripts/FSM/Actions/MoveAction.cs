using UnityEngine;

[CreateAssetMenu(fileName = "New State", menuName = "Scriptable Objects/Npc/Actions/Move Action")]
public class MoveAction : Action
{
    public override void Act(FiniteStateMachineController controller)
    {
        Move(controller);
    }
    void Move(FiniteStateMachineController controller)
    {
        Collider2D closestTarget = controller.GetClosestTargetDirection();
        if(controller.GetCanStrafe())
            controller.SetCanStrafe(false);
            
        if (closestTarget != null)
        {
            controller.target = closestTarget.transform;
            controller.shipManager.shipMovement.SetCanMoveNpc(true);
        }
        else
        {
            controller.target = null;
            controller.shipManager.shipMovement.SetCanMoveNpc(false);
        }
        
    }

}
