using UnityEngine;

[CreateAssetMenu(fileName = "ShipStats", menuName = "Scriptable Objects/ShipStats")]
public class ShipStats : ScriptableObject
{
    public string shipName;

    [Header("Health and Shields")]
    public int maxHitpoints = 100;
    public int maxShields = 100;
    
    public float shieldRegenStartDelay = 3f;
    public int shieldRegenRate = 1;
    public float shieldRegenFreq = 1;

    [Header("Energy")]
    public int maxEnergy = 100;
    public float energyRegenStartDelay = 1f;
    public int energyRegenRate = 1;
    public float energyRegenFreq = 1;

    [Header("Mobility")]
    public bool isFighter = true;
    public bool isCruiser = false;

    public float maxSpeed = 5;
    public float maxTurnSpeed = 10;

    [Header("Mobility")]
    public float cameraFoV = 5;

    [Header("Npc specific")]
    public float minFollowRange = 20f;
    public float maxFollowRange = 10f;
    public float strafeTimer = 3f;
    public float strafeDuration = 3f;

    public Vector2 maxWeaponFireOffset = Vector2.zero;

    public MovementControl GetMovementController(Rigidbody2D rb, ShipManager shipManager)
    {
        if (isFighter)
        {
            return new FighterControl(rb, shipManager);
        }
        else
        {
            return new CruiserControl(rb, shipManager);
        }
    }
}
