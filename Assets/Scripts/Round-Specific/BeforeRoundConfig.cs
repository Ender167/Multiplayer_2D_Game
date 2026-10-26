using UnityEngine;
using Unity.Netcode;
using TMPro;

public class BeforeRoundConfig : NetworkBehaviour
{

    public GameObject camObj;
    [Header("UI")]
    public GameObject uiParent;
    public TMP_InputField clientName;
    public TMP_Dropdown teamDropdown;
    public TMP_Dropdown shipDropdown;

    ClientData clientData;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        clientData = GetComponent<ClientData>();

        if (!IsOwner) return;

        SetClientUiStatus(true);

    }
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }


    public void StartClient()
    {
        string name = clientName.text.ToString();
        
        if(LobbyManager.Instance.CanUseName(name) && LobbyManager.Instance.CanStart())
        {
            clientData.SetName(name);
            clientData.SetTeam(teamDropdown.options[teamDropdown.value].text);
            SetClientUiStatus(false);
            LobbyManager.Instance.SpawnClientShipServerRpc(shipDropdown.value, GetComponent<NetworkObject>());
            
        }
    }
    public void SetClientUiStatus(bool status)
    {
        camObj.GetComponent<Camera>().enabled = status;
        camObj.GetComponent<AudioListener>().enabled = status;
        uiParent.SetActive(status);
    }
}
