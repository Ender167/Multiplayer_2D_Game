using UnityEngine;

[CreateAssetMenu(fileName = "WeaponStats", menuName = "Scriptable Objects/WeaponStats")]
public class WeaponStats : ScriptableObject
{
    public float maxAngle = 45;
    public float fireRate = 1;

    public bool isBurst = false;
    public int shotsPerBurst = 3;
    public float delayPerBurstShot = 0.2f;

    public int energyCost = 0;
    public Damage damage;

    public float maxProjectileVelocity = 10;
    public float timeUntilDestruction = 1f;

    public bool trackMousePosition = true;

    [Header("Phaser Specific")]

    public bool lockOnTarget = false;
    public bool breakChainOnOutrange = false;

    public float phaserDamageFreq = 0.1f;
    public float maxFireRange = 15f;
    public float maxDamageRange = 1f;
}
