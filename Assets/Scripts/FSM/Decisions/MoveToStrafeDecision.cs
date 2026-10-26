using UnityEngine;

[CreateAssetMenu(fileName = "New State", menuName = "Scriptable Objects/Npc/Decisions/MoveToStrafe Decision")]
public class MoveToStrafeDecision : Decision
{
    public override bool Decide(FiniteStateMachineController controller)
    {
        if (!controller.GetCanStrafe() && controller.IsStrafeTimerDone())
        {
            return true;
        }
        return false;
    }
}
