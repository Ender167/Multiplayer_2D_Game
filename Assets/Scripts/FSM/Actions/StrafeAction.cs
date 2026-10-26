using UnityEngine;

[CreateAssetMenu(fileName = "New State", menuName = "Scriptable Objects/Npc/Actions/Strafe Action")]
public class StrafeAction : Action
{
    public override void Act(FiniteStateMachineController controller)
    {
        Strafe(controller);
    }
    void Strafe(FiniteStateMachineController controller)
    {
        if (!controller.GetCanStrafe())
        {
            controller.AssignStrafePoint();
            controller.SetCanStrafe(true);
        }
        
    }
}
