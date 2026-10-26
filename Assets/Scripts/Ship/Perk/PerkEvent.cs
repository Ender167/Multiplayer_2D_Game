using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PerkEventType
{
    OnHit,
    OnKill,
    OnDamageTaken,
    OnDamageDealt,
    OnMovement,
    AlwaysOn
}

[System.Serializable]
public class PerkEvent
{
    public PerkEventType type;
}
