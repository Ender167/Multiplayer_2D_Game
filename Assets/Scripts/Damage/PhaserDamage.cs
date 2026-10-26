using UnityEngine;
using Unity.Netcode;

public class PhaserDamage : DamageDealer
{
    
    public Vector2 targetPos;

    ShipHealth targetHealth;
    Vector3 lastPos;
    Vector3 hitPos;

    Vector2 endPoint = Vector2.zero;
    Transform weaponSpawnPoint;
    protected override void Init()
    {

        if (!IsServer) return;
    }

    public override void Fire(Vector2 direction)
    {

        weaponSpawnPoint = origin_weapon.GetComponent<Weapon>().GetSpawnPoint();

        endPoint = targetPos;

        if(Vector2.Distance(endPoint, weaponSpawnPoint.position) >= stats.maxFireRange)
        {
            endPoint = weaponSpawnPoint.position + (new Vector3(targetPos.x, targetPos.y, 0) - weaponSpawnPoint.position).normalized * stats.maxFireRange;
        }
        InvokeRepeating(nameof(DamageOverTime), 0f, stats.phaserDamageFreq);

        /*
        Collider2D target = GetClosestTarget();
        weaponSpawnPoint = origin_weapon.GetComponent<Weapon>().GetSpawnPoint();

        if (target != null)
        {
            targetHealth = target.gameObject.GetComponent<ShipHealth>();
            InvokeRepeating(nameof(DamageOverTime), 0f, stats.phaserDamageFreq);
        }
        else
        {
            //InvokeRepeating(nameof(FindIntersectingTarget), 0f, 0.1f);
            InvokeRepeating(nameof(DamageOverTime), 0f, stats.phaserDamageFreq);
        }
        */

        RequestAudioSpawnServerRpc();
        RequestSpawnPhaserServerRpc();

    }

    private void Update()
    {
        //if (!IsOwner) return;

        if (!IsServer) return;

        if(Vector2.Distance(lastPos, weaponSpawnPoint.position) > 0.05f)
        {
            RequestSpawnPhaserServerRpc();
            lastPos = weaponSpawnPoint.position;
            return;
        }

        if (stats.lockOnTarget)
        {
            if (targetHealth != null && Vector2.Distance(endPoint, targetHealth.transform.position) > 0.05f)
            {
                RequestSpawnPhaserServerRpc();
            }
        }
    }
    public override void OnStateChanged(Vector2 previous, Vector2 current)
    {

    }

    public override void SetStats(WeaponStats newStats)
    {
        stats = newStats;
    }

