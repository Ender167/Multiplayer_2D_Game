using Unity.Netcode;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    public GameObject uiParent;
    public GameObject lobbyManager;

    private void Awake()
    {
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            GameObject clone = Instantiate(lobbyManager);
            clone.GetComponent<NetworkObject>().Spawn();
        };
        NetworkManager.Singleton.OnServerStopped += (wasHost) =>
        {
            Debug.Log("Server has stopped. Was Host: " + wasHost);

            if (NetworkManager.Singleton.IsServer)
            {
                NotifyClientsOfServerStopClientRpc(wasHost);
            }

            if (wasHost || NetworkManager.Singleton.IsClient)
            {
                uiParent.SetActive(true);
            }
        };
    }
    [ClientRpc]
    private void NotifyClientsOfServerStopClientRpc(bool wasHost)
    {
        uiParent.SetActive(true);
    }
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        uiParent.SetActive(false);
    }
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        uiParent.SetActive(false);
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        uiParent.SetActive(false);
    } 
}