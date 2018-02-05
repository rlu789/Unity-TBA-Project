using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkCommands : NetworkBehaviour
{
    PlayerInfo playerInfo;

    private void Start()
    {
        playerInfo = GetComponent<PlayerInfo>();
    }

    #region Lobby team list
    [Command]
    public void CmdRequestTeamList()    //request team list
    {
        foreach (NetworkConnection target in NetworkServer.connections)
        {
            if (target.connectionId == 0)
            {
                TargetRpcSendTeamList(target);
            }
        }
    }

    [TargetRpc]
    void TargetRpcSendTeamList(NetworkConnection target)    //host sends the team list
    {
        CmdSendTeamList(playerInfo.team);
    }

    [Command]
    public void CmdSendTeamList(TeamListing[] teamList) //everyone but the host receives the team list
    {
        foreach (NetworkConnection target in NetworkServer.connections)
        {
            if (target.connectionId != 0)
            {
                TargetRpcUpdateTeamList(target, teamList);
            }
        }
    }

    [TargetRpc]
    void TargetRpcUpdateTeamList(NetworkConnection target, TeamListing[] teamList)
    {
        playerInfo.ChangeTeam(teamList);
    }

    [Command]
    public void CmdLobbyReady(TeamListing[] localTeam)  //send other players your local team
    {
        RpcLobbyReady(localTeam);
    }

    [ClientRpc]
    public void RpcLobbyReady(TeamListing[] localTeam) //set the team units at a preset location for the owner
    {
        int ownerID = localTeam[0].ownerID; //get the owner ID from one of the sent units
        playerInfo.team[ownerID * 2] = localTeam[0];
        playerInfo.team[(ownerID * 2) + 1] = localTeam[1];
    }

    [Command]
    public void CmdLobbyStartGame()
    {
        SceneManager.LoadScene(1);
    }
    #endregion
}
