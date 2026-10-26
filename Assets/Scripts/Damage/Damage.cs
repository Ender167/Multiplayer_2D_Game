using UnityEngine;
using Unity.Netcode;

[System.Serializable]
public class Damage
{
    public int damageValue;
    public int shieldDamageValue;

    public void CopyDamageValues(Damage newDamage)
    {
        damageValue = newDamage.damageValue;
        shieldDamageValue = newDamage.shieldDamageValue;
    }
    public void IncreaseDamageByPercentage(int newPercentage)
    {
        damageValue += (newPercentage * damageValue) / 100;
    }
}
