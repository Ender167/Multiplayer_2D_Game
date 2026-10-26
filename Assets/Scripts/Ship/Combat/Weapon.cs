using UnityEngine;
using Unity.Netcode;

[System.Serializable]
public class Weapon: NetworkBehaviour
{
    public NetworkVariable<bool> canFire = new NetworkVariable<bool>(true);
    public DamageDealer damageDealer;
    public WeaponStats weaponStats;

    float nextTimeToFire = 0;
    int shotsFiredPerBurst = 0;

    ShipManager manager1;
    GameObject origin1;
    Transform spawnPoint1;
    Vector2 direction1;
    Vector2 targetPos1;
    //ulong callerId1;
    public Weapon()
    {

    }
    public void Fire(GameObject origin, Transform spawnPoint, Vector2 direction, Vector2 targetPos, ShipManager manager)
    {
        origin1 = origin;
        spawnPoint1 = spawnPoint;
        direction1 = direction;
        targetPos1 = targetPos;
        //callerId1 = callerId;
        manager1 = manager;

        if (!weaponStats.isBurst)
        {
            if (Time.time >= nextTimeToFire)
            {
                SingleShot();
                nextTimeToFire = Time.time + 1 / weaponStats.fireRate;
                manager1.UseEnergyForWeapon(this);
            }
        }
        else
        {
            if (Time.time >= nextTimeToFire)
            {
                shotsFiredPerBurst = 0;
                InvokeRepeating(nameof(BurstShot), 0f, weaponStats.delayPerBurstShot);
                nextTimeToFire = Time.time + 1 / weaponStats.fireRate;
                manager1.UseEnergyForWeapon(this);
            }
        }
    }
    public bool CanFire(Transform spawnPoint, Vector2 dir)
    {
        if (canFire.Value && IsDirectionWithinAngle(spawnPoint, dir))
            return true;
        return false;
    }

    void BurstShot()
    {
        shotsFiredPerBurst++;
        SingleShot();
        if(shotsFiredPerBurst >= weaponStats.shotsPerBurst)
        {
            CancelInvoke(nameof(BurstShot));
        }
    }
    void SingleShot()
    {
        DamageDealer clone = Instantiate(damageDealer, spawnPoint1.position, spawnPoint1.rotation);
        clone.GetComponent<NetworkObject>().Spawn();
        //clone.SetOrigin(origin1);

        PhaserDamage phaserClone = clone.GetComponent<PhaserDamage>();

        if (phaserClone != null)
            phaserClone.targetPos = targetPos1;

        clone.SetOriginWeapon(origin1, gameObject);
        clone.SetStats(weaponStats);
        clone.Fire(direction1);
        

    }
    public bool IsDirectionWithinAngle(Transform spawnPoint, Vector2 direction)
    {
        direction.Normalize();
        Vector2 forward = spawnPoint.up;

        if (weaponStats.maxAngle >= 180f)
            return true;

        float dot = Vector2.Dot(forward, direction);
        float angleThreshold = Mathf.Cos(weaponStats.maxAngle * Mathf.Deg2Rad);

        return dot >= angleThreshold;
    }

    public Transform GetSpawnPoint()
    {
        return spawnPoint1;
    }
    public int GetEnergyCost()
    {
        return weaponStats.energyCost;
    }
    public void InitCanFire(bool newState)
    {
        if (!IsServer) return;

        canFire.Value = newState;
    }
    public bool IsTrackingMousePosition()
    {
        if (weaponStats.trackMousePosition)
            return true;
        return false;
    }
}
