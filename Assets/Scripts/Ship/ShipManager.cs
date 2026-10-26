using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class ShipManager : NetworkBehaviour
{
    public NetworkObject root;
    public ShipStats shipStats;

    [System.NonSerialized] public ShipMovement shipMovement;
    [System.NonSerialized] public ShipHealth shipHealth;
    [System.NonSerialized] public ShipCombat shipCombat;

    public PerkManager shipPerkManager;

    [Header("Player specific fields")]
    public CameraControl playerCamera;
    public ProjectileDirectionIndicator playerProjectileIndicator;
    public MapManager playerMapManager;
    public UIControl playerUiControl;
    public NetworkVariable<ulong> ownerId = new NetworkVariable<ulong>();

    [Header("Npc specific fields")]
    public FiniteStateMachineController npcFsm;

    public NetworkVariable<Vector3> Scale = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Scale.OnValueChanged += OnScaleChanged;

        if (IsServer)
        {
            if (playerCamera == null)
            {
                root.GetComponent<NetworkObject>().RemoveOwnership();
            }
        }
        if (!IsOwner) return;
    }
    private void Start()
    {
        InitForAll();

        if (!IsOwner) return;

        InitForOwner();
    }
    private void Update()
    {
        if (IsServer)
        {
            if (transform.localScale != Scale.Value)
            {
                Scale.Value = transform.localScale;
            }
        }
    }
    private void OnScaleChanged(Vector3 previous, Vector3 current)
    {
        transform.localScale = current;
    }

    void Init()
    {
        if(shipMovement == null)
        {
            if (playerCamera != null)
                playerCamera.EnableCamera(shipStats.cameraFoV);


            shipHealth = GetComponent<ShipHealth>();
            shipCombat = GetComponent<ShipCombat>();
            shipMovement = GetComponent<ShipMovement>();
            shipPerkManager = GetComponent<PerkManager>();

            playerProjectileIndicator = GetComponent<ProjectileDirectionIndicator>();
            playerMapManager = GetComponent<MapManager>();

            npcFsm = GetComponent<FiniteStateMachineController>();
        }
    }
    void InitForOwner()
    {
        if (playerCamera != null)
            playerCamera.EnableCamera(shipStats.cameraFoV);
    }
    void InitForAll()
    {
        if (shipMovement == null)
        {

            shipHealth = GetComponent<ShipHealth>();
            shipCombat = GetComponent<ShipCombat>();
            shipMovement = GetComponent<ShipMovement>();
            shipPerkManager = GetComponent<PerkManager>();

            playerProjectileIndicator = GetComponent<ProjectileDirectionIndicator>();
            playerMapManager = GetComponent<MapManager>();

            npcFsm = GetComponent<FiniteStateMachineController>();
        }
    }
    public void DisableControlComponents()
    {
        if (IsOwner)
        {
            
            shipCombat.enabled = false;
            shipMovement.enabled = false;

            if(shipPerkManager != null)
            {
                shipPerkManager.enabled = false;
            }
            if(playerProjectileIndicator != null)
            {
                playerProjectileIndicator.enabled = false;
            }
            if(npcFsm != null)
            {
                npcFsm.enabled = false;
            }
        }

        GetComponent<CircleCollider2D>().enabled = false;
    }
    public int GetEnergy()
    {
        return shipHealth.Energy.Value;
    }
    public int GetHealth()
    {
        return shipHealth.Health.Value;
    }
    public int GetShield()
    {
        return shipHealth.Shield.Value;
    }
    public NetworkObject GetNetworkObject()
    {
        return root.GetComponent<NetworkObject>();
    }
    public bool IsPlayer()
    {
        if (playerCamera != null)
            return true;
        return false;
    }
    public void ModifySize(float newSize)
    {
        if (!IsServer) return;

        Vector3 newScale = transform.localScale + new Vector3(newSize, newSize, newSize);
        Scale.Value = newScale;

        UpdateScaleClientRpc(newScale.x, newScale.y);
        //transform.localScale = transform.localScale + new Vector3(newSize, newSize, 0);
    }
    [ClientRpc]
    private void UpdateScaleClientRpc(float x, float y)
    {
        if (IsOwner) return;

        transform.localScale = new Vector3(x, y, 0);
    }
    public void SendMessageToPerkManager(PerkEventType msg, EnemyDetails det = null)
    {
        if (shipPerkManager != null)
        {
            shipPerkManager.ReceiveEventFromEnemy(msg, det);
        }
        else
        {
            //Debug.Log("perk manager is null");
        }
    }
    public ClientData FetchUserClientData()
    {
        ulong userClientId = OwnerClientId;
        ClientData ownerClient = LobbyManager.Instance.clients.Find(client => client.GetComponent<NetworkObject>().OwnerClientId == userClientId);
        return ownerClient;
    }
    public ClientData FetchLocalClientData()
    {
        ulong userClientId = NetworkManager.Singleton.LocalClientId;
        ClientData localClient = LobbyManager.Instance.clients.Find(client => client.GetComponent<NetworkObject>().OwnerClientId == userClientId);
        return localClient;
    }
    public List<ClientData> FetchAllClients()
    {
        return new List<ClientData>(LobbyManager.Instance.clients);
    }
    public void UseEnergyForWeapon(Weapon weapon)
    {
        shipHealth.ModifyEnergy(weapon);
    }
    public bool HasEnergyToFireWeapon(Weapon weapon)
    {
        return shipHealth.HasEnergyToFireWeapon(weapon);
    }
}
