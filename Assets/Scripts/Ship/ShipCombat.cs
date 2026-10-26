using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShipCombat : NetworkBehaviour
{

    public GameObject projectile;
    public Transform spawnPoint;

    [SerializeField] public List<WeaponGroup> weaponGroups = new List<WeaponGroup>();

    ShipManager shipManager;
    Transform target; //for NPCs

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) return;

        
    }

    private void Start()
    {
        shipManager = GetComponent<ShipManager>();
        InitAllGroups();
    }
    void Update()
    {
        if (!IsOwner) return;

        if (shipManager.IsPlayer())
        {
            if (Input.GetMouseButton(0) && !shipManager.playerMapManager.IsMapOpen())
            {
                Vector2 mousePos = shipManager.playerCamera.cam.ScreenToWorldPoint(Input.mousePosition);
                RequestProjectileSpawnServerRpc(mousePos, 0);
            }
            if (Input.GetMouseButton(1) && !shipManager.playerMapManager.IsMapOpen())
            {
                Vector2 mousePos = shipManager.playerCamera.cam.ScreenToWorldPoint(Input.mousePosition);
                RequestProjectileSpawnServerRpc(mousePos, 1);
            }
        }
        else
        {
            // NPC control
            Collider2D closestTarget = GetClosestTargetDirection();
            if (closestTarget != null)
            {
                target = closestTarget.transform;
                //Vector2 attackDir = (closestTarget.transform.position - transform.position).normalized;
                RequestProjectileSpawnServerRpc(Vector2.zero, 0);
            }
            else
            {
                target = null;
            }

        }
    }
    void StartNpcAttackSequence()
    {
       
    }
    Collider2D GetClosestTargetDirection()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 10);
        Collider2D closestTarget = null;
        if (colliders.Length > 0)
        {
            float minDist = float.MaxValue;
            foreach (Collider2D coll in colliders)
            {
                if (coll.transform.root != transform)
                {
                    ShipManager shipColl = coll.GetComponent<ShipManager>();
                    float distToColl = Vector2.Distance(transform.position, coll.transform.position);
                    if (shipColl != null && shipColl.IsPlayer() && distToColl < minDist)
                    {
                        minDist = distToColl;
                        closestTarget = coll;
                    }
                }
            }
        }
        return closestTarget;
    }
    public Vector2 GetFireDirection(Vector2 startPos, Vector2 target, Vector2 mouseWorldPos)
    {
        Vector2 direction = Vector2.zero;
        Vector2 localTarget = target;

        if(shipManager.playerCamera == null)
        {
            localTarget += new Vector2(Random.Range(-shipManager.shipStats.maxWeaponFireOffset.x, shipManager.shipStats.maxWeaponFireOffset.x), Random.Range(-shipManager.shipStats.maxWeaponFireOffset.y, shipManager.shipStats.maxWeaponFireOffset.y));
        }
        if (shipManager.playerCamera != null)
        {
            localTarget = mouseWorldPos;
            //localTarget = shipManager.playerCamera.cam.ScreenToWorldPoint(mouseWorldPos);
            //Vector2 mouseWorldPosition = shipManager.playerCamera.cam.ScreenToWorldPoint(Input.mousePosition);
            //direction = (mouseWorldPosition - startPos).normalized;
        }

        direction = (localTarget - startPos).normalized;
        
        return direction;
    }
    void InitAllGroups()
    {
        if (!IsServer)
        {
            return;
        }
        foreach (WeaponGroup group in weaponGroups)
        {
            group.Init(gameObject);
        }
    }
    void FireAllGroups(int mouseClickId, ulong callerId)
    {
        foreach(WeaponGroup group in weaponGroups)
        {
            if(shipManager.playerCamera != null)
            {
                if(group.mouseClickId == mouseClickId)
                    group.FireAllWeapons(gameObject, null, callerId);
            }
            else
            {
                group.FireAllWeapons(gameObject, target, callerId);
            }
            
        }
    }
    void ComputeAllDirection(Vector2 mouseWorldPos)
    {
        foreach (WeaponGroup group in weaponGroups)
        {
            if (shipManager.playerCamera != null)
            {
                group.ComputeDirections(gameObject, null, mouseWorldPos);
            }
            else
            {
                group.ComputeDirections(gameObject, target, mouseWorldPos);
            }
        }
    }

    [ServerRpc]
    void RequestProjectileSpawnServerRpc(Vector2 mouseWorldPos, int mouseClickId, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        ulong callerId = rpcParams.Receive.SenderClientId;

        ComputeAllDirection(mouseWorldPos);
        FireAllGroups(mouseClickId, callerId);
    }
    
    public void ChangeGroupsFiringStatus(List<int>idx, bool state)
    {
        for(int i = 0; i < idx.Count; i++)
        {
            ChangeGroupFiringStatus(idx[i], state);
        }
    }
    public void ChangeGroupFiringStatus(int i, bool state)
    {
        if (!IsServer) return;

        weaponGroups[i].UpdateGroupFireState(state);
    }
    public void AssignTarget(Transform newTarget)
    {
        target = newTarget;
    }
    public Transform GetTarget()
    {
        return target;
    }


    private void OnDrawGizmos()
    {
        if (shipManager == null || shipManager.playerCamera == null) return;

        Vector2 direction = GetFireDirection(transform.position, Vector2.zero, Input.mousePosition);
        Vector2 startPosition = transform.position;
        Vector2 endPosition = startPosition + direction * 10;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(startPosition, endPosition);
    }
}
