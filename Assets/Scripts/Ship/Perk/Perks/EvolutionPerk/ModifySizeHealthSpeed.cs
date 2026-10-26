using UnityEngine;
using System.Collections.Generic;

public class ModifySizeHealthSpeed : Perk
{
    [Header("Specific")]
    public int speedPercentage = 10;
    public int turnRatePercentage = 10;
    public int maxHealthPercentage = 10;
    public int sizePercentage = 10;

    public List<int> groupsToEnable = new List<int>();
    public List<int> groupsToDisable = new List<int>();

    protected override void Setup()

    {

    }
    protected override bool PerkActivationConditional()
    {
        return true;
    }
    protected override void RunInUpdateMode()
    {

    }
    protected override void ApplyPerk()
    {
        if (shipManager != null)
        {
            int newHealth = (shipManager.shipHealth.MaxHealth.Value * maxHealthPercentage) / 100;

            float newSize = (shipManager.transform.localScale.x * sizePercentage) / 100;
            float newMovementSpeed = (shipManager.shipMovement.MaxMovementSpeed.Value * speedPercentage) / 100;
            float newTurnSpeed = (shipManager.shipMovement.MaxTurnSpeed.Value * turnRatePercentage) / 100;

            shipManager.shipHealth.ModifyMaxHealthAttribute(newHealth);
            shipManager.shipMovement.ModifySpeed(newMovementSpeed, newTurnSpeed);
            shipManager.ModifySize(newSize);
            if(groupsToDisable.Count > 0)
                shipManager.shipCombat.ChangeGroupsFiringStatus(groupsToDisable, false);
            if(groupsToEnable.Count > 0)
                shipManager.shipCombat.ChangeGroupsFiringStatus(groupsToEnable, true);
        }
        else
        {
            Debug.Log("null ship manager");
        }
    }
    protected override void RemovePerk()
    {
        ResetTriggeredBool();
    }
}
