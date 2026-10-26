using UnityEngine;
using Unity.Netcode;
using TMPro;

public class UIControl : NetworkBehaviour
{

    public GameObject uiRoot;
    public GameObject uiName;
    public GameObject uiEvolution;

    public TextMeshProUGUI healthText;
    public TextMeshProUGUI nameText;

    
    public ShipManager shipManager;

    [Header("Evolution")]
    public TextMeshProUGUI damageTakenText;
    public TextMeshProUGUI damageDealtText;
    public TextMeshProUGUI killsText;

    bool executed = false;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Init();

    }
    private void Start()
    {
        Init();
    }

    void Init()
    {
        if (!executed)
        {
            if (!IsOwner)
            {
                uiName.SetActive(true);
                executed = true;
                return;
            }

            if (!uiRoot.activeSelf)
            {
                uiRoot.SetActive(true);
                uiName.SetActive(false);

                healthText.GetComponent<RectTransform>().anchoredPosition = new Vector3(healthText.GetComponent<RectTransform>().anchoredPosition.x, 100, 0);
            }
            executed = true;
        }
    }


    void Update()
    {
        if (!IsSpawned) return;

        ClientData clientData = shipManager.FetchUserClientData();
        ClientData localClientData = shipManager.FetchLocalClientData();

        if (clientData != null)
        {
            string clientTeam = clientData.clientTeam.Value.ToString();
            string localClientTeam = localClientData.clientTeam.Value.ToString();

            if(clientTeam == localClientTeam)
            {
                nameText.color = Color.green;
                nameText.text = clientData.clientName.Value.ToString() + " - " + clientData.clientTeam.Value.ToString();
            }
            else
            {
                nameText.color = Color.red;
                nameText.text = clientData.clientName.Value.ToString() + " - " + clientData.clientTeam.Value.ToString();
            }
        }

        if (!IsOwner) return;

        healthText.text = shipManager.GetEnergy().ToString() + '\n' + shipManager.GetShield().ToString() + '\n' + shipManager.GetHealth().ToString();
        
    }
    public void EnableEvolutionUi()
    {
        if (!IsOwner) return;
        if(uiEvolution != null)
        {
            uiEvolution.SetActive(true);
        }
    }
    public void UpdateEvolutionUi(EvolutionPerk evolutionPerk)
    {
        //if (!IsOwner) return;
        if (evolutionPerk.IsEvolutionDone())
        {
            uiEvolution.SetActive(false);
        }
        if (uiEvolution != null && uiEvolution.activeSelf)
        {
            damageDealtText.text = "Damage dealt: " + evolutionPerk.damageDealt.Value.ToString() + "/" + evolutionPerk.damageDealtThreshold.ToString();
            damageTakenText.text = "Damage taken: " + evolutionPerk.damageTaken.Value.ToString() + "/" + evolutionPerk.damageTakenThreshold.ToString();
            killsText.text = "Kills: " + evolutionPerk.kills.Value.ToString() + "/" + evolutionPerk.killsThreshold.ToString();
        }
    }
}
