using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class UnitListing    //TODO: move to a different class
{
    public UnitListing()
    {
        ownerID = 0;
        unitID = -1;    //ID points to an array of all units
        //deck to assign to unit
        //any changes to stats like missing health etc. if we want to carry that over between battles
    }

    public UnitListing(int _ownerID, int _unitID)
    {
        ownerID = _ownerID;
        unitID = _unitID;
    }

    public int ownerID;
    public int unitID;
}

public class PlayerInfo : MonoBehaviour {

    public int playerID;
    public NetworkCommands commands;

    public UnitListing[] team = new UnitListing[10];
    public List<UnitListing> localTeam = new List<UnitListing>();
    public int teamCapacity = 8;
    public int localTeamCapacity = 2;

    private void Start()
    {
        commands = FindObjectOfType<NetworkCommands>();
        commands.playerInfo = this;

        if (playerID != 0) GetComponent<PlayerInfo>().commands.CmdRequestTeamList();
        team.Initialize();
        Debug.Log(localTeam.Count);
        Debug.Log(team.Length);
        Debug.Log("Player " + playerID + " joined.");
    }

    public void AddTeamMember(int unitID)
    {
        if (localTeam.Count == localTeamCapacity)
        {
            Debug.Log("Too many units!");
            return;
        }
        localTeam.Add(new UnitListing(playerID, unitID));
    }

    public void RemoveFromTeam()
    {
        if (localTeam.Count == 0)
        {
            Debug.Log("No units to remove!");
            return;
        }
        localTeam.RemoveAt(localTeam.Count - 1);
    }

    public void ChangeTeam(UnitListing[] teamList)   //updated based on host
    {
        team = teamList;
        //display full team based on all players local teams
    }
}
