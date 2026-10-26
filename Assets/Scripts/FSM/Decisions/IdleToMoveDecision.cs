using UnityEngine;

[CreateAssetMenu(fileName = "New State", menuName = "Scriptable Objects/Npc/Decisions/IdleToMove Decision")]
public class IdleToMoveDecision : Decision
{
    public override bool Decide(FiniteStateMachineController controller)
    {
        Collider2D closestTarget = controller.GetClosestTargetDirection();
        if (closestTarget != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
