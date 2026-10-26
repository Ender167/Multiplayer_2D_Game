using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class ClientData : NetworkBehaviour
{
    [Header("General")]
    [SerializeField] public NetworkVariable<FixedString128Bytes> clientName = new NetworkVariable<FixedString128Bytes>();
    [SerializeField] public NetworkVariable<FixedString128Bytes> clientTeam = new NetworkVariable<FixedString128Bytes>();

    [SerializeField] private string clientNameStr = "";
    [SerializeField] private string clientTeamStr = "";

    bool added = false;
    float counter = 0;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) return;

        if(LobbyManager.Instance != null)
        {
            added = true;
            LobbyManager.Instance.RegisterClientServerRpc(NetworkObject);
        }
        
    }
    private void Update()
    {
        

        if(!added)
        {
            if (!IsOwner) return;

            if (counter < 0.1f)
            {
                counter += Time.deltaTime;
            }
            if(counter >= 0.1f)
            {
                added = true;
                LobbyManager.Instance.RegisterClientServerRpc(NetworkObject);
            }
        }
        else
        {
            clientNameStr = clientName.Value.ToString();
            clientTeamStr = clientTeam.Value.ToString();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetNameOnServerServerRpc(string name)
    {
        clientName.Value = name;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTeamOnServerServerRpc(string team)
    {
        clientTeam.Value = team;
    }

    public void SetName(string name)
    {
        if (IsOwner)
        {
            SetNameOnServerServerRpc(name);
        }
    }

    public void SetTeam(string team)
    {
        if (IsOwner)
        {
            SetTeamOnServerServerRpc(team);
        }
    }
}
