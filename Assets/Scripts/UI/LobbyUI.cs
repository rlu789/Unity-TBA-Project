using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//TODO: a lot of this is hard-coded and not scalable but its just for testing, will change once we have a better idea of the system we want to make
public class LobbyUI : MonoBehaviour {

    public List<Text> unitList = new List<Text>();

    public PlayerInfo playerInfo;

    //for buttons
    public void AddUnit(int index)
    {
        if (playerInfo == null) return;
        playerInfo.AddTeamMember(index);
        UpdateTextFields();
    }

    public void RemoveUnit()
    {
        if (playerInfo == null) return;
        playerInfo.RemoveFromTeam();
        UpdateTextFields();
    }

    public void ReadyUp()
    {
        if (playerInfo == null) return;
        if (playerInfo.localTeam.Count != 2) Debug.Log("Select two units");
        else
        {
            playerInfo.commands.CmdLobbyReady(playerInfo.localTeam.ToArray());
            gameObject.SetActive(false);
        }
    }

    public void StartGame()
    {
        if (playerInfo == null) return;
        if (playerInfo.playerID != 0)
        {
            Debug.Log("Only the host can start the game!");
            return;
        }

        for (int i = 0; i < Network.connections.Length + 1; ++i)
        {
            if (playerInfo.team[(i * 2) + 1].unitID == -1 || playerInfo.team[i * 2].unitID == -1)   //if a players preset slot is -1 (uninitialized) dont start the game
            {
                Debug.Log("Players not ready!");
                return;
            }
        }

        playerInfo.commands.CmdLobbyStartGame();
    }

    void UpdateTextFields() //clear old text and update 
    {
        for (int i = 0; i < unitList.Count; ++i)
        {
            unitList[i].text = "";
        }
        for (int i = 0; i < playerInfo.localTeam.Count; ++i)
        {
            unitList[i].text = "Unit " + playerInfo.localTeam[i].unitID + ". Player " + playerInfo.localTeam[i].ownerID;
        }
    }
}