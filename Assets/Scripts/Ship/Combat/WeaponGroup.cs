using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[System.Serializable]
public class WeaponGroup
{
    public List<WeaponItem> weapons = new List<WeaponItem>();
    public bool canFireGroupLocal = true;
    public int mouseClickId = 0;

    public WeaponGroup()
    {

    }

    public void Init(GameObject origin)
    {
        foreach(WeaponItem wepItem in weapons)
        {
            wepItem.InstantiateWeapon(origin);
            wepItem.weaponInstance.InitCanFire(canFireGroupLocal);
        }
    }
    public void ComputeDirections(GameObject origin, Transform target, Vector2 mousePosition)
    {
        foreach (WeaponItem wep in weapons)
        {
            Vector2 direction = Vector2.zero;
            if (target != null)
                direction = origin.GetComponent<ShipCombat>().GetFireDirection(wep.spawnPoint.position, target.position, mousePosition);
            if (target == null)
                direction = origin.GetComponent<ShipCombat>().GetFireDirection(wep.spawnPoint.position, Vector2.zero, mousePosition);
            wep.direction = direction;

            if (target != null)
                wep.targetPos = target.position;
            if (target == null)
                wep.targetPos = mousePosition;
        }
    }
    public void FireAllWeapons(GameObject origin, Transform target, ulong callerId)
    {
        ShipManager origin_manager = origin.GetComponent<ShipManager>();
        foreach(WeaponItem wep in weapons)
        {
            if (wep.CanFire(origin_manager, wep.direction))
            {
                //, wep.direction, wep.targetPos
                wep.Fire(origin_manager, origin, wep.spawnPoint, callerId);
            }
        }
    }
    public void UpdateGroupFireState(bool newState)
    {
        foreach(WeaponItem wepItem in weapons)
        {
            wepItem.weaponInstance.InitCanFire(newState);
        }
    }

}

[System.Serializable]
public class WeaponItem
{
    public Weapon weaponPref;
    public Weapon weaponInstance;
    public Transform spawnPoint;

    [System.NonSerialized] public Vector2 direction;
    [System.NonSerialized] public Vector2 targetPos;

    public int face = 0; //0 - left, 1 - right, 2 - up, 3 - down

    public void InstantiateWeapon(GameObject origin)
    {
        if (weaponInstance == null)
        {
            FaceSide(origin.transform);
            weaponInstance = Object.Instantiate(weaponPref, spawnPoint.position, spawnPoint.rotation);
            weaponInstance.GetComponent<NetworkObject>().Spawn();
        }
    }
    public bool CanFire(ShipManager manager, Vector2 dir)
    {
        if (weaponInstance != null && weaponInstance.CanFire(spawnPoint, dir) && manager.HasEnergyToFireWeapon(weaponInstance))
            return true;
        return false;
    }
    public void Fire(ShipManager manager, GameObject origin, Transform spawnPoint, ulong callerId) //, Vector2 direction, Vector2 targetPos
    {
        FaceSide(origin.transform);
        if (weaponInstance.IsTrackingMousePosition())
        {
            weaponInstance.Fire(origin, spawnPoint, direction, targetPos, manager);
        }
        else
        {
            weaponInstance.Fire(origin, spawnPoint, spawnPoint.up, targetPos, manager);
        }


    }
    public void FaceSide(Transform origin)
    {
        switch (face)
        {
            case 0:
                spawnPoint.up = -origin.right;
                break;
            case 1:
                spawnPoint.up = origin.right;
                break;
            case 2:
                spawnPoint.up = origin.up;
                break;
            case 3:
                spawnPoint.up = -origin.up;
                break;
            default:
                spawnPoint.up = -origin.up;
                break;
        }
    }
}