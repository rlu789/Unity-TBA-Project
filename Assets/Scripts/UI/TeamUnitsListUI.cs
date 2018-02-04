using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamUnitsListUI : MonoBehaviour {

    public List<Text> unitList = new List<Text>();

    PlayerInfo player;

    private void Update()   //very bad just for testing
    {
        if (player != null)
        {
            if (player.localTeam.Count >= 1) unitList[0].text = "Unit " + player.localTeam[0].unitID + ". Player " + player.localTeam[0].ownerID;
            else unitList[0].text = "";
            if (player.localTeam.Count >= 2) unitList[1].text = "Unit " + player.localTeam[1].unitID + ". Player " + player.localTeam[1].ownerID;
            else unitList[1].text = "";
        }
    }
    //for buttons
    public void AddUnit(int index)
    {
        if (player == null) player = FindObjectOfType<PlayerInfo>();
        if (player != null) player.AddTeamMember(index);
    }

    public void RemoveUnit()
    {
        if (player == null) player = FindObjectOfType<PlayerInfo>();
        if (player != null) player.RemoveFromTeam();
    }
}