using Unity.Netcode;
using UnityEngine;

public class ShipHealth : NetworkBehaviour
{
    public NetworkVariable<int> MaxHealth = new NetworkVariable<int>(100);
    public NetworkVariable<int> MaxShield = new NetworkVariable<int>(100);
    public NetworkVariable<int> MaxEnergy = new NetworkVariable<int>(100);

    public NetworkVariable<int> Health = new NetworkVariable<int>(100);
    public NetworkVariable<int> Shield = new NetworkVariable<int>(100);
    public NetworkVariable<int> Energy = new NetworkVariable<int>(100);

    public GameObject ShieldEffect;
    public SpriteRenderer mainSprite;
    public GameObject deathPref;

    public NetworkVariable<bool> wasDamaged = new NetworkVariable<bool>(false);
    public NetworkVariable<float> wasDamagedTimer = new NetworkVariable<float>(0);

    public NetworkVariable<bool> wasEnergyRegenCalled = new NetworkVariable<bool>(false);
    public NetworkVariable<float> energyRegenTimer = new NetworkVariable<float>(0);

    private ShipManager lastAttackerShip;
    private ClientData lastAttacker;
    ShipManager shipManager;

    bool callDeathRpc = false;
    ulong ownerId;
    ClientData ownerClient;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Health.OnValueChanged += OnStateChanged;

        if (!IsOwner) return;

