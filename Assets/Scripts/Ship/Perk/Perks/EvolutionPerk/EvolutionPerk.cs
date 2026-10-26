using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class EvolutionPerk : Perk
{
    [Header("Specific")]
    public int damageTakenThreshold = 500;
    public int damageDealtThreshold = 500;
    public int killsThreshold = 5;

    public int maxEvolutions = 2;
    public NetworkVariable<int> currentEvolution = new NetworkVariable<int>(0);

    public NetworkVariable<int> damageTaken = new NetworkVariable<int>(0);
    public NetworkVariable<int> damageDealt = new NetworkVariable<int>(0);
    public NetworkVariable<int> kills = new NetworkVariable<int>(0);

    [SerializeField] public List<Perk> damageTakenThresholdPerks = new List<Perk>();
    [SerializeField] public List<Perk> damageDealtThresholdPerks = new List<Perk>();
    [SerializeField] public List<Perk> killsThresholdPerks = new List<Perk>();


    protected override void Setup()
    {
        kills.OnValueChanged += (oldValue, newValue) =>
        {
            shipManager?.playerUiControl?.UpdateEvolutionUi(this);
        };
        damageDealt.OnValueChanged += (oldValue, newValue) =>
        {
            shipManager?.playerUiControl?.UpdateEvolutionUi(this);
        };
        damageTaken.OnValueChanged += (oldValue, newValue) =>
        {
            shipManager?.playerUiControl?.UpdateEvolutionUi(this);
        };

        if (shipManager != null && shipManager.playerUiControl != null)
        {
            shipManager.playerUiControl.EnableEvolutionUi();
            shipManager.playerUiControl.UpdateEvolutionUi(this);
        }
    }
    protected override void RunInUpdateMode()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (currentEvolution.Value < maxEvolutions && damageTaken.Value >= damageTakenThreshold)
            {
                AddEvolutionPerkServerRpc(0);
                UpdateEvolutionUiServerRpc();
                //ResetCounters();
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (currentEvolution.Value < maxEvolutions && damageDealt.Value >= damageDealtThreshold)
            {
                AddEvolutionPerkServerRpc(1);
                UpdateEvolutionUiServerRpc();
                //shipManager.shipPerkManager.AddPerk(damageDealtThresholdPerks[currentEvolution.Value]);

            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (currentEvolution.Value < maxEvolutions && kills.Value >= killsThreshold)
            {
                AddEvolutionPerkServerRpc(2);
                UpdateEvolutionUiServerRpc();
                //shipManager.shipPerkManager.AddPerk(killsThresholdPerks[currentEvolution.Value]);
                //ResetCounters();
            }
        }
    }
    protected override bool PerkActivationConditional()
    {
        return true;
    }
    protected override void ApplyPerk()
    {
        if (!IsServer) return;

        if (shipManager != null)
        {
            if (details != null && currentEvolution.Value < maxEvolutions)
            {
                if(details.damageTaken != null)
                    damageTaken.Value += details.damageTaken.damageValue;
                if (details.damageDealt != null)
                    damageDealt.Value += details.damageDealt.damageValue;
                if (details.kill > 0)
                    kills.Value += details.kill;

                /*
                if(currentEvolution.Value < maxEvolutions && damageTaken.Value >= damageTakenThreshold)
                {
                    AddEvolutionPerkServerRpc(0);
                    ResetCounters();
                }
                if(currentEvolution.Value < maxEvolutions && damageDealt.Value >= damageDealtThreshold)
                {
                    AddEvolutionPerkServerRpc(1);
                    //shipManager.shipPerkManager.AddPerk(damageDealtThresholdPerks[currentEvolution.Value]);
                    ResetCounters();
                }
                if(currentEvolution.Value < maxEvolutions && kills.Value >= killsThreshold)
                {
                    AddEvolutionPerkServerRpc(2);
                    //shipManager.shipPerkManager.AddPerk(killsThresholdPerks[currentEvolution.Value]);
                    ResetCounters();
                }
                */
                
            }
            //ReceiveUpdateEvolutionUiClientRpc();
            ResetTriggeredBool();
        }
        else
        {
            Debug.Log("null ship manager");
        }
    }
    [ServerRpc]
    public void UpdateEvolutionUiServerRpc()
    {
        if (!IsServer) return;

        shipManager.playerUiControl.UpdateEvolutionUi(this);

        ReceiveUpdateEvolutionUiClientRpc();
    }
    [ClientRpc]
    public void ReceiveUpdateEvolutionUiClientRpc()
    {
        if (shipManager != null && shipManager.playerUiControl != null)
        {
            shipManager.playerUiControl.UpdateEvolutionUi(this);
        }
    }

    [ServerRpc]
    public void AddEvolutionPerkServerRpc(int evolutionType, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        ulong callerId = rpcParams.Receive.SenderClientId;

        switch (evolutionType)
        {
            case 0:
                shipManager.shipPerkManager.AddPerk(damageTakenThresholdPerks[currentEvolution.Value], callerId);
                break;
            case 1:
                shipManager.shipPerkManager.AddPerk(damageDealtThresholdPerks[currentEvolution.Value], callerId);
                break;
            case 2:
                shipManager.shipPerkManager.AddPerk(killsThresholdPerks[currentEvolution.Value], callerId);
                break;
        }
        ResetCounters();
        shipManager.shipPerkManager.SpawnPerksNonRpc();
    }
    void ResetCounters()
    {
        currentEvolution.Value++;
        damageTaken.Value = 0;
        damageDealt.Value = 0;
        kills.Value = 0;
    }
    public bool IsEvolutionDone()
    {
        if (currentEvolution.Value >= maxEvolutions)
            return true;
        return false;
    }
    protected override void RemovePerk()
    {

    }
}
