using UnityEngine;

[CreateAssetMenu(fileName = "New State", menuName = "Scriptable Objects/Npc/Actions/Idle Action")]
public class IdleAction : Action
{
    public override void Act(FiniteStateMachineController controller)
    {
        Idle(controller);
    }
    void Idle(FiniteStateMachineController controller)
    {

    }
}
