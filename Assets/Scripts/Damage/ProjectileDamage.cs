using UnityEngine;
using Unity.Netcode;

public class ProjectileDamage : DamageDealer
{
    public float velocity = 10;
    public bool destroyOnCollision = true;


    Rigidbody2D rb;
    protected override void Init()
    {
        
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (!IsServer) return;
    }
    private void Update()
    {
        if (IsServer)
        {
            Position.Value = transform.position;
        }
    }
    public override void Fire(Vector2 direction)
    {
        velocity = stats.maxProjectileVelocity;
        PropelProjectile(direction);

        RequestAudioSpawnServerRpc();
    }
    public override void OnStateChanged(Vector2 previous, Vector2 current)
    {
        if (!IsOwner && Position.Value != previous)
        {
            transform.position = Position.Value;
        }
    }

    /*
    public override void SetOrigin(GameObject origin1)
    {
        //UNUSED
        if (IsServer)
        {
            origin = origin1;
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), origin.GetComponent<Collider2D>());
            ReceiveOriginUpdateClientRpc(origin1.GetComponent<ShipManager>().GetNetworkObject());
        }
    }
    */
    public override void SetStats(WeaponStats newStats)
    {
        stats = newStats;
    }


    public void PropelProjectile(Vector2 direction)
    {
        transform.up = direction;
        rb.linearVelocity = direction * velocity;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        ProjectileDamage projectile = collision.gameObject.GetComponent<ProjectileDamage>();
        ShipHealth targetHealth = collision.gameObject.GetComponent<ShipHealth>();

        if (projectile != null) //&& projectile.origin == origin)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), projectile.GetComponent<Collider2D>());
            return;
        }

        HandleDamageRequest(targetHealth);

        if (spawnVfxOnCollision && destroyOnCollision)
        {
            RequestDestructionAndVfxServerRpc();
            return;
        }

        if (spawnVfxOnCollision)
        {
            RequestVfxSpawnRpc(transform.position.x, transform.position.y);
        }
        if (destroyOnCollision)
        {
            RequestProjectileDestructionRpc();
        }
    }

    [Rpc(SendTo.Server)]
    public void RequestProjectileDestructionRpc()
    {
        if (!IsServer) return;

        GetComponent<NetworkObject>().Despawn();

    }

    [Rpc(SendTo.Server)]
    public void RequestDestructionAndVfxServerRpc()
    {
        if (!IsServer) return;

        if (vfxOnCollision != null)
        {
            GameObject clone = Instantiate(vfxOnCollision, transform.position, Quaternion.identity);
            clone.GetComponent<NetworkObject>().Spawn();
        }

        GetComponent<NetworkObject>().Despawn();

    }

}
