using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TeamListing    //TODO: move to a different class
{
    public TeamListing()
    {
        ownerID = 0;
        unitID = -1;
    }

    public TeamListing(int _ownerID, int _unitID)
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

    public TeamListing[] team = new TeamListing[10];
    public List<TeamListing> localTeam = new List<TeamListing>();
    public int teamCapacity = 8;
    public int localTeamCapacity = 2;

    private void Start()
    {
        commands = FindObjectOfType<NetworkCommands>();
        Debug.Log(commands);

        Debug.Log("Player " + playerID + " joined.");
    }

    public void AddTeamMember(int unitID)
    {
        if (localTeam.Count == localTeamCapacity)
        {
            Debug.Log("Too many units!");
            return;
        }
        localTeam.Add(new TeamListing(playerID, unitID));
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

    public void ChangeTeam(TeamListing[] teamList)   //updated based on host
    {
        team = teamList;
        //display full team based on all players local teams
    }
}
