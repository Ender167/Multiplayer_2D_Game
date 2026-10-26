using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ScoreBoard : NetworkBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI finalResultText;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(LobbyManager.Instance.prematchTimer.Value > 0)
        {
            scoreText.text = "Time until match start " + LobbyManager.Instance.prematchTimer.Value.ToString();
        }
        else
        {
            scoreText.text = "A " + LobbyManager.Instance.teamAScore.Value.ToString() + "|" + LobbyManager.Instance.teamBScore.Value.ToString() + " B" + '\n' + LobbyManager.Instance.matchTimer.Value.ToString();
        }

        if(LobbyManager.Instance.matchTimer.Value > 0)
        {
            finalResultText.text = "";
        }
        else
        {
            string result = LobbyManager.Instance.GetWinningTeam();
            if(result != "Tie")
            {
                finalResultText.text = "Winner: " + result;
            }
            else
            {
                finalResultText.text = result;
            }
        }
    }
}
