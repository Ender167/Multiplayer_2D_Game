using UnityEngine;

public class RestoreHealthOnKillPerk : Perk
{
    [Header("Specific")]
    public int healthPercentage = 10;
    public bool isPercentageBasedOnDamageDealt = false;
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
            int newHealth = 0;
            if (!isPercentageBasedOnDamageDealt)
            {
                newHealth = (shipManager.shipHealth.MaxHealth.Value * healthPercentage) / 100;
            }
            else
            {
                newHealth = (details.damageDealt.damageValue * healthPercentage) / 100;
            }
            shipManager.shipHealth.ModifyHealthAttribute(newHealth);
            ResetTriggeredBool();
        }
        else
        {
            Debug.Log("null ship manager");
        }
    }
    protected override void RemovePerk()
    {

    }
}