        Initialize();

    }
    public void Start()
    {
        Initialize();
        InitializeHealth();
    }

    void Initialize()
    {
        if (shipManager == null)
        {
            shipManager = GetComponent<ShipManager>();

        }
    }
    void InitializeHealth()
    {
        if (!IsServer) return;

        Shield.Value = shipManager.shipStats.maxShields;
        Health.Value = shipManager.shipStats.maxHitpoints;
        Energy.Value = shipManager.shipStats.maxEnergy;

        MaxHealth.Value = shipManager.shipStats.maxHitpoints;
        MaxShield.Value = shipManager.shipStats.maxShields;
        MaxEnergy.Value = shipManager.shipStats.maxEnergy;
    }

    private void Update()
    {
        

        if (Shield.Value > 0 && wasDamaged.Value)
        {
            ShieldEffect.SetActive(true);
        }
        else
        {
            ShieldEffect.SetActive(false);
        }

        if (IsServer)
        {

            if (Shield.Value >= MaxShield.Value)
            {
                Shield.Value = MaxShield.Value;
            }
            if (Shield.Value < 0)
            {
                Shield.Value = 0;
            }

            if(Health.Value >= MaxHealth.Value)
            {
                Health.Value = MaxHealth.Value;
            }
            if (Health.Value < 0)
            {
                Health.Value = 0;
            }

            if(Energy.Value >= MaxEnergy.Value)
            {
                Energy.Value = MaxEnergy.Value;
            }
            if(Energy.Value < 0)
            {
                Energy.Value = 0;
            }
        }

        if(Energy.Value >= MaxEnergy.Value && wasEnergyRegenCalled.Value)
        {
            if(IsServer)
                wasEnergyRegenCalled.Value = false;
            CancelInvoke(nameof(RestoreEnergy));
        }
        if (Energy.Value < MaxEnergy.Value && !wasEnergyRegenCalled.Value)
        {
            if (energyRegenTimer.Value < shipManager.shipStats.energyRegenStartDelay)
            {
                if (IsServer)
                {
                    energyRegenTimer.Value += Time.deltaTime;
                }
            }
            if (energyRegenTimer.Value >= shipManager.shipStats.energyRegenStartDelay)
            {
                if (IsServer)
                {
                    energyRegenTimer.Value = 0;
                    wasEnergyRegenCalled.Value = true;
                }
                InvokeRepeating(nameof(RestoreEnergy), 0.1f, shipManager.shipStats.energyRegenFreq);
            }
        }

        if (wasDamaged.Value)
        {
            if (wasDamagedTimer.Value < shipManager.shipStats.shieldRegenStartDelay)
            {
                CancelInvoke(nameof(RestoreShield));
                if(IsServer)
                    wasDamagedTimer.Value += Time.deltaTime;
            }
            if (wasDamagedTimer.Value >= shipManager.shipStats.shieldRegenStartDelay)
            {
                if (IsServer)
                {
                    wasDamaged.Value = false;
                    wasDamagedTimer.Value = 0;
                }
                InvokeRepeating(nameof(RestoreShield), 0.1f, shipManager.shipStats.shieldRegenFreq);

            }
        }
        if (!IsOwner) return;

        if (Health.Value <= 0 && !callDeathRpc)
        {
            callDeathRpc = true;
            RequestObjectDespawnServerRpc();
        }
    }

    public void RestoreShield()
    {
        if (!IsServer) return;

        Shield.Value += shipManager.shipStats.shieldRegenRate;

        if(Shield.Value >= MaxShield.Value)
        {
            CancelInvoke(nameof(RestoreShield));
        }
    }
    public void RestoreEnergy()
    {
        if (!IsServer) return;

        Energy.Value += shipManager.shipStats.energyRegenRate;

        if (Energy.Value >= MaxEnergy.Value)
        {
            CancelInvoke(nameof(RestoreEnergy));
        }
    }
    public void FlashSprite()
    {
        if (IsServer)
        {
            RpcFlashSpriteClientRpc();
        }
    }


    void HideSprite()
    {
        if (mainSprite == null) return;
        Color tmp = mainSprite.color;
        tmp.a = 0.2f;
        mainSprite.color = tmp;
        Invoke(nameof(ShowSprite), 0.1f);
    }

    void ShowSprite()
    {
        if (mainSprite == null) return;
        Color tmp = mainSprite.color;
        tmp.a = 1f;
        mainSprite.color = tmp;
    }

    void DelayedDeath()
    {
        if (shipManager.playerCamera != null)
            UpdateClientUiStatusClientRpc(ownerId, ownerClient.GetComponent<NetworkObject>());
        shipManager.root.Despawn();
    }

    public void TakeDamage(Damage dmg, GameObject origin)
    {
        wasDamaged.Value = true;
        wasDamagedTimer.Value = 0;

        if(dmg.damageValue > 0)
        {
            FlashSprite();
        }

        int shieldImpact = Shield.Value - dmg.damageValue;
        if (shieldImpact > 0)
        {
            if(IsServer)
                Shield.Value = Shield.Value - dmg.damageValue;
        }
        else
        {
            Shield.Value = 0;
            int newHealth = Health.Value + shieldImpact;

            if (IsServer)
            {
                Health.Value = newHealth;

                ShipManager attackingShip = origin.GetComponent<ShipManager>();
                lastAttackerShip = attackingShip;

                if (attackingShip.IsPlayer())
                {
                    lastAttacker = attackingShip.FetchUserClientData();
                }
                
            }
        }
        
    }

    public void OnStateChanged(int previous, int current)
    {

    }

    public void ModifyEnergy(Weapon wep)
    {
        if (!IsServer) return;

        Energy.Value -= wep.GetEnergyCost();
    }
    public bool HasEnergyToFireWeapon(Weapon wep)
    {
        if (Energy.Value - wep.GetEnergyCost() > 0)
            return true;
        return false;
    }
    public bool IsAlive()
    {
        if (Health.Value > 0)
            return true;
        return false;
    }

    public void ModifyHealthAttribute(int newHealth)
    {
        if (!IsServer) return;
        Health.Value += newHealth;
    }
    public void ModifyMaxHealthAttribute(int newHealth)
    {
        if (!IsServer) return;
        MaxHealth.Value += newHealth;
    }
    [ClientRpc]
    void RpcFlashSpriteClientRpc()
    {
        Invoke(nameof(HideSprite), 0.1f);
    }
    [ClientRpc]
    public void DisableControlComponentsClientRpc(ulong clientId)
    {
        if (OwnerClientId == clientId)
        {
            shipManager.DisableControlComponents();
        }
    }
    [ClientRpc]
    private void TriggerPerkClientRpc(ulong attackerShipId)
    {
        if (NetworkObjectId == attackerShipId)
        {
            shipManager.SendMessageToPerkManager(PerkEventType.OnKill);
        }
        else
        {
            Debug.Log("attacker not found");
        }
    }
    [ServerRpc]
    public void RequestObjectDespawnServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        ownerId = OwnerClientId;
        ownerClient = null;

        if (shipManager.playerCamera != null)
        {
            ownerClient = LobbyManager.Instance.clients.Find(client => client.GetComponent<NetworkObject>().OwnerClientId == ownerId);
            if (ownerClient != null)
            {
                Debug.Log($"Found owner prefab: {ownerClient.clientName}");
            }
            else
            {
                Debug.LogWarning($"No matching client found for owner ID {ownerId}");
            }

        }

        if (lastAttacker != null)
        {
            LobbyManager.Instance.IncrementTeamScore(lastAttacker);
        }
        if(lastAttackerShip != null)
        {
            //TriggerPerkClientRpc(lastAttackerShip.NetworkObjectId);
            EnemyDetails lastAttackerDet = new EnemyDetails();
            lastAttackerDet.kill = 1;
            lastAttackerShip.SendMessageToPerkManager(PerkEventType.OnKill, lastAttackerDet);
        }

        if (deathPref != null)
        {
            GameObject clone = Instantiate(deathPref, transform.position, transform.rotation);
            clone.GetComponent<NetworkObject>().Spawn();
        }
        DisableControlComponentsClientRpc(rpcParams.Receive.SenderClientId);
        UpdateSpriteStateClientRpc();
        Invoke(nameof(DelayedDeath), 3);

    }
    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateSpriteStateClientRpc()
    {
        mainSprite.enabled = false;

        if (!IsOwner)
        {
            shipManager.DisableControlComponents();
        }
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateClientUiStatusClientRpc(ulong clientId, NetworkObjectReference clientObjectRef)
    {
        if (clientObjectRef.TryGet(out NetworkObject clientObject))
        {
            ClientData clientData = clientObject.GetComponent<ClientData>();
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                BeforeRoundConfig beforeRoundConfig = clientData.GetComponent<BeforeRoundConfig>();
                if (beforeRoundConfig != null)
                {
                    beforeRoundConfig.SetClientUiStatus(true);
                    Debug.Log($"UI status updated for client with ID {clientId}.");
                }
                else
                {
                    Debug.Log($"UI status unchanged for client with ID {clientId}.");
                }
            }
            else
            {
                Debug.Log($"Not the owning client, skipping UI update.");
            }
        }
        else
        {
            Debug.Log($"Cliend data not passed.");
        }
        
    }
}
