using UnityEngine.Networking;
using UnityEngine;  //Debug.Log()
using UnityEngine.SceneManagement;

public class NetworkCommands : NetworkBehaviour
{
    public PlayerInfo playerInfo;

    private void Start()
    {
        Debug.Log("A network commands was created");
    }

    #region Lobby/Team list
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
        if (playerInfo == null) playerInfo = GetComponent<PlayerInfo>();
        CmdSendTeamList(playerInfo.team);
    }

    [Command]
    public void CmdSendTeamList(UnitListing[] teamList) //everyone but the host receives the team list
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
    void TargetRpcUpdateTeamList(NetworkConnection target, UnitListing[] teamList)
    {
        playerInfo.ChangeTeam(teamList);
    }

    [Command]
    public void CmdLobbyReady(UnitListing[] localTeam)  //send other players your local team
    {
        RpcLobbyReady(localTeam);
    }

    [ClientRpc]
    public void RpcLobbyReady(UnitListing[] localTeam) //set the team units at a preset location for the owner
    {
        if (playerInfo == null) playerInfo = GetComponent<PlayerInfo>();
        int ownerID = localTeam[0].ownerID; //get the owner ID from one of the sent units
        playerInfo.team[ownerID * 2] = localTeam[0];
        playerInfo.team[(ownerID * 2) + 1] = localTeam[1];
        Debug.Log("Received units. " + playerInfo.team[ownerID * 2].ownerID + " / " + playerInfo.team[ownerID * 2].unitID);
    }

    [Command]
    public void CmdLobbyStartGame()
    {
        RpcLobbyStartGame();
    }

    [ClientRpc]
    public void RpcLobbyStartGame()
    {
        foreach (UnitListing u in playerInfo.team)
        {
            Debug.Log(u.ownerID + " (owner) / (unit) " + u.unitID);
        }
        SceneManager.LoadScene(1);
    }
    #endregion
}
