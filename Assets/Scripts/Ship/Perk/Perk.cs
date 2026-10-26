using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[System.Serializable]
public abstract class Perk : NetworkBehaviour
{
    [Header("Main")]
    public string perkName = "";
    public List<PerkEventType> triggerEvents = new List<PerkEventType>();
    public ShipManager shipManager;

    public EnemyDetails details;

    public PerkEffectType perkType;
    public int maxNumberOfTriggers = -1;


    private int currentNumberOfTriggers = 0;
    private bool isTriggered = false;


    protected abstract void ApplyPerk();
    protected abstract void RemovePerk();
    protected abstract void Setup();
    protected abstract bool PerkActivationConditional();
    protected abstract void RunInUpdateMode();


    public void Init(ShipManager shipM)
    {
        shipManager = shipM;
        Setup();
    }
    private void Update()
    {
        RunInUpdateMode();
    }

    public void ReceivePerkEvent(PerkEventType ev, EnemyDetails det)
    {
        details = det;
        if (CanTriggerPerk(ev))
        {
            if (PerkActivationConditional() && !isTriggered)
            {
                isTriggered = true;
                currentNumberOfTriggers++;
                ApplyPerk();
            }
            if(!PerkActivationConditional())
            {
                if (isTriggered)
                {
                    RemovePerk();
                }
                isTriggered = false;
            }
        }
    }
    protected void ResetTriggeredBool()
    {
        isTriggered = false;
    }
    public void RemoveActivePerk()
    {
        RemovePerk();
    }
    public bool CanTriggerPerk(PerkEventType ev)
    {
        if (triggerEvents.Contains(ev) || triggerEvents.Count == 0)
            return true;
        return false;
    }
    public bool HasReachedMaxTriggers()
    {
        if (maxNumberOfTriggers == -1)
            return false;

        if (currentNumberOfTriggers > maxNumberOfTriggers)
            return true;

        return false;
    }
}