    void DamageOverTime()
    {
        FindIntersectingTarget();
        if (CanBeamDamage())
            HandleDamageRequest(targetHealth);
    }
    void FindIntersectingTarget()
    {
        float dist_from_wep_to_endpoint = Vector2.Distance(weaponSpawnPoint.position, endPoint);
        //float dist_from_wep_to_endpoint = stats.maxFireRange;
        RaycastHit2D[] hits = Physics2D.RaycastAll(weaponSpawnPoint.position, (new Vector3(endPoint.x, endPoint.y, 0) - weaponSpawnPoint.position).normalized, dist_from_wep_to_endpoint + 0.1f);

        float maxDist = float.MaxValue;
        bool found = false;
        RaycastHit2D closestHit = new RaycastHit2D();
        foreach(RaycastHit2D hit in hits)
        {
            if (hit.transform.root != origin.transform.root)
            {
                float dist = Vector2.Distance(hit.point, weaponSpawnPoint.position);
                if (dist < maxDist)
                {
                    closestHit = hit;
                    maxDist = dist;
                    found = true;
                }
            }
        }

        if (found && closestHit)
        {
            if (origin_weapon.GetComponent<Weapon>().IsDirectionWithinAngle(weaponSpawnPoint, (new Vector3(closestHit.point.x, closestHit.point.y, 0) - weaponSpawnPoint.position).normalized))
            {
                ShipHealth target = closestHit.collider.gameObject.GetComponent<ShipHealth>();
                if (target != null)
                {
                    targetHealth = target;
                }
                else
                {
                    targetHealth = null;
                }
                if (spawnVfxOnCollision)
                    RequestVfxSpawnRpc(closestHit.point.x, closestHit.point.y);
                //hitPos = closestHit.point;
                endPoint = closestHit.point;
                //endPoint = closestHit.point - closestHit.normal * 0.1f;
            }
        }

    }
    Collider2D GetClosestTarget()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(targetPos, stats.maxDamageRange);
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
                    if (shipColl != null && distToColl < minDist)
                    {
                        minDist = distToColl;
                        closestTarget = coll;
                    }
                }
            }
        }
        return closestTarget;
    }
    bool CanFireBeam()
    {
        RaycastHit2D hit = Physics2D.Raycast(weaponSpawnPoint.position, (new Vector3(endPoint.x, endPoint.y, 0) - weaponSpawnPoint.position).normalized, Vector2.Distance(weaponSpawnPoint.position, endPoint));

        if (hit)
        {
            return false;
        }
        return true;
    }
    bool CanBeamDamage()
    {
        RaycastHit2D hit = Physics2D.Raycast(weaponSpawnPoint.position, (new Vector3(endPoint.x, endPoint.y, 0) - weaponSpawnPoint.position).normalized, Vector2.Distance(weaponSpawnPoint.position, endPoint));

        if(targetHealth != null && hit && hit.collider.gameObject != targetHealth.gameObject)
        {
            return false;
        }
        // to fix
        if(!origin_weapon.GetComponent<Weapon>().IsDirectionWithinAngle(weaponSpawnPoint, (new Vector3(endPoint.x, endPoint.y, 0) - weaponSpawnPoint.position).normalized))
        {
            Debug.Log("Here");
            return false;
        }

        return true;
    }

    [ServerRpc]
    public void RequestSpawnPhaserServerRpc()
    {
        if (!IsServer) return;

        LineRenderer phaser = GetComponent<LineRenderer>();
        phaser.positionCount = 2;
        phaser.SetPosition(0, weaponSpawnPoint.position);

        if ((stats.breakChainOnOutrange && Vector2.Distance(weaponSpawnPoint.position, endPoint) < stats.maxFireRange) ||
            origin_weapon.GetComponent<Weapon>().IsDirectionWithinAngle(weaponSpawnPoint, (new Vector3(endPoint.x, endPoint.y, 0) - weaponSpawnPoint.position).normalized))
        {
            if (targetHealth == null || !stats.lockOnTarget)
            {
                phaser.SetPosition(1, endPoint);
                RequestSpawnPhaserNoTargetClientRpc(weaponSpawnPoint.position.x, weaponSpawnPoint.position.y, endPoint.x, endPoint.y);
            }
            else
            {
                if (stats.lockOnTarget && targetHealth != null)
                {
                    phaser.SetPosition(1, targetHealth.transform.position);
                    endPoint = targetHealth.transform.position;
                    RequestSpawnPhaserTargetClientRpc(targetHealth.GetComponent<ShipManager>().root.GetComponent<NetworkObject>(), weaponSpawnPoint.position.x, weaponSpawnPoint.position.y, targetPos.x, targetPos.y);
                }
            }
        }
        else
        {
            phaser.SetPosition(1, weaponSpawnPoint.position);
            RequestSpawnPhaserNoTargetClientRpc(weaponSpawnPoint.position.x, weaponSpawnPoint.position.y, weaponSpawnPoint.position.x, weaponSpawnPoint.position.y);
        }

    }
    [ClientRpc]
    public void RequestSpawnPhaserNoTargetClientRpc(float x_origin, float y_origin,float x_target, float y_target)
    {
        LineRenderer phaser = GetComponent<LineRenderer>();
        phaser.positionCount = 2;
        phaser.SetPosition(0, new Vector2(x_origin, y_origin));
        phaser.SetPosition(1, new Vector2(x_target, y_target));

    }
    [ClientRpc]
    public void RequestSpawnPhaserTargetClientRpc(NetworkObjectReference targetHealthRef, float x_origin, float y_origin, float x, float y)
    {
        //if (IsOwner) return;

        if (targetHealthRef.TryGet(out NetworkObject targetHealth1))
        {
            targetHealth = targetHealth1.GetComponent<RootToMain>().target.GetComponent<ShipHealth>();
        }
        LineRenderer phaser = GetComponent<LineRenderer>();
        phaser.positionCount = 2;
        //phaser.SetPosition(0, weaponSpawnPoint.position);
        phaser.SetPosition(0, new Vector2(x_origin, y_origin));

        if (targetHealth == null)
        {
            phaser.SetPosition(1, new Vector2(x, y));
        }
        else
        {
            phaser.SetPosition(1, targetHealth.transform.position);
        }
        

    }
}
