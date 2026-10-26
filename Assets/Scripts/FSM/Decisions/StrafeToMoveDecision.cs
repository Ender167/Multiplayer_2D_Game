using UnityEngine;

[CreateAssetMenu(fileName = "New State", menuName = "Scriptable Objects/Npc/Decisions/StrafeToMove Decision")]
public class StrafeToMoveDecision : Decision
{
    public override bool Decide(FiniteStateMachineController controller)
    {
        if (controller.GetCanStrafe())
        {
            //float dist = Vector2.Distance(controller.GetStrafePoint(), controller.transform.position);
            if (controller.isStrafingDone.Value == true)
            {
                controller.SetIsStrafeTimerDone(false);
                controller.SetIsStrafingDone(false);
                return true;
            }
        }
        return false;
    }
}
