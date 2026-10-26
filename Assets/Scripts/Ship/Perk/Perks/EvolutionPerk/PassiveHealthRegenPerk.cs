using UnityEngine;

public class PassiveHealthRegenPerk : Perk
{
    [Header("Specific")]
    public int healthPercentage = 1;
    public int healthRegenFrequency = 5;
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
            InvokeRepeating(nameof(RegenSequence), 0, healthRegenFrequency);
            
        }
        else
        {
            Debug.Log("null ship manager");
        }
    }
    protected override void RemovePerk()
    {
        CancelInvoke(nameof(RegenSequence));
        ResetTriggeredBool();
    }

    void RegenSequence()
    {
        int newHealth = (healthPercentage * shipManager.shipHealth.MaxHealth.Value) / 100;
        shipManager.shipHealth.ModifyHealthAttribute(newHealth);
    }
}
