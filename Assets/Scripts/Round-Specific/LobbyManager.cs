using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public List<ClientData> clients = new List<ClientData>();
    public GameObject[] clientPrefs = new GameObject[1];

    public float matchDuration = 300f;
    public float preMatchDuration = 20f;
    public float postMatchDuration = 10f;
    public int maxScore = 20;

    public float maxRadiusSpawn = 5;
    public Vector2 teamASpawnPoint = Vector2.zero;
    public Vector2 teamBSpawnPoint = Vector2.zero;

    public EntitySpawner entitySpawner;

    public NetworkVariable<int> teamAScore = new NetworkVariable<int>(0);
    public NetworkVariable<int> teamBScore = new NetworkVariable<int>(0);

    public NetworkVariable<float> matchTimer = new NetworkVariable<float>(0f);
    public NetworkVariable<float> prematchTimer = new NetworkVariable<float>(0f);
    public NetworkVariable<float> postMatchTimer = new NetworkVariable<float>(0f);
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        clients.Clear();

        matchTimer.Value = matchDuration;
        prematchTimer.Value = preMatchDuration;
        postMatchTimer.Value = postMatchDuration;

        //InvokeRepeating(nameof(UpdateMatchTimer), 1f, 1f);
        InvokeRepeating(nameof(UpdatePreMatchTimer), 1f, 1f);
        GetComponent<ScoreBoard>().enabled = true;
    }
    private void UpdatePreMatchTimer()
    {
        if (!IsServer) return;

        prematchTimer.Value -= 1f;

        if (prematchTimer.Value <= 0f)
        {
            InvokeRepeating(nameof(UpdateMatchTimer), 1f, 1f);
            entitySpawner.SpawnEnemies();
            CancelInvoke(nameof(UpdatePreMatchTimer));
        }
    }
    private void UpdateMatchTimer()
    {
        if (!IsServer) return;

        matchTimer.Value -= 1f;
        SyncTimerClientRpc(matchTimer.Value);

        if (matchTimer.Value <= 0f)
        {
            InvokeRepeating(nameof(UpdatePostMatchTimer), 1f, 1f);
            CancelInvoke(nameof(UpdateMatchTimer));
        }
    }
    private void UpdatePostMatchTimer()
    {
        if (!IsServer) return;

        postMatchTimer.Value -= 1f;
        //SyncTimerClientRpc(postMatchTimer.Value);

        if (postMatchTimer.Value <= 0f)
        {
            EndMatch();
        }
    }
    [ClientRpc]
    private void SyncTimerClientRpc(float timeRemaining)
    {
        //matchTimer.Value = timeRemaining;
    }

    private void EndMatch()
    {
        Debug.Log("Match ended! Disconnecting players...");

        //foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        //{
        //    NetworkManager.Singleton.DisconnectClient(client.ClientId);
        //}

        NetworkUI ui_control = GameObject.FindWithTag("GameController").GetComponent<NetworkUI>();
        CancelInvoke(nameof(UpdateMatchTimer));
        NotifyClientsMatchEndedClientRpc();

        NetworkManager.Singleton.Shutdown();

    }
    [ClientRpc]
    private void NotifyClientsMatchEndedClientRpc()
    {
        NetworkUI ui_control = GameObject.FindWithTag("GameController").GetComponent<NetworkUI>();
        if (ui_control != null)
        {
            ui_control.uiParent.SetActive(true);
        }
    }
    private ulong[] GetClientIds()
    {
        List<ulong> clientIds = new List<ulong>();

        foreach (ClientData client in clients)
        {
            if (client.TryGetComponent(out NetworkObject networkObject))
            {
                clientIds.Add(networkObject.OwnerClientId);
            }
        }
        return clientIds.ToArray();
    }

    public bool CanUseName(string name)
    {
        foreach(ClientData client in clients)
        {
            if (client.name == name)
                return false;
        }
        return true;
    }
    public bool CanStart()
    {
        if (prematchTimer.Value > 0)
            return false;
        return true;
    }
    public void IncrementTeamScore(ClientData attacker)
    {
        if (!IsServer) return;

        if (matchTimer.Value > 0)
        {
            if (attacker.clientTeam.Value == "Team A")
                teamAScore.Value += 1;
            if (attacker.clientTeam.Value == "Team B")
                teamBScore.Value += 1;

        }
    }
    public string GetWinningTeam()
    {
        if (teamAScore.Value > teamBScore.Value)
            return "Team A";
        if (teamAScore.Value < teamBScore.Value)
            return "Team B";
        return "Tie";
    }
    [ServerRpc(RequireOwnership = false)]
    public void RegisterClientServerRpc(NetworkObjectReference clientObjectRef)
    {
        if (!IsServer) return;

        if (clientObjectRef.TryGet(out NetworkObject clientObject))
        {
            ClientData clientData = clientObject.GetComponent<ClientData>();
            if (clientData != null && !clients.Contains(clientData))
            {
                clients.Add(clientData);
                Debug.Log($"Client {clientData.clientName} joined the lobby!");
            }
        }

        //UpdateClientsClientRpc(clientObjectRef);
        SyncClientsListClientRpc(GetClientIds());

    }

    [ClientRpc]
    private void SyncClientsListClientRpc(ulong[] clientIds)
    {
        clients.Clear();

        foreach (ulong clientId in clientIds)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                ClientData clientData = client.PlayerObject.GetComponent<ClientData>();
                if (clientData != null)
                {
                    clients.Add(clientData);
                }
            }
        }
    }


    [ClientRpc]
    private void UpdateClientsClientRpc(NetworkObjectReference clientObjectRef)
    {
        if (clientObjectRef.TryGet(out NetworkObject clientObject))
        {
            ClientData clientData = clientObject.GetComponent<ClientData>();
            Debug.Log($"Client {clientData.clientName} joined the lobby!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnClientShipServerRpc(int shipId, NetworkObjectReference clientObjectRef, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        ulong ownerId = rpcParams.Receive.SenderClientId;
        Vector2 spawnPoint = Vector2.zero;
        if (clientObjectRef.TryGet(out NetworkObject clientObject))
        {
            ClientData clientData = clientObject.GetComponent<ClientData>();
            if(clientData.clientTeam.Value == "Team A")
            {
                spawnPoint = Random.insideUnitCircle * Random.Range(0, maxRadiusSpawn) + teamASpawnPoint;
            }
            if (clientData.clientTeam.Value == "Team B")
            {
                spawnPoint = Random.insideUnitCircle * Random.Range(0, maxRadiusSpawn) + teamBSpawnPoint;
            }
        }

        GameObject clone = Instantiate(clientPrefs[shipId], spawnPoint, Quaternion.identity);
        NetworkObject networkObject = clone.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(ownerId);
    }
}
