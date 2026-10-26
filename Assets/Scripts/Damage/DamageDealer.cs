using UnityEngine;
using Unity.Netcode;

public abstract class DamageDealer : NetworkBehaviour
{
    public Damage damage;
    public GameObject origin;
    public GameObject origin_weapon;
    public GameObject audioPref;
    public GameObject vfxOnCollision;

    public bool spawnVfxOnCollision = true;

    [System.NonSerialized] public NetworkVariable<Vector2> Position = new NetworkVariable<Vector2>();
    [SerializeField] protected WeaponStats stats;
    protected abstract void Init();

    public override void OnNetworkSpawn()
    {
        Position.OnValueChanged += OnStateChanged;
        base.OnNetworkSpawn();

        if (!IsOwner) return;
        Init();
    }

    private void Start()
    {
        Init();
    }
    public ShipManager GetOriginShip()
    {
        return origin.GetComponent<ShipManager>();
    }
    public ClientData GetOriginClientData()
    {
        ShipManager originManager = origin.GetComponent<ShipManager>();
        ClientData originClientData = originManager.FetchUserClientData();
        return originClientData;
    }

    public void HandleDamageRequest(ShipHealth targetHealth)
    {
        if (targetHealth != null)
        {
            ShipManager targetManager = targetHealth.gameObject.GetComponent<ShipManager>();
            ShipManager originManager = origin.GetComponent<ShipManager>();

            ClientData playerClientData = targetManager.FetchUserClientData();
            ClientData originClientData = originManager.FetchUserClientData();

            if (targetManager.IsPlayer() && originManager.IsPlayer() && playerClientData.clientTeam.Value != originClientData.clientTeam.Value)
            {
                DealDamage(targetHealth, targetManager);
            }
            if ((!targetManager.IsPlayer() && originManager.IsPlayer()) || (!originManager.IsPlayer() && targetManager.IsPlayer()))
            {
                DealDamage(targetHealth, targetManager);
            }

        }
    }
    public void SetOriginWeapon(GameObject origin1, GameObject origin_weapon1)
    {
        if (IsServer)
        {
            origin = origin1;
            origin_weapon = origin_weapon1;
            if(GetComponent<Collider2D>())
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), origin.GetComponent<Collider2D>());
            ReceiveOriginWeaponUpdateClientRpc(origin1.GetComponent<ShipManager>().GetNetworkObject(), origin_weapon.GetComponent<NetworkObject>());
        }
    }
    protected void DealDamage(ShipHealth targetHealth, ShipManager targetManager)
    {
        targetHealth.TakeDamage(damage, origin);
        SendDamageDealtAndDamageTakenServerRpc(targetManager.root.GetComponent<NetworkObject>());
    }
    [ServerRpc]
    public void SendDamageDealtAndDamageTakenServerRpc(NetworkObjectReference targetManagerRef)
    {
        if (!IsServer) return;

        if (targetManagerRef.TryGet(out NetworkObject targetManagerObj))
        {
            ShipManager targetManager = targetManagerObj.GetComponent<RootToMain>().target.GetComponent<ShipManager>();
            ShipManager originManager = origin.GetComponent<ShipManager>();

            EnemyDetails targetDet = new EnemyDetails();
            targetDet.damageTaken = damage;

            EnemyDetails originDet = new EnemyDetails();
            originDet.damageDealt = damage;

            targetManager.SendMessageToPerkManager(PerkEventType.OnDamageTaken, targetDet);
            originManager.SendMessageToPerkManager(PerkEventType.OnDamageDealt, originDet);
        }

    }
    [ServerRpc]
    public void RequestAudioSpawnServerRpc()
    {
        if (!IsServer) return;

        if (audioPref != null)
        {
            GameObject clone = Instantiate(audioPref, transform.position, Quaternion.identity);
            clone.GetComponent<NetworkObject>().Spawn();
        }

    }

    [ClientRpc]
    public void ReceiveOriginUpdateClientRpc(NetworkObjectReference originRef)
    {
        if (originRef.TryGet(out NetworkObject originObj1))
        {
            NetworkObject originObj = originObj1.GetComponent<NetworkObject>();
            origin = originObj.GetComponent<RootToMain>().target;
        }
    }
    [ClientRpc]
    public void ReceiveOriginWeaponUpdateClientRpc(NetworkObjectReference originRef, NetworkObjectReference originWeaponRef)
    {
        if (originWeaponRef.TryGet(out NetworkObject originWeaponObj1) && originRef.TryGet(out NetworkObject originObj1))
        {
            NetworkObject originObj = originObj1.GetComponent<NetworkObject>();
            origin = originObj.GetComponent<RootToMain>().target;

            NetworkObject originWeaponObj = originWeaponObj1.GetComponent<NetworkObject>();
            origin_weapon = originWeaponObj.gameObject;

            stats = origin_weapon.GetComponent<Weapon>().weaponStats;
            damage.CopyDamageValues(stats.damage);

            if(GetComponent<TimedObjectDestructor>())
                GetComponent<TimedObjectDestructor>().Initialize(stats);
        }
    }

    [Rpc(SendTo.Server)]
    public void RequestVfxSpawnRpc(float x, float y)
    {
        if (!IsServer) return;

        if (vfxOnCollision != null)
        {
            GameObject clone = Instantiate(vfxOnCollision, new Vector3(x, y, 0), Quaternion.identity);
            clone.GetComponent<NetworkObject>().Spawn();
        }

    }

    public abstract void OnStateChanged(Vector2 previous, Vector2 current);
    public abstract void Fire(Vector2 direction);
    //public abstract void SetOrigin(GameObject origin1);
    public abstract void SetStats(WeaponStats newStats);
    //public abstract void SetOriginWeapon(GameObject origin1, GameObject originWeapon1);

}
