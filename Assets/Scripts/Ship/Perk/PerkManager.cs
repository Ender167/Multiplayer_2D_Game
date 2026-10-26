using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[System.Serializable]
public class PerkManager : NetworkBehaviour
{
    public PerkEffectType typeOfPerks;
    public List<PerkPair> perks = new List<PerkPair>();

    ShipManager shipManager;
    [SerializeField] private List<Perk> instantiatedPerks = new List<Perk>();

    bool cleanup = false;
    private void Start()
    {
        shipManager = GetComponent<ShipManager>();
        InitPerks();
    }
    private void Update()
    {
        if (!IsServer) return;

        if (!cleanup)
        {
            SendEventMessage(PerkEventType.AlwaysOn, null);
            //ValidatePerks();
        }
    }
    void InitPerks()
    {
        if (!IsOwner) return;

        SpawnPerksServerRpc();

    }

    void SendEventMessage(PerkEventType t, EnemyDetails det)
    {
        for (int i = 0; i < perks.Count; i++)
        {
            if(perks[i].perkInstance != null)
                perks[i].perkInstance.ReceivePerkEvent(t, det);
        }
    }
    public void ValidatePerks()
    {
        if (!IsOwner) return;

        for (int i = 0; i < perks.Count; i++)
        {
            if (perks[i].perkInstance == null)
            {
                //Perk perkCopy = Instantiate(perks[i].perkPref, Vector3.zero, Quaternion.identity);
                //perkCopy.GetComponent<NetworkObject>().Spawn();
                //perks[i].perkInstance = perkCopy;
                SpawnAdditionalPerksServerRpc(i);
            }
            else
            {
                if (perks[i].perkInstance.HasReachedMaxTriggers())
                {
                    RemovePerk(perks[i].perkPref);
                }
            }
        }
    }
    void Cleanup()
    {
        foreach (PerkPair perk in perks)
        {
            if (perk.perkInstance != null)
            {
                perk.perkInstance.GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    public void ReceiveEventFromEnemy(PerkEventType t, EnemyDetails det)
    {
        SendEventMessage(t, det);
    }
    public void ReceiveEventFromEnemy(PerkEventType t)
    {
        SendEventMessage(t, null);
    }
    public void RemovePerk(Perk p)
    {
        if (!IsServer) return;

        for(int i = 0; i < perks.Count; i++)
        {
            if(perks[i].perkPref == p)
            {
                if (perks[i].perkInstance != null)
                {
                    perks[i].perkInstance.RemoveActivePerk();
                    perks[i].perkInstance.GetComponent<NetworkObject>().Despawn();
                    //Destroy(perks[i].perkInstance);
                    perks[i].perkInstance = null;
                }
                perks.RemoveAt(i);
            }
        }
    }
    public void RemovePerks(List<Perk> pToRemove)
    {
        for (int i = 0; i < perks.Count; i++)
        {
            if (pToRemove.Contains(perks[i].perkPref))
            {
                if (perks[i].perkInstance != null)
                {
                    perks[i].perkInstance.RemoveActivePerk();
                    Destroy(perks[i].perkInstance);
                    perks[i].perkInstance = null;
                }
                perks.RemoveAt(i);
            }
        }
    }
    public void AddPerk(Perk p, ulong owner)
    {
        perks.Add(new PerkPair(p, null, owner));
    }
    public void AddPerks(List<Perk> newPerks, ulong owner)
    {
        foreach(Perk p in newPerks)
        {
            perks.Add(new PerkPair(p, null, owner));
        }
    }

    public void SpawnPerksNonRpc()
    {
        if (!IsServer) return;

        for (int i = 0; i < perks.Count; i++)
        {
            if (perks[i].perkInstance == null)
            {
                Perk perkCopy = Instantiate(perks[i].perkPref, Vector3.zero, Quaternion.identity);
                NetworkObject networkObject = perkCopy.GetComponent<NetworkObject>();
                networkObject.Spawn();

                perks[i].perkInstance = perkCopy;
                perkCopy.Init(shipManager);

                networkObject.ChangeOwnership(perks[i].ownerId);

                InitPerkClientRpc(networkObject.NetworkObjectId);
            }
        }
    }
    [ServerRpc]
    public void SpawnPerksServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        ulong callerId = rpcParams.Receive.SenderClientId;

        for (int i = 0; i < perks.Count; i++)
        {
            if (perks[i].perkInstance == null)
            {
                Perk perkCopy = Instantiate(perks[i].perkPref, Vector3.zero, Quaternion.identity);
                NetworkObject networkObject = perkCopy.GetComponent<NetworkObject>();
                networkObject.Spawn();

                perks[i].perkInstance = perkCopy;
                perkCopy.Init(shipManager);

                networkObject.ChangeOwnership(callerId);

                InitPerkClientRpc(networkObject.NetworkObjectId);
            }
        }

    }

    [ServerRpc]
    public void SpawnAdditionalPerksServerRpc(int perkId)
    {
        if (!IsServer) return;

        Perk perkCopy = Instantiate(perks[perkId].perkPref, Vector3.zero, Quaternion.identity);
        perkCopy.GetComponent<NetworkObject>().Spawn();
        perks[perkId].perkInstance = perkCopy;

        InitPerkClientRpc(perkCopy.GetComponent<NetworkObject>().NetworkObjectId);
    }

    [ClientRpc]
    void InitPerkClientRpc(ulong networkObjectId)
    {
        NetworkObject netObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];

        if (netObj != null)
        {
            Perk perkInstance = netObj.GetComponent<Perk>();
            if (perkInstance != null)
            {
                perkInstance.Init(shipManager);
            }
        }
    }


    public override void OnDestroy()
    {
        if (!IsServer) return;
        cleanup = true;
        Cleanup();
    }
}

[System.Serializable]
public class PerkPair
{
    public Perk perkPref;
    public Perk perkInstance;
    public ulong ownerId;
    
    public PerkPair(Perk pref, Perk inst, ulong owner)
    {
        perkPref = pref;
        perkInstance = inst;
        ownerId = owner;
    }
}